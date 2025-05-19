using System;
using System.Collections;
using TRavljen.UnitSystem.Utility;
using UnityEngine;

namespace TRavljen.UnitSystem
{

    /// <summary>
    /// Abstract base class for unit movement behavior that provides functionality for 
    /// rotation and direction handling. Intended for extension by specific movement implementations.
    /// </summary>
    public abstract class BaseUnitMovement : AUnitMovement
    {

        [Tooltip("Rotation speed used for target direction.")]
        [SerializeField]
        protected float rotationSpeed = 180;

        [Tooltip("The threshold for determining if the unit's rotation has finished adjusting to the target.")]
        [SerializeField, PositiveFloat]
        protected float rotationThreshold = 0.1f;

        /// <summary>
        /// Tracks whether the unit is currently rotating.
        /// </summary>
        protected bool IsRotating { get; private set; } = false;

        /// <summary>
        /// Last target direction for rotation. Used only when <see cref="IsRotating"/> is true.
        /// </summary>
        private Vector3 targetDirection;

        /// <summary>
        /// Checks if the unit is currently facing a specific target direction.
        /// This method indicates whether the unit is in the process of rotating
        /// towards its designated target direction.
        /// </summary>
        /// <returns>True if the unit is facing the target direction; otherwise, false.</returns>
        public override bool IsFacingTargetDirection() => !IsRotating;
        
        public override void SetTargetDirection(Vector3 direction)
        {
            if (direction == targetDirection) return;
            if (IsLookingAt(direction)) return;

            // Set new target direction
            targetDirection = direction;
            IsRotating = true;
        }
        
        private bool IsLookingAt(Vector3 direction)
        {
            return transform.IsLookingAt(direction, new (0, rotationThreshold, 0), true);
        }

        protected void Update()
        {
            if (!IsRotating || IsMoving) return;

            if (targetDirection == Vector3.zero)
                return;

            Quaternion targetRotation = Quaternion.LookRotation(targetDirection);

            transform.rotation = Quaternion.RotateTowards(
                transform.rotation,
                targetRotation,
                rotationSpeed * Time.deltaTime
            );
            
            if (IsLookingAt(targetDirection))
            {
                IsRotating = false;
            }
        }
        
        protected override void ClearTargetDirection()
        {
            IsRotating = false;
        }
    }

}
