namespace TRavljen.UnitSystem.Build
{

    /// <summary>
    /// Represents the capability of a unit to be built.
    /// Provides metadata for whether the building process should start automatically.
    /// </summary>
    public interface IBuildableCapability : IUnitCapability
    {
        /// <summary>
        /// Gets a value indicating whether the building process should start automatically when the unit is initialized.
        /// </summary>
        public bool AutoBuild { get; }

        /// <summary>
        /// Gets a value indicating whether the building process should be using health or independent progress value.
        /// </summary>
        public bool UsesHealth { get; }
    }
    
}