using UnityEngine;

namespace TRavljen.PlacementSystem
{
    /// <summary>
    /// Defines contract for a placement area.
    /// </summary>
    public interface IPlacementArea
    {
        /// <summary>
        /// Returns closest position within the area.
        /// </summary>
        public Vector3 ClosestPosition(Vector3 position);

        /// <summary>
        /// Checks whether the position is inside the area.
        /// </summary>
        public bool IsInside(Vector3 position);
    }
}