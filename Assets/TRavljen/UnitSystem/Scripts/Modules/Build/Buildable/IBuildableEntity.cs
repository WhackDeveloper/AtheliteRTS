using TRavljen.UnitSystem.Interactions;

namespace TRavljen.UnitSystem.Build
{

    /// <summary>
    /// Represents a unit that can undergo a building process.
    /// Provides mechanisms to track and manage the state of the build.
    /// </summary>
    public interface IBuildableEntity : IUnitInteracteeComponent
    {
        /// <summary>
        /// Gets the current progress of the building process as a value between 0 (not started) and 1 (fully built).
        /// </summary>
        float Progress { get; }

        /// <summary>
        /// Gets a value indicating whether the unit is fully constructed and operational.
        /// </summary>
        bool IsBuilt { get; }
        
        /// <summary>
        /// Indicates whether the building process should start automatically.
        /// </summary>
        bool BuildAutomatically { get; }

        /// <summary>
        /// Initiates the building process for the unit. Typically called when construction begins.
        /// </summary>
        void StartBuilding();

        /// <summary>
        /// Completes the building process, marking the unit as fully built and operational.
        /// </summary>
        void FinishBuilding();

        /// <summary>
        /// Updates the building process by applying the specified power.
        /// </summary>
        /// <param name="power">The power applied to the build progress.</param>
        void BuildWithPower(float power);
    }

}