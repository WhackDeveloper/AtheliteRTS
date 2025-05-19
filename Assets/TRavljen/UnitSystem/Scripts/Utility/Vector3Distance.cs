using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TRavljen.UnitSystem.Utility
{

    public static class Vector3Distance
    {

        /// <summary>
        /// Finds the index of the point in a given array of <see cref="Vector3"/> points
        /// that is closest to a specified target position.
        /// </summary>
        /// <param name="targetPosition">
        /// The position to which the closest point is calculated.
        /// </param>
        /// <param name="points">
        /// An array of points to search for the closest one. The array can contain any number of points.
        /// </param>
        /// <returns>
        /// The index of the closest point in the <paramref name="points"/> array.
        /// Returns <c>-1</c> if the array is <c>null</c> or empty.
        /// </returns>
        public static int GetClosestPointIndex(Vector3 targetPosition, Vector3[] points)
        {
            if (points == null || points.Length == 0)
            {
                return -1; // Return -1 if the array is empty or null
            }

            int closestIndex = 0;
            float closestDistanceSqr = (targetPosition - points[0]).sqrMagnitude;

            for (int i = 1; i < points.Length; i++)
            {
                float distanceSqr = (targetPosition - points[i]).sqrMagnitude;
                if (distanceSqr < closestDistanceSqr)
                {
                    closestDistanceSqr = distanceSqr;
                    closestIndex = i;
                }
            }

            return closestIndex;
        }
    }

}