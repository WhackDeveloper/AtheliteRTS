using UnityEngine;

namespace TRavljen.UnitSystem.Interactions
{

    /// <summary>
    /// Defines the contract for providing predefined interaction positions for 
    /// units attempting to interact with other units in the system.
    /// </summary>
    /// <remarks>
    /// This interface is intended for use in systems where units need to 
    /// interact with each other in a defined manner. Implementations of this 
    /// interface should provide logic to determine available interaction 
    /// positions based on the current state of the units involved in the 
    /// interaction. 
    /// 
    /// The methods allow the interactor unit to request an available position 
    /// and to release that position when the interaction is no longer 
    /// necessary. Proper management of these positions ensures that interactions 
    /// are conducted smoothly and logically within the game or simulation.
    /// </remarks>
    public interface IPredefinedInteractionPositionProvider
    {
        /// <summary>
        /// Retrieves an available interaction position for the given interactor and interactee.
        /// </summary>
        /// <param name="interactor">The unit attempting to interact.</param>
        /// <param name="interactee">The unit being interacted with.</param>
        /// <param name="reserve">Whether to reserve the position for the interactor.</param>
        /// <returns>A Vector3 representing the available interaction position.</returns>
        public Vector3 GetAvailableInteractionPosition(IUnitInteractorComponent interactor, IUnitInteracteeComponent interactee, bool reserve);

        /// <summary>
        /// Releases a previously reserved interaction position.
        /// </summary>
        /// <param name="interactor">The unit that occupied the position.</param>
        /// <param name="interactee">The unit being interacted with.</param>
        /// <returns>True if the position was successfully released, otherwise false.</returns>
        public bool ReleaseInteractionPosition(IUnitInteractorComponent interactor, IUnitInteracteeComponent interactee);

    }

}