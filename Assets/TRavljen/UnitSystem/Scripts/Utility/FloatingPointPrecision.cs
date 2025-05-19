using UnityEngine;

namespace TRavljen.UnitSystem.Utility
{

    /// <summary>
    /// Provides methods for converting integer/long values to floating-point representations
    /// and formatted string outputs with configurable precision.
    /// </summary>
    public static class FloatingPointPrecision
    {

        /// <summary>
        /// Converts a long value into a floating-point number with the specified precision.
        /// </summary>
        /// <param name="value">The integer/long value to convert.</param>
        /// <param name="precision">The number of decimal places to represent. Default is 2.</param>
        /// <returns>
        /// A <see cref="float"/> representing the input value with the specified precision.
        /// </returns>
        /// <example>
        /// <code>
        /// long resourceValue = 10000; // Represents 100.00
        /// float displayedValue = FloatingPointPrecision.GetFloat(resourceValue, precision: 2);
        /// // displayedValue = 100.00
        /// </code>
        /// </example>
        public static float GetFloat(long value, int precision = 2)
        {
            float divider = Mathf.Pow(10, precision);
            return value / divider;
        }

        /// <summary>
        /// Converts a long value into a formatted string representation with integer and decimal parts.
        /// </summary>
        /// <param name="value">The integer/long value to convert.</param>
        /// <param name="precision">The number of decimal places to represent. Default is 2.</param>
        /// <returns>
        /// A <see cref="string"/> representing the value as "integer,decimal".
        /// </returns>
        /// <remarks>
        /// The string uses a comma (`,`) as the separator for integer and decimal parts.
        /// </remarks>
        /// <example>
        /// <code>
        /// long resourceValue = 123456; // Represents 1234.56
        /// string displayedString = FloatingPointPrecision.GetString(resourceValue, precision: 2);
        /// // displayedString = "1234,56"
        /// </code>
        /// </example>
        public static string GetString(long value, int precision = 2)
        {
            long power = (long)Mathf.Pow(10, precision);
            long decimalValue = value % power;
            long fullValue = value / power;

            return string.Format("{0},{1:D" + precision + "}", fullValue, decimalValue);
        }

    }

}