using UnityEngine;

namespace TRavljen.UnitSystem.Interactions
{

    /// <summary>
    /// Defines a contract for managing unit movement once interaction is invoked.
    /// It mediates between interaction components and a movement system.
    /// Interactor should find best appropriate position to move to. The rest
    /// should be controlled by the movement components.
    /// </summary>
    public interface IMoveUnitForInteraction
    {
        /// <summary>
        /// Sets the interaction target, moving the interactor unit to an appropriate position for interaction.
        /// </summary>
        /// <param name="interactor">The unit initiating the interaction (e.g., the unit performing an action).</param>
        /// <param name="interactee">The unit being interacted with (e.g., the unit receiving the action).</param>
        /// <returns>True if the interaction target was successfully set and movement was initiated; otherwise, false.</returns>
        public bool SetInteractionTarget(IUnitInteractorComponent interactor, IUnitInteracteeComponent interactee);

        /// <summary>
        /// Removes the interaction target.
        /// </summary>
        /// <param name="interactor">The unit that was interacting.</param>
        /// <param name="interactee">The unit that was being interacted with.</param>
        /// <returns>True if the interaction target was successfully removed; otherwise, false.</returns>
        public bool RemoveInteractionTarget(IUnitInteractorComponent interactor, IUnitInteracteeComponent interactee);
    }

}
