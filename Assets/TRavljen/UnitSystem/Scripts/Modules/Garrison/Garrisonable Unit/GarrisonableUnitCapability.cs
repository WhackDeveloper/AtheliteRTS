using UnityEngine;

namespace TRavljen.UnitSystem.Garrison
{
    /// <summary>
    /// Adds the capability for a unit to enter a garrison unit. 
    /// Configures the unit with garrison-related behaviors and settings.
    /// </summary>
    public struct GarrisonableUnitCapability : IGarrisonableUnitCapability
    {

        [SerializeField, Tooltip("Minimum range required to enter a garrison.")]
        private float minRange;

        [SerializeField, Tooltip("Maximum range allowed to enter a garrison.")]
        private float maxRange;

        #region IEntityCapability

        /// <summary>
        /// Configures the unit entity by adding a <see cref="GarrisonableUnit"/> 
        /// component if it is not already present. Sets the range values 
        /// for garrison entry.
        /// </summary>
        /// <param name="entity">The entity to configure.</param>
        readonly void IEntityCapability.ConfigureEntity(IEntity entity)
        {
            GarrisonableUnit garrisonableUnit = entity.gameObject.AddComponentIfNotPresent<GarrisonableUnit>();
            garrisonableUnit.SetMinRange(minRange);
            garrisonableUnit.SetMaxRange(maxRange);
        }

        void IEntityCapability.SetDefaultValues()
        {
            minRange = 0;
            maxRange = 1;
        }

        #endregion
    }

}