using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace IntegrationDemo
{
    using TRavljen.UnitFormation;
    using TRavljen.UnitSystem.Combat;
    using TRavljen.UnitSystem.Demo;

    class FormationNavMeshMovement : NavMeshMovement, IFormationUnit
    {
        public bool IsWithinStoppingDistance => !agent.pathPending && agent.remainingDistance <= agent.stoppingDistance;

        public void SetTargetDestination(Vector3 newTargetDestination, float newFacingAngle)
        {
            // Get simple world direction.
            Vector3 direction = Quaternion.Euler(0, newFacingAngle, 0) * Vector3.forward;

            // Update stance info for attack if present
            IDefendPosition.UpdateStance(unit.UnitAttack, newTargetDestination, direction);
            SetDestinationAndDirection(newTargetDestination, direction);
        }
    }
}