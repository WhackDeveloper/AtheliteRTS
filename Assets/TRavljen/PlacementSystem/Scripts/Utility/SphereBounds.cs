using UnityEngine;

namespace TRavljen.PlacementSystem
{
    /// <summary>
    /// Defines bounds of a sphere with <see cref="center"/> and <see cref="radius"/>.
    /// </summary>
    [System.Serializable]
    public struct SphereBounds
    {

        [Tooltip("Specifies bounds center.")]
        public Vector3 center;

        [Tooltip("Specifies bounds radius.")]
        [Range(0.01f, 200f)]
        public float radius;

        public SphereBounds(Vector3 center, float radius)
        {
            this.center = center;
            this.radius = radius;
        }

        /// <summary>
        /// Checks if a point is within the sphere.
        /// </summary>
        public bool Contains(Vector3 point)
        {
            return Vector3.Distance(center, point) <= radius;
        }

        /// <summary>
        /// Checks if another sphere intersects with this sphere.
        /// </summary>
        public bool Intersects(SphereBounds other)
        {
            float distance = Vector3.Distance(center, other.center);
            return distance <= (radius + other.radius);
        }

        /// <summary>
        /// Get a bounding box that contains this sphere
        /// </summary>
        public Bounds ToBounds()
        {
            Vector3 size = new Vector3(radius * 2, radius * 2, radius * 2);
            return new Bounds(center, size);
        }

        /// <summary>
        /// Finds the closest point on the sphere surface to the given point.
        /// </summary>
        public Vector3 ClosestPoint(Vector3 point)
        {
            Vector3 direction = point - center;
            if (direction.magnitude <= radius)
            {
                // The point is inside the sphere, return the point itself
                return point;
            }
            // Normalize the direction and scale by the radius to get the closest point on the surface
            return center + direction.normalized * radius;
        }
    }
}