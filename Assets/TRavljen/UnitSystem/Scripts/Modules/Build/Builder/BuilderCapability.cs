using UnityEngine;

namespace TRavljen.UnitSystem.Build
{

    /// <summary>
    /// Concrete implementation of the <see cref="IBuilderCapability"/> interface.
    /// Defines serialized properties for configuring the building capabilities of units.
    /// </summary>
    [System.Serializable]
    public struct BuilderCapability : IBuilderCapability
    {

        [Tooltip("Specifies if the building assignments should be automatically picked up when the unit is available.")]
        [SerializeField]
        private bool autoPickup;

        [Tooltip("The radius within which the unit can automatically pick up building tasks.")]
        [SerializeField]
        private float pickupRadius;

        [Tooltip("The minimum range required for interacting with building tasks.")]
        [SerializeField]
        private float minRange;

        [Tooltip("The maximum range within which the unit can interact with building tasks.")]
        [SerializeField]
        private float range;

        [Tooltip("The time interval between building updates. A value of 0 updates the build process each frame.")]
        [SerializeField]
        private float buildInterval;

        [Tooltip("Specifies the unit's building power, determining construction speed or efficiency.")]
        [SerializeField]
        private int power;

        /// <inheritdoc/>
        public readonly bool AutoPickup => autoPickup;

        /// <inheritdoc/>
        public readonly float PickupRadius => pickupRadius;

        /// <inheritdoc/>
        public readonly float MinRange => minRange;

        /// <inheritdoc/>
        public readonly float Range => range;

        /// <inheritdoc/>
        public readonly float BuildInterval => buildInterval;

        /// <inheritdoc/>
        public readonly int Power => power;

        #region IEntityCapability

        /// <summary>
        /// Configures the associated entity by adding the <see cref="Builder"/> component.
        /// </summary>
        readonly void IEntityCapability.ConfigureEntity(IEntity entity)
        {
            entity.gameObject.AddComponentIfNotPresent<Builder>();
        }

        void IEntityCapability.SetDefaultValues()
        {
            autoPickup = true;
            pickupRadius = 20;
            minRange = 0f;
            range = 1f;
            // Every 100 ms
            buildInterval = 0.01f;
            // Build power of 50%
            power = 50;
        }

        #endregion
    }

}