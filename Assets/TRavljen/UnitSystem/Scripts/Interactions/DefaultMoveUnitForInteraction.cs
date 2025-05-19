using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TRavljen.UnitSystem.Interactions
{
    
    /// <summary>
    /// The default implementation of the <see cref="IMoveUnitForInteraction"/> interface.
    /// This class uses a helper class (InteractorPositionHelper) to determine and set the interaction position.
    /// It is assumed that the underlying movement system handles the actual unit movement.
    /// </summary>
    public struct DefaultMoveUnitForInteraction : IMoveUnitForInteraction
    {
        
        readonly bool IMoveUnitForInteraction.SetInteractionTarget(IUnitInteractorComponent interactor, IUnitInteracteeComponent interactee)
        {
            var (destination, direction) = InteractorPositionHelper.GetInteractionPosition(interactor, interactee);
            interactor.Unit.Movement.SetDestinationAndDirection(destination, direction);
            return true;
        }
        
        readonly bool IMoveUnitForInteraction.RemoveInteractionTarget(IUnitInteractorComponent interactor, IUnitInteracteeComponent interactee)
        {
            if (interactee is IUnitInteractingPosition positions)
                return positions.ReleaseInteractionPosition(interactor);

            // NO position to remove
            return true;
        }

    }
    
}
