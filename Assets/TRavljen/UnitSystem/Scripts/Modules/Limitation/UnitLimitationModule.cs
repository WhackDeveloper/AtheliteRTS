using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace TRavljen.UnitSystem
{

    /// <summary>
    /// Manages the operational and production limits for units within a player's system. 
    /// Tracks unit counts and enforces both global and faction-specific limitations.
    /// </summary>
    public class UnitLimitationModule: APlayerModule
    {

        #region Properties

        [Tooltip("Indicates whether the faction's default limits should be overridden.")]
        [SerializeField]
        private bool overrideFactionLimits = false;

        [Tooltip("Default unit limits applied if faction limits are overridden.")]
        [SerializeField]
        private UnitLimit[] defaultLimits = new UnitLimit[0];

        [Header("Events")]

        /// <summary>
        /// Event invoked when unit limits are updated.
        /// </summary>
        public UnityEvent OnLimitChanged = new();

        /// <summary>
        /// Event invoked when the unit count changes.
        /// </summary>
        public UnityEvent OnUnitCountChange = new();

        /// <summary>
        /// Dictionary mapping unit IDs to their corresponding limits.
        /// </summary>
        private readonly Dictionary<int, int> limitedUnits = new();

        /// <summary>
        /// Dictionary mapping unit IDs to the current counts of each unit type.
        /// </summary>
        private readonly Dictionary<int, int> unitCounts = new();

        #endregion

        #region Lifecycle

        /// <summary>
        /// Initializes the module and sets up initial unit limits based on faction or custom settings.
        /// </summary>
        protected override void Awake()
        {
            base.Awake();

            if (overrideFactionLimits)
                ExtractUnitLimits(defaultLimits);
            else
                ExtractUnitLimits(player.Faction.UnitMaxLimit);
        }

        private void OnEnable()
        {
            RefreshUnitCounts();

            // Add observers for adding or removing units
            player.OnEntityAdded.AddListener(HandleEntityAdded);
            player.OnEntityRemoved.AddListener(HandleEntityRemoved);

            // Add observers for adding producibles, attributes should be considered
            // when its data was added not the scene unit instance (if one is attached).
            // Units may need to be constructed before attributes are applied.
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

        #endregion

        #region Public Interface

        /// <summary>
        /// Checks if a specific unit type has reached its limit.
        /// </summary>
        /// <param name="unitId">The ID of the unit type.</param>
        /// <returns>True if the limit is reached; otherwise, false.</returns>
        public bool HasReachedLimit(int unitId)
        {
            limitedUnits.TryGetValue(unitId, out int limit);
            if (limit <= 0) return false;

            unitCounts.TryGetValue(unitId, out int count);
            return count >= limit;
        }

        #endregion

        #region Add Events

        private void HandleEntityAdded(IEntity entity)
        {
            // If unit is operational apply unit limit attributes right away
            if (entity is not IUnit unit || unit.IsOperational)
            {
                ModifyLimitsForAttributes(entity.Data.ProductionAttributes, false);
            }
            // Otherwise wait for unit to become operational
            else
            {
                unit.OnOperationsActive.AddListener(HandleEntityOperational);
            }
        }

        private void HandleEntityOperational(Entity entity)
        {
            // Cleanup listener
            entity.OnOperationsActive.RemoveListener(HandleEntityOperational);

            // Modify limits from attributes
            ModifyLimitsForAttributes(entity.Data.ProductionAttributes, false);
        }

        private void HandleRegisterProducible(AProducibleSO producible, long quantity)
        {
            // Check if producible is a unit
            if (producible is AUnitSO unit)
            {
                // Applies unit count right away, regardless of its state.
                // Limit will be modified when unit is added & operational.
                AddUnitToUnitCount(unit, (int)quantity);
            }
            else
            {
                // Modifies
                ModifyLimitsForProducible(producible, quantity);
            }
        }

        #endregion

        #region Remove Events

        private void HandleEntityRemoved(IEntity entity)
        {
            // When unit is not yet operational, attributes were not applied yet.
            // And unit count is already handled through producibles.
            if (entity is IUnit { IsOperational: false } unit)
            {
                // Cleanup listener
                unit.OnOperationsActive.RemoveListener(HandleEntityOperational);

                // Return, if it was not operational, the limit attribute was not yet applied.
                return;
            }

            // For units which are operational, remove attributes here.
            ModifyLimitsForAttributes(entity.Data.ProductionAttributes, true);
        }

        private void HandleUnregisterProducible(AProducibleSO producible)
        {
            // Check if unit, count should be updated immediately;
            // applying attribute happens in unit removal when we
            // know its state.
            if (producible is AUnitSO unit)
            {
                RemoveUnitFromUnitCount(unit);
            }
            else
            {
                ModifyLimitsForAttributes(producible.ProductionAttributes, true);
            }
        }

        #endregion

        #region Convenience

        private void RefreshUnitCounts()
        {
            unitCounts.Clear();

            foreach (var unit in player.GetUnits())
            {
                AddUnitToUnitCount(unit.Data, 1);
            }

            OnUnitCountChange.Invoke();
        }

        private void AddUnitToUnitCount(AUnitSO unit, int newUnitCount)
        {
            int id = unit.ID;

            if (!unitCounts.TryGetValue(id, out int count))
                count = 0;

            unitCounts[id] = count + newUnitCount;
            OnUnitCountChange.Invoke();
        }

        private void RemoveUnitFromUnitCount(AUnitSO unit)
        {
            int id = unit.ID;

            if (unitCounts.TryGetValue(id, out int count))
                count = Mathf.Max(0, count - 1);
            else
                count = 0;

            unitCounts[id] = count;
            OnUnitCountChange.Invoke();
        }

        /// <summary>
        /// Applies production attributes of the producible to the current limitation
        /// for the player.
        /// </summary>
        private void ModifyLimitsForProducible(AProducibleSO producible, long quantity)
        {
            var attributes = producible.ProductionAttributes;

            // Multiply with quantity
            for (int index = 0; index < attributes.Length; index++)
            {
                var attribute = attributes[index];
                attribute.Value *= quantity;
                attributes[index] = attribute;
            }

            ModifyLimitsForAttributes(attributes, false);
        }

        /// <summary>
        /// Applies production attributes to the current limitation for the player.
        /// </summary>
        /// <param name="attributeValues">Attributes to apply</param>
        /// <param name="remove">Removes values if true, adds if false.</param>
        private void ModifyLimitsForAttributes(ProductionAttributeValue[] attributeValues, bool remove)
        {
            if (attributeValues.Length == 0) return;

            foreach (var attribute in attributeValues)
            {
                if (attribute.Attribute is UnitLimitAttributeSO limit)
                {
                    int limitUnitID = limit.unit.ID;
                    if (!limitedUnits.TryGetValue(limitUnitID, out int currentLimit))
                    {
                        // No limit yet.
                        currentLimit = 0;
                    }

                    int modifier = remove ? -1 : 1;
                    // Attribute value type is ignored as its currently not supported.
                    limitedUnits[limitUnitID] = Mathf.Max(((int)attribute.Value * modifier) + currentLimit, 0);
                }
            }

            OnLimitChanged.Invoke();
        }

        private void ExtractUnitLimits(UnitLimit[] unitMaxLimit)
        {
            limitedUnits.Clear();

            foreach (UnitLimit limit in unitMaxLimit)
            {
                if (!limitedUnits.TryAdd(limit.unit.ID, limit.limit))
                {
                    Debug.Log("Found duplicate IDs in unit limits. Duplicate will be ignored.");
                }
            }
        }

        #endregion

    }
}
