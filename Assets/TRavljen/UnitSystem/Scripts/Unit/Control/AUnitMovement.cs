using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TRavljen.UnitSystem
{
    using Combat;
    using Navigation;

    /// <summary>
    /// Abstract class that defines the basic contract for unit movement behavior.
    /// By implementing <see cref="IUnitMovement"/> it provides methods and properties
    /// that define how a unit can move, rotate, and respond to commands in the game world.
    /// </summary>
    public abstract class AUnitMovement : AUnitComponent, IUnitMovement
    {

        #region Serialized Fields

        /// <summary>
        /// Toggles the visibility of destination gizmos in the Scene View.
        /// </summary>
        [SerializeField]
        private bool showDestinationGizmo;

        /// <summary>
        /// Color of the gizmo sphere representing the NavMeshAgent's destination.
        /// </summary>
        [SerializeField]
        private Color destinationGizmoColor = Color.blue;

        /// <summary>
        /// Returns the move target destination.
        /// </summary>
        public Vector3 GetControlPosition() => Destination;
        
        #endregion

        #region IControlUnit

        public virtual void SetControlPosition(Vector3 groundPosition)
        {
            // Update stance info for attack if present - this is "command" interface.
            IDefendPosition.UpdateStance(unit.UnitAttack, groundPosition, Vector3.zero);

            SetDestination(groundPosition);
        }

        #endregion

        #region IUnitMovement

        public abstract bool IsMoving { get; }

        public abstract Vector3 Destination { get; }

        public abstract bool HasReachedDestination();

        /// <summary>
        /// Sets a new destination for the unit, optionally validating the ground position.
        /// Clears any previously set target direction before setting the new destination.
        /// </summary>
        /// <param name="groundPosition">The target position to set as the destination.</param>
        public virtual void SetDestination(Vector3 groundPosition)
        {
            ClearTargetDirection();
            SetAgentDestination(groundPosition);
        }

        public virtual void SetDestinationAndDirection(Vector3 position, Vector3 direction)
        {
            SetDestination(position);
            SetTargetDirection(direction);
        }

        public abstract void StopInCurrentPosition();

        #endregion

        #region Rotation

        public abstract bool IsFacingTargetDirection();

        /// <summary>
        /// Sets the target direction for the unit.
        /// </summary>
        /// <param name="direction">The target direction.</param>
        public abstract void SetTargetDirection(Vector3 direction);

        /// <summary>
        /// Clears the target direction, stopping rotation behavior.
        /// </summary>
        protected abstract void ClearTargetDirection();

        #endregion

        #region Movement Agent

        /// <summary>
        /// Sets the destination for the agent to move towards.
        /// This method should invoke movement on the navigation agent.
        /// </summary>
        /// <param name="position">The target position to set as the destination.</param>
        protected abstract void SetAgentDestination(Vector3 position);

        #endregion

        /// <summary>
        /// Draws gizmos in the Scene View to visualize movement-related data.
        /// </summary>
        private void OnDrawGizmos()
        {
            if (showDestinationGizmo)
            {
                Vector3 destination = Destination;
                Gizmos.color = destinationGizmoColor;
                Gizmos.DrawSphere(destination, 0.5f);

                Gizmos.DrawLine(transform.position, destination);
            }
        }
    }

}