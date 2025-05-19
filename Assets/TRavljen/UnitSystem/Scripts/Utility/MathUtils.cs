using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TRavljen.UnitSystem.Utility
{

    /// <summary>
    /// Utility class providing common mathematical operations for long integers.
    /// </summary>
    /// <remarks>
    /// Contains helper methods for comparing and finding the minimum or maximum 
    /// of two <see cref="long"/> values. Designed for clarity and simplicity.
    /// </remarks>
    public static class MathUtils
    {
        /// <summary>
        /// Returns the smaller of two <see cref="long"/> values.
        /// </summary>
        /// <param name="a">The first value to compare.</param>
        /// <param name="b">The second value to compare.</param>
        /// <returns>The smaller of the two input values.</returns>
        public static long Min(long a, long b)
        {
            return a < b ? a : b;
        }

        /// <summary>
        /// Returns the larger of two <see cref="long"/> values.
        /// </summary>
        /// <param name="a">The first value to compare.</param>
        /// <param name="b">The second value to compare.</param>
        /// <returns>The larger of the two input values.</returns>
        public static long Max(long a, long b)
        {
            return a > b ? a : b;
        }
    }

}