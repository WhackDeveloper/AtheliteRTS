using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TRavljen.UnitFormation.Placement
{
    /// <summary>
    /// Interface for searching nearest valid position for the pathfinding system.
    /// </summary>
    public interface IGroundPositioner
    {
        /// <summary>
        /// Searches for nearest ground position valid for pathfinding.
        /// </summary>
        /// <param name="position">Desired position</param>
        /// <param name="maxDistance"Maximal distance from desired position></param>
        /// <returns>Returns valid pathfinding position or the same value if no valid
        /// position was found inside the max distance radius.</returns>
        public Vector3 PositionOnGround(Vector3 position, float maxDistance);
    }
}