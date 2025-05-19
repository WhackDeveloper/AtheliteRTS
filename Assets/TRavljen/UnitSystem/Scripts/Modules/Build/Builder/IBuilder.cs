using TRavljen.UnitSystem.Interactions;

namespace TRavljen.UnitSystem.Build
{

    /// <summary>
    /// Represents a unit that has the capabilities of building another entity
    /// in world space (construction).
    /// </summary>
    public interface IBuilder : IUnitInteractorComponent
    {

        /// <summary>
        /// Specifies if building jobs are picked up automatically.
        /// </summary>
        public bool AutoPickup { get; }
        
        /// <summary>
        /// The radius for automatic task pickup.
        /// </summary>
        float AutoPickUpWorkRadius { get; }

        /// <summary>
        /// Attempts to assign the unit a building task with specified entity to build.
        /// </summary>
        /// <param name="buildable">The target entity to build.</param>
        /// <returns>True if the task was successfully assigned, false otherwise.</returns>
        bool GoBuildUnit(IBuildableEntity buildable);
    }
}