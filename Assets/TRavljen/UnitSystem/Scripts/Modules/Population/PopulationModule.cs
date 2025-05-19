using TRavljen.EditorUtility;
using UnityEngine;
using UnityEngine.Events;

namespace TRavljen.UnitSystem
{

    /// <summary>
    /// A module responsible for managing the population system of a player in the game.
    /// Population is a common mechanic in RTS games used to control the number of units
    /// a player can spawn. This module handles attributes such as population capacity
    /// and population consumption for units and other producibles.
    /// <para>
    /// If the player does not need to track or limit the population, only the 
    /// <see cref="populationConsumptionAttribute"/> can be used for tracking purposes. 
    /// If population can increase dynamically (e.g., through certain buildings or upgrades),
    /// the <see cref="populationCapacityAttribute"/> should also be set to enable automatic handling.
    /// </para>
    /// </summary>
    [DisallowMultipleComponent]
    public class PopulationModule : APlayerModule
    {

        /// <summary>
        /// Specifies the attribute used to determine the population consumption of producibles.
        /// </summary>
        [SerializeField, Tooltip("Specifies the attribute used to determine population consumption of producibles.")]
        private ProductionAttributeSO populationConsumptionAttribute;

        /// <summary>
        /// Specifies the attribute used to determine the population capacity of producibles.
        /// </summary>
        [SerializeField, Tooltip("Specifies the attribute used to determine population capacity of producibles.")]
        private ProductionAttributeSO populationCapacityAttribute;

        /// <summary>
        /// The maximum population allowed for the player. This is typically increased through faction
        /// settings or specific buildings and upgrades.
        /// </summary>
        [PositiveInt]
        [SerializeField, Tooltip("The maximum population allowed for the player.")]
        private int maxPopulation = 0;

        /// <summary>
        /// Indicates whether the population is capped at the maximum allowed population.
        /// </summary>
        [SerializeField, Tooltip("Indicates whether the population is capped at the maximum allowed population.")]
        private bool maxPopulationEnabled = true;

        /// <summary>
        /// A hard cap for the maximum population. This ensures the maximum population cannot exceed
        /// a certain limit.
        /// </summary>
        [SerializeField, DisableInInspector, PositiveInt]
        [Tooltip("A hard cap for the maximum population. Ensures the population cannot exceed a certain limit.")]
        private int populationHardCap = 100;

        /// <summary>
        /// Event invoked whenever the population count or maximum population changes.
        /// Parameters:
        /// - Current population
        /// - Maximum population
        /// </summary>
        [Header("Events")]
        public UnityEvent<int, int> OnPopulationUpdate = new();

        /// <summary>
        /// ID of the population consumption attribute.
        /// </summary>
        [SerializeField, HideInInspector]
        private int populationConsumptionAttributeID;

        /// <summary>
        /// ID of the population capacity attribute.
        /// </summary>
        [SerializeField, HideInInspector]
        private int populationCapacityAttributeID;

        /// <summary>
        /// Indicates whether population is capped at the maximum allowed population.
        /// </summary>
        public bool MaxPopulationEnabled => maxPopulationEnabled;

        /// <summary>
        /// Gets the maximum population allowed, accounting for any hard cap restrictions.
        /// </summary>
        public int MaxPopulation => maxPopulationEnabled ? Mathf.Min(maxPopulation, populationHardCap) : -1;
        
        /// <summary>
        /// The current population count.
        /// </summary>
        private int currentPopulation = 0;

        /// <summary>
        /// Gets the current population count.
        /// </summary>
        public int CurrentPopulation => currentPopulation;

        #region Lifecycle

        private void OnEnable()
        {
            player.OnEntityAdded.AddListener(HandleEntityAdded);
            player.OnEntityRemoved.AddListener(HandleEntityRemoved);
            player.OnRegisterProducible.AddListener(HandleRegisterProducible);
            player.OnUnregisterProducible.AddListener(HandleUnregisterProducible);
        }

        private void OnDisable()
        {
            player.OnEntityAdded.RemoveListener(HandleEntityAdded);
            player.OnEntityRemoved.RemoveListener(HandleEntityRemoved);
            player.OnRegisterProducible.RemoveListener(HandleRegisterProducible);
            player.OnUnregisterProducible.RemoveListener(HandleUnregisterProducible);
        }

