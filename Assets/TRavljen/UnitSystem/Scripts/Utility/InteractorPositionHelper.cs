using TRavljen.UnitSystem.Combat;
using UnityEngine;

namespace TRavljen.UnitSystem
{
    using Interactions;

    /// <summary>
    /// Provides utility methods for handling interaction positioning between 
    /// <see cref="IUnitInteractorComponent"/> and <see cref="IUnitInteracteeComponent"/> objects.
    /// </summary>
    public static class InteractorPositionHelper
    {
        
        #region Getter
        
        internal static (Vector3 destination, Vector3 direction) GetInteractionPosition(IUnitInteractorComponent interactor, IUnitInteracteeComponent interactee, bool reserve = true)
        {
            Vector3 position = interactor.Position;
            Vector3 targetPosition = interactee.Position;
            Vector3 distance = targetPosition - position;
            float distanceMagnitude = distance.magnitude;

            Vector3 destination;
            Vector3 direction;

            if (interactee is IUnitInteractingPosition positions)
            {
                // Take new destination and target's position for calculating new facing direction.
                destination = positions.GetAvailableInteractionPosition(interactor, true);
                direction = targetPosition - destination;
            }
            // If already within distance, no need to move.
            else if (interactor.MinInteractionRange < distanceMagnitude && distanceMagnitude < interactor.MaxInteractionRange)
            {
                destination = position;
                direction = targetPosition - destination;
            }
            else
            {
                destination = targetPosition;
                direction = targetPosition - position;
            }
            
            return (destination, direction);
        }

        
        #endregion
        
        #region Calculate For Bounds
        
        /// <summary>
        /// Calculates the interaction position and direction for a given interactor and interactee.
        /// </summary>
        /// <param name="interactor">The unit initiating the interaction.</param>
        /// <param name="interactee">The unit being interacted with.</param>
        /// <returns>
        /// A tuple containing:
        ///   - The calculated destination position for the interactor.
        ///   - The direction the interactor should face towards the interactee.
        /// </returns>
        /// <remarks>
        /// This method determines the interactee's bounding box (using collider if present or using transform scale)
        /// and then calls the overload that accepts bounding box.
        /// It assumes that the interactor's current position should be considered when calculating the destination.
        /// </remarks>
        public static (Vector3, Vector3) CalculateInteractionPositionAndDirection(this IUnitInteractorComponent interactor, IUnitInteracteeComponent interactee)
        {
            Bounds bounds;

            // Get collider if present
            if (interactee.transform.TryGetComponent(out Collider collider))
            {
                bounds = collider.bounds;
            }
            // Fallback on scale if no collider
            else
            {
                bounds = new Bounds(interactee.Position, interactee.transform.localScale);
            }

            return CalculateInteractionPositionAndDirection(interactor, interactee, bounds);
        }

        /// <summary>
        /// Calculates the interaction position and direction, taking the interactee's bounding box into account.
        /// </summary>
        /// <param name="interactor">The unit initiating the interaction.</param>
        /// <param name="interactee">The unit being interacted with.</param>
        /// <param name="interacteeBoundingBox">The bounding box of the interactee. This is used to calculate the closest point for the interaction.</param>
        /// <returns>
        /// A tuple containing:
        ///   - The calculated destination position for the interactor.
        ///   - The direction the interactor should face towards the interactee.
        /// </returns>
        /// <remarks>
        /// This method determines the destination for the interactor based on the interactee's bounding box,
        /// the interactor's position, and the MaxInteractionRange of interactor.
        /// The interactor will move to a position MaxInteractionRange units away from the interactee.
        /// </remarks>
        public static (Vector3, Vector3) CalculateInteractionPositionAndDirection(this IUnitInteractorComponent interactor, IUnitInteracteeComponent interactee, Bounds interacteeBoundingBox)
        {
            Vector3 position = interactor.Position;
            Vector3 targetPosition = interactee.Position;
            Vector3 distance = targetPosition - position;

            // If already within distance, no need to move.
            targetPosition = interacteeBoundingBox.ClosestPoint(position);

            Vector3 offset = distance.normalized * interactor.MaxInteractionRange;
            Vector3 potentialPos = targetPosition - offset;
            Vector3 destination = potentialPos;
            Vector3 newDistance = targetPosition - potentialPos;
            Vector3 direction = newDistance.normalized;

            return (destination, direction);
        }
        
        #endregion

    }
}
