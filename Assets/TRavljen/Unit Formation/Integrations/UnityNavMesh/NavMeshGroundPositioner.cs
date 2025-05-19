using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TRavljen.UnitFormation.Placement
{
    /// <summary>
    /// Implementation of the <see cref="IGroundPositioner"/> for Unity's NavMesh
    /// pathfinding system.
    /// </summary>
    sealed class NavMeshGroundPositioner : IGroundPositioner
    {
        public Vector3 PositionOnGround(Vector3 position, float maxDistance)
        {
            // Use NavMesh for finding valid position.
            UnitFormationHelper.TryMovePositionOnGround(position, maxDistance, out Vector3 groundPosition);
            return groundPosition;
        }
    }
}