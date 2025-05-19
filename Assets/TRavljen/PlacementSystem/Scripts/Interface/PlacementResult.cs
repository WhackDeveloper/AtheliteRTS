using UnityEngine;

namespace TRavljen.PlacementSystem
{
    /// <summary>
    /// Information about the placement result.
    /// It contains objects placing position and it's rotation.
    /// </summary>
    public struct PlacementResult
    {
        /// <summary>
        /// Position of the placement.
        /// </summary>
        public Vector3 Position;

        /// <summary>
        /// Rotation of the placement.
        /// </summary>
        public Quaternion Rotation;
    }
}
