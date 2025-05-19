using UnityEngine;

namespace TRavljen.UnitSystem.Interactions
{

    /// <summary>
    /// Represents an object that can provide a specific interaction position for a unit.
    /// Extends the <see cref="IUnitInteracteeComponent"/> interface.
    /// This allows units to define their own interaction positions instead of interactors simply
    /// positioning themselves around it.
    /// </summary>
    /// <remarks>
    /// This interface is useful for defining dynamic interaction points for units.
    /// For example, units can interact with a structure at a specific offset or
    /// interact with other units based on their position and range.
    /// </remarks>
    public interface IUnitInteractingPosition : IUnitInteracteeComponent
    {

        /// <summary>
        /// Retrieves the interaction position for the specified <see cref="IUnitInteractorComponent"/>.
        /// </summary>
        /// <param name="interactor">The interacting unit requesting the interaction position.</param>
        /// <param name="reserve">If the interaction position should be reserved for the interactor.</param>
        /// <returns>
        /// A <see cref="Vector3"/> representing the world-space position where
        /// the <paramref name="interactor"/> should move or interact.
        /// </returns>
        Vector3 GetAvailableInteractionPosition(IUnitInteractorComponent interactor, bool reserve);

        /// <summary>
        /// Releases the interaction position reserved for the specified <see cref="IUnitInteractorComponent"/>.
        /// </summary>
        /// <param name="interactor">The interacting unit that reserved the interaction position.</param>
        /// <returns>
        /// <c>true</c> if the interaction position was successfully released,
        /// <c>false</c> otherwise.
        /// </returns>
        bool ReleaseInteractionPosition(IUnitInteractorComponent interactor);

    }

}