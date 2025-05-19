namespace TRavljen.UnitSystem.Combat
{
    using UnityEngine;

    /// <summary>
    /// Represents the capability of a unit or entity to have health-related behaviors.
    /// </summary>
    public struct HealthCapability : IHealthCapability
    {
        [SerializeField]
        [Tooltip("The maximum hit points (health) the entity starts with.")]
        private int healthPoints;

        [SerializeField]
        [Tooltip("Specifies if the health of the entity can decrease (i.e., take damage).")]
        private bool canDecrease;

        [SerializeField]
        [Tooltip("Specifies if the health of the entity can increase (i.e., heal or repair).")]
        private bool canIncrease;

        [SerializeField, PositiveInt]
        [Tooltip("Health regeneration rate per tick (e.g., per second). Set to 0 for no regeneration.")]
        private int regeneration;

        /// <summary>
        /// Gets the maximum hit points for the entity.
        /// </summary>
        readonly public int HealthPoints => healthPoints;

        /// <summary>
        /// Determines if the entity's health can decrease.
        /// </summary>
        readonly public bool CanDecrease => canDecrease;

        /// <summary>
        /// Determines if the entity's health can increase.
        /// </summary>
        readonly public bool CanIncrease => canIncrease;

        /// <summary>
        /// Gets the regeneration rate of the entity's health.
        /// </summary>
        readonly public int Regeneration => regeneration;

        #region IEntityCapability

        /// <summary>
        /// Configures the health component for the given entity.
        /// </summary>
        /// <param name="entity">The entity to configure health for.</param>
        readonly void IEntityCapability.ConfigureEntity(IEntity entity)
        {
            // Ensure the entity has a Health component to manage health behaviors.
            entity.gameObject.AddComponentIfNotPresent<Health>();
        }

        void IEntityCapability.SetDefaultValues()
        {
            // Give some health and enable decreasing it
            healthPoints = 100;
            canDecrease = true;
            // Enables heal by default, this also impacts building with health.
            canIncrease = true;
            // Disables regeneration by default
            regeneration = 0;
        }

        #endregion
    }


}