using System.Collections.Generic;
using UnityEngine;
using TRavljen.UnitSystem.Garrison;
using TRavljen.UnitSystem.Utility;
using UnityEngine.Events;

namespace TRavljen.UnitSystem
{

    /// <summary>
    /// Delegate that allows implementation to prevent production of certain units
    /// when they reached the very end of production.
    /// <para>
    /// Example: This may be used if some
    /// resources like population needs to be above 0 before spawning/producing
    /// a unit is possible.
    /// </para>
    /// </summary>
    public interface IActiveProductionDelegate : IProductionQueueDelegate { }

    /// <summary>
    /// Feature rich component for active production of any <see cref="AProducibleSO"/>.
    /// Handles queue of production requests, supports cancelation, garrison produced units,
    /// spawning produced units, collect right away or stash produced items before player collects.
    /// </summary>
    [DisallowMultipleComponent]
    public class ActiveProduction : AEntityComponent, IActiveProduction, IProductionQueueDelegate
    {

        #region Properties

        /// <summary>
        /// Specifies behaviour for produced units. If this is set to `true`
        /// then <see cref="IGarrisonEntity"/> (if present) will be used to
        /// garrison units after production.
        /// </summary>
        [Tooltip("Set this to true if unit should garrison produced units. " +
            "Which means that they will need to be manually spawned." +
            "Garrison maximal capacity is still used.")]
        public bool GarrisonUnitsAfterProduction = false;

        /// <summary>
        /// Specifies if the producibles that are not units, get stashed in
        /// the produced stash to be collected manually. If this is disabled
        /// produced producibles (like resources) will be added to the player
        /// immediately.
        /// </summary>
        [Tooltip("Specifies if the producibles that are not units get stashed" +
            "in the unit to be collected manually. If this is disabled then" +
            "producibles (like resources) will be added to the player immediately " +
            "after production.")]
        [SerializeField]
        private bool stashProduction = false;

        [Tooltip("If stash production is enabled, this will enable grouping of " +
            "those stashed producibles by accumulating their quantities.")]
        [SerializeField]
        private bool groupStashedQuantity = false;

        [Tooltip("Currently produced stash.")]
        [SerializeField]
        private List<ProducibleQuantity> producedStash = new();

        [Tooltip("Event invoked when first production request is scheduled.")]
        public UnityEvent OnProductionStarted = new();
        
        [Tooltip("Event invoked when last production request is finished.")]
        public UnityEvent OnProductionFinished = new();
        
        /// <summary>
        /// Queue for producing units, resources, etc.
        /// </summary>
        private readonly ProductionQueue ProductionQueue = new();

        /// <summary>
        /// Current production delegate.
        /// </summary>
        public IActiveProductionDelegate Delegate { get; set; }

        /// <summary>
        /// Currently stashed producible.
        /// </summary>
        public ProducibleQuantity[] ProducedStash => producedStash.ToArray();

        public float CurrentProductionProgress
            => ProductionQueue.CurrentProductionProgress;

        #endregion

        #region Lifecycle

        private void OnEnable()
        {
            ProductionQueue.OnProductionFinished += HandleFinishedProduction;
            ProductionQueue.Delegate = this;
        }

        private void OnDisable()
        {
            ProductionQueue.OnProductionFinished -= HandleFinishedProduction;
            ProductionQueue.Delegate = null;
        }

        #endregion

        #region Stash

        /// <summary>
        /// Clears produced stash and returns it. This method does not apply
        /// the produced stash to player itself.
        /// </summary>
        public ProducibleQuantity[] CollectedProducedStash()
        {
            ProducibleQuantity[] stash = producedStash.ToArray();
            producedStash.Clear();
            return stash;
        }

        /// <summary>
        /// Collects stashed production on specified index.
        /// </summary>
        /// <param name="index">Index on which to collect from</param>
        /// <param name="collected">Collected producible quantity</param>
        /// <returns>Returns true if collecting was successful.</returns>
        public bool CollectFromProducedStash(int index, out ProducibleQuantity collected)
        {
            if (index < producedStash.Count)
            {
                collected = default;
                return false;
            }

            collected = producedStash[index];
            producedStash.RemoveAt(index);
            return true;
        }