        protected override void OnValidate()
        {
            base.OnValidate();

            if (populationConsumptionAttribute != null)
                populationConsumptionAttributeID = populationConsumptionAttribute.ID;
            else 
                Debug.LogWarning("Population consumption attribute is null.");
            
            if (populationCapacityAttribute != null)
                populationCapacityAttributeID = populationCapacityAttribute.ID;
            else 
                Debug.LogWarning("Population capacity attribute is null.");
        }

        #endregion

        #region Public Interface

        /// <summary>
        /// Sets a hard cap for the population. This ensures the maximum population cannot exceed
        /// the specified limit.
        /// </summary>
        /// <param name="hardCapacity">The new hard cap value.</param>
        public void SetPopulationHardCap(int hardCapacity)
        {
            int currentMax = MaxPopulation;
            populationHardCap = hardCapacity;

            // When max pop changes duo to hard cap, invoke population event.
            if (currentMax != MaxPopulation)
                InvokePopulationUpdatedAction();
        }

        /// <summary>
        /// Sets the maximum population to a specified value. Only positive values will be applied.
        /// </summary>
        /// <param name="population">The new maximum population value.</param>
        public void SetMaxPopulation(int population)
        {
            maxPopulation = population;
            InvokePopulationUpdatedAction();
        }

        /// <summary>
        /// Increases the maximum population.
        /// </summary>
        public void IncreaseMaxPopulation(int population)
        {
            SetMaxPopulation(maxPopulation + population);
            InvokePopulationUpdatedAction();
        }

        /// <summary>
        /// Adds population amount to the current population.
        /// Only positive values are applied, others ignored.
        /// </summary>
        /// <param name="population">Population amount to add.</param>
        public void AddPopulation(int population)
        {
            // Ensure population is updated only with positive value.
            if (population > 0)
                ModifyPopulation(Mathf.Abs(population));
        }

        /// <summary>
        /// Determines whether a producible can be added to the player's population
        /// without exceeding the maximum population capacity.
        /// </summary>
        /// <param name="producibleQuantity">The producible and its quantity to evaluate.</param>
        /// <returns>
        /// Returns <c>true</c> if the producible can be added without exceeding
        /// the maximum population capacity; otherwise, <c>false</c>.
        /// If population limits are not enabled or the producible does not have
        /// a population consumption attribute, this method returns <c>true</c>.
        /// </returns>
        public bool HasPopulationCapacity(ProducibleQuantity producibleQuantity)
        {
            if (maxPopulationEnabled &&
                GetPopulationConsumptionAttribute(
                    producibleQuantity.Producible, out ProductionAttributeValue attributeValue))
            {
                float fullValue = attributeValue.Value * producibleQuantity.Quantity;
                return HasPopulationCapacity((int)fullValue);
            }

            // No population consumption attribute
            return true;
        }

        /// <summary>
        /// Determines whether a population can be added to the player's population
        /// without exceeding the maximum population capacity.
        /// </summary>
        /// <param name="populationConsumption">The population amount to evaluate.</param>
        /// <returns>
        /// Returns <c>true</c> if the producible can be added without exceeding
        /// the maximum population capacity; otherwise, <c>false</c>.
        /// If population limits are not enabled or the producible does not have
        /// a population consumption attribute, this method returns <c>true</c>.
        /// </returns>
        public bool HasPopulationCapacity(int populationConsumption)
        {
            return populationConsumption + CurrentPopulation <= MaxPopulation;
        }

        #endregion

        #region Handle producibles

        /// <summary>
        /// Determines if production for a producible can finish based on its population consumption.
        /// </summary>
        /// <param name="producibleQuantity">The producible being checked.</param>
        /// <returns>True if production can finish, otherwise false.</returns>
        private void HandleRegisterProducible(AProducibleSO producible, long quantity)
        {
            // Apply consumption for the producible
            if (GetPopulationConsumptionAttribute(producible, out ProductionAttributeValue consumption))
            {
                ModifyPopulation((int)(consumption.Value * quantity));
            }

            // Apply capacity if it is not a unit, for units this is applied
            // when it spawns and becomes operational.
            if (producible is not AUnitSO &&
                GetPopulationCapacityAttribute(producible, out ProductionAttributeValue capacity))
            {
                ModifyMaxPopulation((int)(capacity.Value * quantity));
            }
        }

