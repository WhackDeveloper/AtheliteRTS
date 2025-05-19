using UnityEngine;

namespace TRavljen.UnitSystem
{

    /// <summary>
    /// Interface defining movement behavior for units within the UnitSystem.  
    /// Enables interaction with unit positioning, movement, and rotation.  
    /// </summary>
    public interface IUnitMovement : IUnitComponent, IControlEntity
    {
        
        /// <summary>
        /// Gets the current destination of the unit.
        /// </summary>
        public Vector3 Destination { get; }
        
        /// <summary>
        /// Checks if the unit is moving or standing still.
        /// </summary>
        public bool IsMoving { get; }

        /// <summary>
        /// Moves the unit to the specified target position.
        /// </summary>
        /// <param name="position">The world position to move the unit to.</param>
        void SetDestination(Vector3 position);

        /// <summary>
        /// Moves the unit to the specified target position and sets its facing direction.
        /// </summary>
        /// <param name="position">The world position to move the unit to.</param>
        /// <param name="direction">The direction the unit should face after reaching the position.</param>
        void SetDestinationAndDirection(Vector3 position, Vector3 direction);

        void SetTargetDirection(Vector3 direction);

        /// <summary>
        /// Stops the unit at its current position, halting all movement.
        /// </summary>
        void StopInCurrentPosition();

        /// <summary>
        /// Checks if the unit has reached its designated destination.
        /// </summary>
        /// <returns>True if the unit is at the destination; otherwise, false.</returns>
        bool HasReachedDestination();

        /// <summary>
        /// Checks if the unit is currently facing a specific target direction.
        /// </summary>
        /// <returns>True if the unit is facing the target direction; otherwise, false.</returns>
        bool IsFacingTargetDirection();
    }

}
