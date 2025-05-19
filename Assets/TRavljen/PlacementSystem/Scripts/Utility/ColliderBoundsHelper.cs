using UnityEngine;

namespace TRavljen.PlacementSystem.Utility
{
    static public class ColliderBoundsHelper
    {
        /// <summary>
        /// Retrieving collider bounds while the collider itself is disabled
        /// will return Vector3 of zero size. This method will calculate it
        /// from collider center and size, while applying its transform's
        /// rotation and scale. The bounds position is still in local space.
        /// </summary>
        /// <param name="collider">Collider to calculate the Bounds for</param>
        /// <returns></returns>
        static public Bounds CalculateBounds(BoxCollider collider)
        {
            Vector3 center = collider.center;
            Vector3 size = collider.size * 0.5f;
            Quaternion rotation = collider.transform.rotation;

            // Apply lossy scale to get world size, but keep the position local.
            size = Vector3.Scale(size, collider.transform.lossyScale);

            Vector3[] points = new Vector3[8];
            points[0] = rotation * new Vector3(-size.x, -size.y, -size.z) + center;
            points[1] = rotation * new Vector3(size.x, -size.y, -size.z) + center;
            points[2] = rotation * new Vector3(-size.x, size.y, -size.z) + center;
            points[3] = rotation * new Vector3(size.x, size.y, -size.z) + center;
            points[4] = rotation * new Vector3(-size.x, -size.y, size.z) + center;
            points[5] = rotation * new Vector3(size.x, -size.y, size.z) + center;
            points[6] = rotation * new Vector3(-size.x, size.y, size.z) + center;
            points[7] = rotation * new Vector3(size.x, size.y, size.z) + center;

            Bounds bounds = new Bounds(points[0], Vector3.zero);
            foreach (Vector3 point in points)
            {
                bounds.Encapsulate(point);
            }

            return bounds;
        }
    }
}