        /// <summary>
        /// Collects stashed production for requested producible.
        /// </summary>
        /// <param name="requested">Producible to collect</param>
        /// <param name="collected">Collected producible quantity</param>
        /// <returns>Returns true if collecting was successful.</returns>
        public bool CollectFromProducedStash(AProducibleSO requested, out ProducibleQuantity collected)
        {
            for (int index = 0; index < producedStash.Count; index++)
            {
                if (producedStash[index].Producible.ID == requested.ID)
                {
                    collected = producedStash[index];
                    producedStash.RemoveAt(index);
                    return true;
                }
            }

            collected = default;
            return false;
        }

        /// <summary>
        /// Collects stashed production for requested producible and it's quantity.
        /// Collecting will be successful even if it was not collected in full.
        /// </summary>
        /// <param name="requested">Requested producible and it's quantity</param>
        /// <param name="collected">Collected producible quantity</param>
        /// <returns>
        /// Returns true if collecting was successful. If requested quantity was
        /// 5 but produced stash was 4, it will retrieve the 4 and output it.
        /// </returns>
        public bool CollectFromProducedStash(ProducibleQuantity requested, out ProducibleQuantity collected)
        {
            for (int index = 0; index < producedStash.Count; index++)
            {
                ProducibleQuantity currentStash = producedStash[index];

                if (currentStash.Producible.ID == requested.Producible.ID)
                {
                    // Check if there is less or equal than requested
                    if (currentStash.Quantity <= requested.Quantity)
                    {
                        // Partially collected the requested amount, but stash is
                        // now depleted. Remove after collecting.
                        collected = currentStash;
                        producedStash.RemoveAt(index);
                        return true;
                    }
                    else
                    {
                        currentStash.Quantity -= requested.Quantity;
                        currentStash.Quantity = MathUtils.Max(0, currentStash.Quantity);
                        producedStash[index] = currentStash;
                        // Collected in full.
                        collected = requested;
                        return true;
                    }
                }
            }

            collected = default;
            return false;
        }

        #endregion

        #region Production

        /// <summary>
        /// Queues production order on the unit and if set, splits it into multiple
        /// production orders.
        /// </summary>
        /// <param name="productionQuantity">Production quantity to be queued</param>
        /// <param name="queueMultipleOrders">Split production quantity into multiple orders</param>
        public void StartProduction(ProducibleQuantity productionQuantity, bool queueMultipleOrders = false)
        {
            StartProduction(
                productionQuantity.Producible,
                productionQuantity.Quantity,
                queueMultipleOrders);
        }

        /// <summary>
        /// Queues the production of producible with quantity or splits it into
        /// multiple productions.
        /// </summary>
        /// <param name="producible">Producible to be produced</param>
        /// <param name="quantity">Number of producibles</param>
        /// <param name="queueMultipleOrders">Split production quantity into multiple orders</param>
        public void StartProduction(AProducibleSO producible, long quantity, bool queueMultipleOrders = false)
        {
            // Invoke an event for first production request
            if (ProductionQueue.QueueSize == 0)
                OnProductionStarted.Invoke();
            
            ProductionQueue.AddProductionOrder(producible, quantity, queueMultipleOrders);
            ProductionEvents.Instance.OnNewProductionScheduled.Invoke(this, producible, quantity);
        }

        /// <summary>
        /// Convenience method to cancel all production orders. Specific production
        /// can be cancelled on <see cref="ProductionQueue"/> directly.
        /// </summary>
        /// <return>All the accumulating resources freed duo to cancalled productions.</return>
        public ResourceQuantity[] CancelProduction()
        {
            ResourceQuantity[] returnedResources = ProductionQueue.CancelProduction();
            ProductionEvents.Instance.OnAllProductionCancelled.Invoke(this);
            return returnedResources;
        }

