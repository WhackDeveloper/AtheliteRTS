using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TRavljen.UnitSystem.Interactions.PredefinedPositions
{
    
    /// <summary>
    /// Utility class for generating evenly distributed positions around the bounds of a rectangle or cuboid.
    /// </summary>
    /// <remarks>
    /// This class is designed to assist in positioning units or objects along the edges of a rectangular boundary.
    /// It calculates positions based on the bounds, unit spacing, and an optional offset distance.
    /// </remarks>
    public static class RectanglePositioner
    {

        /// <summary>
        /// Generates positions along the perimeter of a rectangular bounds.
        /// </summary>
        /// <param name="bounds">The bounds defining the rectangular area.</param>
        /// <param name="unitRadius">The spacing between positions, based on the radius of the unit or object.</param>
        /// <param name="distance">An additional offset distance from the bounds. Defaults to 0.</param>
        /// <returns>A list of <see cref="Vector3"/> positions evenly distributed around the rectangle's perimeter.</returns>
        /// <remarks>
        /// The method calculates positions along the bottom edges of the rectangular bounds, ensuring that objects 
        /// are spaced evenly. It can also include an optional distance offset for positioning away from the bounds.
        /// </remarks>
        public static List<Vector3> GeneratePositionsAroundBounds(Bounds bounds, float unitRadius, float distance = 0)
        {
            List<Vector3> positions = new List<Vector3>();

            Vector3 extents = bounds.extents;

            // Define the bottom corners of the box
            Vector3[] corners = new Vector3[4];

            Vector3 cornerA = new Vector3(-extents.x, -extents.y, -extents.z);
            Vector3 cornerB = new Vector3(extents.x, -extents.y, -extents.z);
            Vector3 cornerC = new Vector3(extents.x, -extents.y, extents.z);
            Vector3 cornerD = new Vector3(-extents.x, -extents.y, extents.z);

            Vector3 GetModifierCorner(Vector3 corner)
            {
                // Position in local space with additional distance.
                Vector3 normalized = corner.normalized;
                normalized.y = 0;
                return bounds.center + (corner + normalized * distance);
            }

            corners[0] = GetModifierCorner(cornerA);
            corners[1] = GetModifierCorner(cornerB);
            corners[2] = GetModifierCorner(cornerC);
            corners[3] = GetModifierCorner(cornerD);

            // Generate positions along the edges of the bottom rectangle
            for (int i = 0; i < 4; i++)
            {
                Vector3 start = corners[i];
                Vector3 end = corners[(i + 1) % 4];

                positions.AddRange(GeneratePositionsAlongEdge(start, end, unitRadius));
            }

            return positions;
        }

        /// <summary>
        /// Generates positions evenly distributed along an edge between two points.
        /// </summary>
        /// <param name="start">The starting point of the edge.</param>
        /// <param name="end">The ending point of the edge.</param>
        /// <param name="unitRadius">The spacing between positions, based on the radius of the unit or object.</param>
        /// <returns>A list of <see cref="Vector3"/> positions along the edge.</returns>
        /// <remarks>
        /// The positions are calculated using linear interpolation between the start and end points.
        /// </remarks>
        private static List<Vector3> GeneratePositionsAlongEdge(Vector3 start, Vector3 end, float unitRadius)
        {
            List<Vector3> positions = new List<Vector3>();

            float distance = Vector3.Distance(start, end);
            int numPoints = Mathf.CeilToInt(distance / unitRadius);

            for (int i = 0; i < numPoints; i++)
            {
                float t = i / (float)numPoints;
                Vector3 position = Vector3.Lerp(start, end, t);
                positions.Add(position);
            }

            return positions;
        }

    }

}
