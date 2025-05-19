using System.Collections;
using TRavljen.UnitSystem.Navigation;
using UnityEngine;

namespace TRavljen.UnitSystem.Demo
{
    using TRavljen.UnitSystem;
    using UnityEngine.AI;

    /// <summary>
    /// Handles unit movement using Unity's NavMeshAgent. Provides methods for 
    /// setting movement targets, handling rotation for interactions, and managing 
    /// movement states. Designed for demo purposes, but can be extended or replaced 
    /// for specific implementations.
    /// </summary>
    [RequireComponent(typeof(NavMeshAgent))]
    [DisallowMultipleComponent]
    public class NavMeshMovement : BaseUnitMovement, IUnitMovement
    {

        #region Properties

        /// <summary>
        /// The NavMeshAgent used for movement.
        /// </summary>
        [SerializeField]
        protected NavMeshAgent agent;

        private Vector3 destination;
        
        /// <summary>
        /// The manager responsible for validating ground positions for movement.
        /// </summary>
        private GroundValidationManager validationManager;
        
        #endregion
        
        #region Lifecycle

        protected override void OnInitialize()
        {
            base.OnInitialize();
            
            validationManager = GroundValidationManager.GetOrCreate();
        }

        private void OnValidate()
        {
            if (agent == null)
                agent = GetComponent<NavMeshAgent>();
        }

        #endregion

        #region AUnitMovement

        public override bool IsMoving => agent.velocity.magnitude > 0.01f;

        public override Vector3 Destination => agent.destination;
        
        public override bool HasReachedDestination()
            => Vector3.Distance(transform.position, destination) <= agent.stoppingDistance;

        public override void StopInCurrentPosition()
            => agent.ResetPath();
        
        protected override void SetAgentDestination(Vector3 position)
            => agent.SetDestination(position);

        public override void SetDestination(Vector3 groundPosition)
        {
            // Validate position before setting the destination.
            // This prevents issues from waiting for agent path for too long.
            var validated = validationManager.ValidatePosition(groundPosition, out var newPos);
            destination = validated ? newPos : groundPosition;
            base.SetDestination(destination);
        }

        #endregion

    }
}
