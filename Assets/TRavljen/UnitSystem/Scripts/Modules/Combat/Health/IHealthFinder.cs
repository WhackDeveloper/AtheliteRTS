using System.Collections;
using System.Collections.Generic;
using TRavljen.UnitSystem.Combat;
using UnityEngine;

namespace TRavljen.UnitSystem.Combat
{

    /// <summary>
    /// Interface defining methods for scanning and identifying health components
    /// within a specific range and conditions.
    /// </summary>
    public interface IHealthFinder
    {
        /// <summary>
        /// Finds all nearby health components within a specified range.
        /// </summary>
        /// <param name="attacker">The unit performing the scan.</param>
        /// <param name="position">The center of the scan area.</param>
        /// <param name="range">The maximum range to scan.</param>
        /// <param name="layer">The layer mask to filter targets.</param>
        /// <returns>An array of <see cref="Health"/> components within the range.</returns>
        public IHealth[] FindNearbyHealth(IUnitAttack attacker, Vector3 position, float range);

        /// <summary>
        /// Finds the nearest health component within a specified range.
        /// </summary>
        /// <param name="attacker">The unit performing the scan.</param>
        /// <param name="position">The center of the scan area.</param>
        /// <param name="range">The maximum range to scan.</param>
        /// <param name="layer">The layer mask to filter targets.</param>
        /// <param name="closestHealth">Outputs the closest health component found.</param>
        /// <returns>
        /// True if any health nearby is found; otherwise, false.
        /// </returns>
        public bool FindNearestHealth(IUnitAttack attacker, Vector3 position, float range, out IHealth closestHealth);

    }

}