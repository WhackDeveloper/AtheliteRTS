namespace TRavljen.UnitSystem.Build
{

    /// <summary>
    /// Represents the capability for units to build other entities.
    /// Includes attributes for automated task pickup, interaction ranges,
    /// and build power and frequency.
    /// </summary>
    public interface IBuilderCapability : IUnitCapability
    {
        
        /// <summary>
        /// Specifies whether the unit should automatically pick up build tasks
        /// when available.
        /// </summary>
        bool AutoPickup { get; }

        /// <summary>
        /// The radius within which the unit can pick up build assignments automatically.
        /// </summary>
        float PickupRadius { get; }

        /// <summary>
        /// The minimum interaction range for the building tasks.
        /// </summary>
        float MinRange { get; }

        /// <summary>
        /// The maximum interaction range for the building tasks.
        /// </summary>
        float Range { get; }

        /// <summary>
        /// The frequency of building updates. A value of 0 implies that the building
        /// process is updated every frame.
        /// </summary>
        float BuildInterval { get; }

        /// <summary>
        /// The power level of the unit's building ability, influencing the speed
        /// or efficiency of construction.
        /// </summary>
        int Power { get; }
        
    }

}