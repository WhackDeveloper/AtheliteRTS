using UnityEngine;

namespace TRavljen.Utility
{

    /// <summary>
    /// Specifies an integer range by defining its lower and upper bounds with
    /// <see cref="min"/> and <see cref="max"/> values.
    /// </summary>
    [System.Serializable]
    public struct IntRange
    {
        /// <summary>
        /// Minimal/lower bound of the integer range.
        /// </summary>
        public int min;

        /// <summary>
        /// Maximal/upper bound of the integer range.
        /// </summary>
        public int max;

        public IntRange(int min, int max)
        {
            this.min = min;
            this.max = max;
        }

        /// <summary>
        /// Clamps the value within the range.
        /// </summary>
        /// <param name="value">Value to clamp</param>
        /// <returns>Returns a value within the Integer range</returns>
        public float Clamp(float value) => Mathf.Clamp(value, min, max);

        /// <summary>
        /// Checks if the value is within the integer range/bounds. Min and max
        /// values are inclusive.
        /// </summary>
        /// <param name="value">Value to check</param>
        /// <returns>Returns true if the value is within integer range.</returns>
        public bool IsWithinBounds(float value) => min <= value && value <= max;

    }

}