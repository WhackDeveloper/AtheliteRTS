namespace TRavljen.UnitSystem.Combat
{

    /// <summary>
    /// Defines the health-related capabilities of an entity.
    /// This includes the initial health points, whether health can be decreased or increased,
    /// and the entity's health regeneration rate.
    /// </summary>
    public interface IHealthCapability : IEntityCapability
    {
        /// <summary>
        /// The maximum health points the entity starts with.
        /// </summary>
        public int HealthPoints { get; }

        /// <summary>
        /// Indicates whether the entity's health can decrease (i.e., take damage).
        /// </summary>
        public bool CanDecrease { get; }

        /// <summary>
        /// Indicates whether the entity's health can increase (i.e., heal or repair).
        /// </summary>
        public bool CanIncrease { get; }

        /// <summary>
        /// The health regeneration rate per tick (e.g., per second). 
        /// Set to 0 if no regeneration is required.
        /// Regeneration may be applied regardless of the <see cref="CanIncrease"/> flag.
        /// </summary>
        public int Regeneration { get; }
    }


}