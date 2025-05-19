using UnityEngine;

namespace TRavljen.UnitSystem.Garrison
{

    /// <summary>
    /// Specifies the capacity of this unit to garrison other units
    /// and the permissions for it.
    /// </summary>
    /// <remarks>
    /// The use of `sealed` ensures that this class cannot be inherited,
    /// which simplifies its design and enforces clear boundaries for its functionality.
    /// This also allows for default values, improving usability without requiring full manual setup.
    /// </remarks>
    public struct GarrisonUnitsCapability : IGarrisonUnitsCapability
    {

        [SerializeField]
        [PositiveInt]
        [Tooltip("The maximum number of units this garrison can hold. Exceeding this will depend on specific garrison logic.")]
        private int capacity;

        [SerializeField]
        [Tooltip("Determines if this garrison spawns units when they exit.")]
        private bool spawnsGarrisonedUnits;

        [Space]
        [SerializeField]
        [Tooltip("Types of units allowed to enter the garrison. If empty, all types are allowed.")]
        private UnitTypeSO[] permittedTypes;

        [SerializeField]
        [Tooltip("Specific units that are excluded from entering the garrison.")]
        private AUnitSO[] excludedUnits;

        public readonly int Capacity => capacity;

        /// <summary>
        /// Determines if a specific unit is eligible to enter the garrison.
        /// </summary>
        /// <param name="unit">The unit to check.</param>
        /// <returns>Returns true if the unit is eligible to enter, false otherwise.</returns>
        public readonly bool IsEligibleToEnter(AUnitSO unit)
        {
            if (IsUnitExcluded(unit)) return false;

            // If no specific types are permitted, all units are allowed
            if (permittedTypes.Length == 0) return true;

            return unit.DoesMatchAnyType(permittedTypes);
        }

        /// <summary>
        /// Checks if a unit is explicitly excluded from entering the garrison.
        /// </summary>
        /// <param name="unit">The unit to check.</param>
        /// <returns>Returns true if the unit is excluded, false otherwise.</returns>
        private readonly bool IsUnitExcluded(AUnitSO unit)
        {
            foreach (var excludedUnit in excludedUnits)
            {
                if (excludedUnit.ID == unit.ID)
                {
                    Debug.Log($"Unit {unit.Name} is excluded from entering the garrison.");
                    return true;
                }
            }
            return false;
        }

        #region IEntityCapability

        /// <summary>
        /// Configures the entity to support garrison functionality.
        /// Adds necessary components like a garrison behavior and spawn points.
        /// </summary>
        /// <param name="entity">The entity to configure.</param>
        readonly void IEntityCapability.ConfigureEntity(IEntity entity)
        {
            var root = entity.gameObject;
            root.AddComponentIfNotPresent<GarrisonEntity>();

            // When garrison can spawn, add default spawn point if missing.
            if (spawnsGarrisonedUnits)
            {
                IUnitSpawn existingSpawnPoint = entity.gameObject.GetComponentInChildren<IUnitSpawn>();
                if (existingSpawnPoint == null)
                {
                    entity.gameObject.AddComponent<UnitSpawnPoint>();
                }
            }
        }

        void IEntityCapability.SetDefaultValues()
        {
            capacity = 5;
            spawnsGarrisonedUnits = true;
            permittedTypes = null;
            excludedUnits = null;
        }

        #endregion
    }

}