        private void HandleUnregisterProducible(AProducibleSO producible)
        {
            // If removed producible has a population consumption, it will be returned
            // here to the Player.
            if (GetPopulationConsumptionAttribute(producible, out ProductionAttributeValue attribute))
            {
                ModifyPopulation(-(int)attribute.Value);
            }

            // Remove capacity if it is not a unit, for units this is applied
            // when they are removed and if they were operational.
            if (producible is not AUnitSO &&
                GetPopulationCapacityAttribute(producible, out ProductionAttributeValue capacity))
            {
                ModifyMaxPopulation(-(int)capacity.Value);
            }
        }

        #endregion

        #region Handle units

        /// <summary>
        /// Handles adding a entity by applying population capacity if present. If entity is a unit
        /// and not operational it will instead wait for it to be come operational and then apply
        /// the population capacity.
        /// </summary>
        /// <param name="entity">Entity added</param>
        private void HandleEntityAdded(IEntity entity)
        {
            if (!GetPopulationCapacityAttribute(entity.Data, out ProductionAttributeValue capacity))
                return;

            if (entity is IUnit unit)
            {
                // If unit is operational apply population capacity right away
                if (unit.IsOperational)
                {
                    // When unit is operational, we may consider its capacity attributes
                    ModifyMaxPopulation((int)capacity.Value);
                }
                // Otherwise wait for unit to become operational
                else
                {
                    unit.OnOperationsActive.AddListener(HandleEntityOperational);
                }
            }
            else
            {
                // Apply attributes of other entities right away
                ModifyMaxPopulation((int)capacity.Value);
            }
        }

        private void HandleEntityOperational(Entity entity)
        {
            // Cleanup listener
            entity.OnOperationsActive.RemoveListener(HandleEntityOperational);

            // When unit is operational, we may consider its capacity attributes.
            if (GetPopulationCapacityAttribute(entity.Data, out var capacity))
            {
                ModifyMaxPopulation((int)capacity.Value);
            }
        }

        /// <summary>
        /// Removes the population capacity if present on the entity & cleanups any
        /// listeners if present.
        /// </summary>
        /// <param name="unit"></param>
        private void HandleEntityRemoved(IEntity entity)
        {
            if (entity is IUnit unit)
            {
                // When unit is not yet operational, attributes were not applied yet.
                // And unit count is already handled through producibles
                if (!unit.IsOperational)
                {
                    // Cleanup listener
                    unit.OnOperationsActive.RemoveListener(HandleEntityOperational);

                    // Return, if it was not operational, the capacity was not yet applied.
                    return;
                }
            }

            // For units which are operational, remove attributes here.
            if (GetPopulationCapacityAttribute(entity.Data, out ProductionAttributeValue capacity))
            {
                ModifyMaxPopulation(-(int)capacity.Value);
            }
        }

        #endregion

        #region Population Helpers

        private bool GetPopulationCapacityAttribute(AProducibleSO producible, out ProductionAttributeValue attribute)
        {
            return producible.TryGetAttribute(populationCapacityAttributeID, out attribute);
        }

        private bool GetPopulationConsumptionAttribute(AProducibleSO producible, out ProductionAttributeValue attributeValue)
        {
            return producible.TryGetAttribute(populationConsumptionAttributeID, out attributeValue);
        }

        private void ModifyPopulation(int value)
        {
            currentPopulation += value;
            if (maxPopulationEnabled)
            {
                currentPopulation = Mathf.Max(currentPopulation, 0);
            }

            InvokePopulationUpdatedAction();
        }

        private void ModifyMaxPopulation(int value)
        {
            int currentMax = MaxPopulation;
            maxPopulation += value;

            if (currentMax != MaxPopulation)
                InvokePopulationUpdatedAction();
        }

        private void InvokePopulationUpdatedAction() =>
            OnPopulationUpdate?.Invoke(CurrentPopulation, MaxPopulation);

        #endregion

    }

}
