using UnityEngine;

namespace TRavljen.UnitSystem.Interactions.PredefinedPositions
{
    /// <summary>
    /// Defines the contract for generating positions within a specified range.
    /// </summary>
    /// <remarks>
    /// This interface is useful in scenarios where you need to generate multiple positions,
    /// such as for placing units, spawning objects, or creating waypoints in a defined area.
    /// Implementations should provide logic to determine how positions are generated 
    /// based on the specified distance.
    /// </remarks>
    interface IPositionGenerator
    {

        /// <summary>
        /// Attempts to generate positions within a specified range.
        /// </summary>
        /// <param name="distance">The maximum distance within which positions should be generated.</param>
        /// <param name="result">An array that will hold the generated positions if successful.</param>
        /// <returns>
        /// <c>true</c> if positions were successfully generated; otherwise, <c>false</c>.
        /// </returns>
        public bool TryGeneratePositionsInRange(float distance, out Vector3[] result);
    }

}