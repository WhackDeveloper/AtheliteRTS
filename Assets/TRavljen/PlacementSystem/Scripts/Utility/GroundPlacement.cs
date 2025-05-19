namespace TRavljen.PlacementSystem.Utility
{
    using UnityEngine;

    /// <summary>
    /// Finds ground bottom points of the rotated bounds in world space and
    /// if needed, aligns the bounds to ground with new position and rotation.
    /// </summary>
    [System.Serializable]
    class GroundAlignment
    {

        [HideInInspector]
        [Range(-150, 150)]
        [Tooltip("Specifies the Y offset of raycast detection for ground position. " +
            "Detected points on bounds edges define the angle of object placement.")]
        public float GroundDetectionOffset = 10;

        public bool debugLogsEnabled = false;

        private Vector3[] bottomPoints = new Vector3[4];
        private Vector3 sum = new Vector3();
        private RaycastHit hit;

        public Vector3[] BottomPoints => bottomPoints;

        public GroundAlignment() { }

        /// <summary>
        /// Attempts to find the new position and rotation for the placing bounds.
        /// </summary>
        /// <param name="bounds">Bounds of placing object</param>
        /// <param name="currentRotation">Rotation of bounds in world space</param>
        /// <param name="groundLayer">Layer of ground to detect placement colliders (mesh or terrain).</param>
        /// <param name="newRotation">New rotation in world space, aligned to ground</param>
        /// <param name="newPosition">New position in world space, positioned on ground</param>
        /// <returns>Returns true if alignment was successful, returns false if it was not</returns>
        public bool TryGetAlignedPositionWithGround(
            Bounds bounds,
            Quaternion currentRotation,
            LayerMask groundLayer,
            out Quaternion newRotation,
            out Vector3 newPosition)
        {
            if (!FindGroundPoints(bounds, currentRotation, groundLayer, ref bottomPoints))
            {
                newPosition = Vector3.zero;
                newRotation = Quaternion.identity;
                return false;
            }

            sum.Set(0, 0, 0);

            for (int i = 0; i < bottomPoints.Length; i++)
            {
                sum += bottomPoints[i];
            }

            newPosition = sum / 4;

            Vector3 normal = CalculatePlaneNormal(bottomPoints);
            newRotation = Quaternion.FromToRotation(Vector3.up, normal);
            return true;
        }

        /// <summary>
        /// Takes the bottom 4 edges of the rotated bounds and finds the nearest
        /// ground below it using <see cref="Physics.Raycast(Vector3, Vector3, out RaycastHit, float, int)"/>.
        /// </summary>
        /// <param name="bounds">Bounds of placing object</param>
        /// <param name="rotation">Rotation of bounds in world space</param>
        /// <param name="groundLayer">Layer of ground to detect placement colliders (mesh or terrain).</param>
        /// <param name="points">Points updated if ground was found</param>
        /// <returns>Returns true if ground for all 4 edges was found.</returns>
        public bool FindGroundPoints(Bounds bounds, Quaternion rotation, LayerMask groundLayer, ref Vector3[] points)
        {
            GetBottomEdges(bounds.center, bounds.size, rotation, ref points);

            for (int i = 0; i < points.Length; i++)
            {
                var start = points[i] + new Vector3(0, GroundDetectionOffset, 0);
                float maxDistance = GroundDetectionOffset * 2;
                if (Physics.Raycast(start, Vector3.down, out hit, maxDistance, groundLayer))
                    points[i] = hit.point;
                else
                    // No valid ground for the edge of the collider
                    return false;
            }

            return true;
        }

        /// <summary>
        /// Calculates planes normal vector.
        /// </summary>
        static Vector3 CalculatePlaneNormal(Vector3[] points)
        {
            Vector3 edge1 = points[3] - points[1];
            Vector3 edge2 = points[2] - points[0];
            Vector3 normal = Vector3.Cross(edge1, edge2).normalized;
            return normal;
        }

        /// <summary>
        /// Calculates the bottom edges of a bounding box in world space.
        /// </summary>
        static void GetBottomEdges(Vector3 center, Vector3 size, Quaternion rotation, ref Vector3[] edges)
        {
            Vector3 extents = size * 0.5f;

            edges[0] = center + rotation * new Vector3(-extents.x, -extents.y, -extents.z);
            edges[1] = center + rotation * new Vector3(extents.x, -extents.y, -extents.z);
            edges[2] = center + rotation * new Vector3(extents.x, -extents.y, extents.z);
            edges[3] = center + rotation * new Vector3(-extents.x, -extents.y, extents.z);
        }
    }

}