        /// <summary>
        /// Cancels the first production with specified producible.
        /// </summary>
        /// <param name="producible">Producible to cancel production for.</param>
        /// <returns>Returns quantity of the producible order cancelled.</returns>
        public long CancelProductionOrder(AProducibleSO producible)
        {
            var cancelledQuantity = ProductionQueue.CancelProductionOrder(producible);

            if (cancelledQuantity > 0)
                ProductionEvents.Instance.OnProductionCancelled.Invoke(this, producible, cancelledQuantity);

            return cancelledQuantity;
        }

        private void HandleFinishedProduction(ProducibleQuantity item)
        {
            // Invoke event even if collection fails
            ProductionEvents.Instance.OnProductionFinished.Invoke(this, item);

            // Invoke when last request is produced
            if (ProductionQueue.QueueSize == 0)
                OnProductionFinished.Invoke();

            // Spawn unit
            switch (item.Producible)
            {
                case AUnitSO unit:
                    if (Entity.UnitSpawn != null)
                    {
                        for (int index = 0; index < item.Quantity; index++)
                        {
                            SpawnProducedUnit(unit);
                        }
                    }
                    // Cannot spawn units without a spawn point.
                    else
                    {
                        Debug.LogWarning("Produced unit's cannot be properly spawned without spawn point.");
                    }

                    break;

                default:
                    // Check if stashing is enabled.
                    if (stashProduction)
                    {
                        AddToStash(item);
                    }
                    else
                    {
                        // Update player immediately;
                        Entity.Owner.RegisterProducible(item.Producible, item.Quantity);
                    }

                    break;
            }
        }

        private void AddToStash(ProducibleQuantity item)
        {
            // Check if quantity should be grouped with existing stash
            if (groupStashedQuantity)
            {
                for (int index = 0; index < producedStash.Count; index++)
                {
                    var stash = producedStash[index];
                    if (stash.Producible.ID == item.Producible.ID)
                    {
                        stash.Quantity += item.Quantity;
                        producedStash[index] = stash;
                        return;
                    }
                }
            }

            // If grouping failed, add new stash
            producedStash.Add(item);
        }

        private void SpawnProducedUnit(AUnitSO unit)
        {
            // Makes attributes like unit limit and population take immediate effect.
            Owner.RegisterProducible(unit);

            Entity.UnitSpawn.SpawnUnit(unit, true, spawnedUnit =>
            {
                // Assign spawned unit to the player and garrison it if eligible.
                // Do not register unit, it has been registered before loading prefab
                // and spawning the unit
                Owner.AddUnit(spawnedUnit, false);
                GarrisonSpawnedUnitIfEligible(spawnedUnit);
            });
        }

        /// <summary>
        /// Garrison unit after spawning if eligible. If configured and unit
        /// implements <see cref="IGarrisonableUnit"/> then it will be garrisoned.
        /// </summary>
        /// <param name="newUnit">Newly spawned unit.</param>
        private void GarrisonSpawnedUnitIfEligible(Unit newUnit)
        {
            if (!GarrisonUnitsAfterProduction || Entity.Garrison == null ||
                !newUnit.TryGetComponent(out IGarrisonableUnit garrisonableUnit)) return;
            
            if (Entity.Garrison.AddUnit(garrisonableUnit))
            {
                garrisonableUnit.EnterGarrison();
            }
        }

        #endregion

        #region IProductionUnit

        public bool ShouldFinishProductionFor(ProducibleQuantity producibleQuantity)
            => Delegate?.ShouldFinishProductionFor(producibleQuantity) ?? true;

        /// <summary>
        /// Updates production time on production queue.
        /// This can only produce while unit is operational.
        /// </summary>
        /// <param name="delta">Amount of progression for the queue</param>
        public void Produce(float delta)
        {
            if (isActiveAndEnabled && Entity.IsOperational)
                ProductionQueue.Produce(delta);
        }

        public bool IsProducing(AProducibleSO producible)
            => ProductionQueue.IsProducing(producible);

        public ProducibleQuantity[] GetProductionQueue()
            => ProductionQueue.Queue;

        #endregion
    }

}