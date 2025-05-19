using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TRavljen.UnitSystem.Interactions.PredefinedPositions
{

    /// <summary>
    /// Utility class for generating evenly distributed positions around a sphere collider or circular area.
    /// </summary>
    /// <remarks>
    /// This class is designed to assist in positioning units or objects around a circular boundary. 
    /// It calculates positions based on the radius of the sphere and the spacing required between units.
    /// </remarks>
    public static class CirclePositioner
    {

        /// <summary>
        /// Generates positions around a sphere collider or circular area.
        /// </summary>
        /// <param name="center">The center point of the circle in local space.</param>
        /// <param name="radius">The radius of the circle or sphere collider.</param>
        /// <param name="scale">The scale of the object. Used to adjust the radius dynamically.</param>
        /// <param name="unitRadius">The radius of the unit or object to be positioned.</param>
        /// <param name="additionalDistance">Additional distance to be added to the calculated radius. Defaults to 0.</param>
        /// <returns>
        /// A list of <see cref="Vector3"/> positions representing evenly distributed points around the circle.
        /// </returns>
        /// <remarks>
        /// The method dynamically adjusts the radius based on the object's scale, ensuring correct spacing. 
        /// It then calculates the number of points required to evenly distribute the units or objects around the circle.
        /// </remarks>
        public static List<Vector3> GeneratePositionsAroundSphereCollider(
            Vector3 center,
            float radius,
            Vector3 scale,
            float unitRadius,
            float additionalDistance = 0)
        {
            List<Vector3> positions = new();

            float maxScale = Mathf.Max(scale.x, scale.y, scale.z);
            float distance = radius * maxScale + additionalDistance;

            // Calculate the number of points around the sphere's circumference
            int numPoints = Mathf.CeilToInt(2 * Mathf.PI * distance / (unitRadius * 2));

            for (int i = 0; i < numPoints; i++)
            {
                float angle = i * (2 * Mathf.PI / numPoints);
                Vector3 localPosition = new Vector3(
                    distance * Mathf.Cos(angle),
                    0,
                    distance * Mathf.Sin(angle)
                );

                positions.Add(center + localPosition);
            }

            return positions;
        }
    }

}