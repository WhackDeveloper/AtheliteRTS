using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TRavljen.UnitSelection.Utility
{
    public static class FrustumUtils
    {

        /// <summary>
        /// Calculates frustum planes based on camera configuration.
        /// </summary>
        /// <param name="camera">Camera from which the near/far planes are calculated</param>
        /// <param name="rect">Normalized rectangle for custom planes</param>
        /// <param name="overrideFarDistance">
        /// To override cameras far plan distance, pass a value here. Negative values are ignored.
        /// </param>
        /// <returns>Returns caclulated frustum planes.</returns>
        public static Plane[] GetPlanes(Camera camera, Rect rect, float overrideFarDistance = -1)
        {
            float farDistance = overrideFarDistance > 0 ? overrideFarDistance : camera.farClipPlane;
            var (nearCorners, farCorners) = GetNearFarCorners(camera, rect, farDistance);
            return CreateCustomPlanes(nearCorners, farCorners);
        }

        /// <summary>
        /// Gear near and far corners of a plane for specified position and size; 2 sides of bounds.
        /// </summary>
        /// <param name="center">Center world position</param>
        /// <param name="size">World scale</param>
        /// <param name="rotation">World rotation</param>
        /// <returns>Calculated near and far corners</returns>
        public static (Vector3[] nearCorners, Vector3[] farCorners) GetNearFarCorners(Vector3 center, Vector3 size, Quaternion rotation)
        {
            Vector3 halfExtents = size / 2.0f;

            Vector3[] nearCorners = new Vector3[4];
            Vector3[] farCorners = new Vector3[4];

            // Front face corners
            Vector3 frontBottomLeft = center + rotation * new Vector3(-halfExtents.x, -halfExtents.y, -halfExtents.z);
            Vector3 frontBottomRight = center + rotation * new Vector3(halfExtents.x, -halfExtents.y, -halfExtents.z);
            Vector3 frontTopLeft = center + rotation * new Vector3(-halfExtents.x, halfExtents.y, -halfExtents.z);
            Vector3 frontTopRight = center + rotation * new Vector3(halfExtents.x, halfExtents.y, -halfExtents.z);

            // Back face corners
            Vector3 backBottomLeft = center + rotation * new Vector3(-halfExtents.x, -halfExtents.y, halfExtents.z);
            Vector3 backBottomRight = center + rotation * new Vector3(halfExtents.x, -halfExtents.y, halfExtents.z);
            Vector3 backTopLeft = center + rotation * new Vector3(-halfExtents.x, halfExtents.y, halfExtents.z);
            Vector3 backTopRight = center + rotation * new Vector3(halfExtents.x, halfExtents.y, halfExtents.z);

            farCorners[0] = frontBottomLeft;
            farCorners[1] = frontBottomRight;
            farCorners[2] = frontTopRight;
            farCorners[3] = frontTopLeft;

            nearCorners[0] = backBottomLeft;
            nearCorners[1] = backBottomRight;
            nearCorners[2] = backTopRight;
            nearCorners[3] = backTopLeft;

            return (nearCorners, farCorners);
        }

        /// <summary>
        /// Gear near and far corners of a plane for specified screen area.
        /// </summary>
        /// <param name="camera">Camera from which the near/far planes are calculated</param>
        /// <param name="normalisedScreenRect">Screen area defined by a rectangle, this value should be normalized</param>
        /// <param name="farDistance">Far distance of the frustum</param>
        /// <returns>Calculated near and far corners</returns>
        public static (Vector3[] nearCorners, Vector3[] farCorners) GetNearFarCorners(Camera camera, Rect normalisedScreenRect, float farDistance)
        {
            // Calculate far and near frustum corners of the camera for the specified 'selectionBox'
            Vector3[] nearCorners = new Vector3[4];
            Vector3[] farCorners = new Vector3[4];

            camera.CalculateFrustumCorners(normalisedScreenRect, camera.nearClipPlane, Camera.MonoOrStereoscopicEye.Mono, nearCorners);
            camera.CalculateFrustumCorners(normalisedScreenRect, camera.farClipPlane, Camera.MonoOrStereoscopicEye.Mono, farCorners);

            // Adjust corners position to world space
            for (int i = 0; i < 4; i++)
            {
                nearCorners[i] = camera.transform.TransformPoint(nearCorners[i]);

                Vector3 farCorner = farCorners[i];
                farCorner = camera.transform.TransformPoint(farCorner);

                // More far corners to the desired far distance
                Vector3 rayDirection = (farCorner - camera.transform.position).normalized;
                farCorners[i] = nearCorners[i] + rayDirection * farDistance;
            }

            return (nearCorners, farCorners);
        }

        /// <returns> Creates all 6 planes for frustum from near and far corners. </returns>
        public static Plane[] CreateCustomPlanes(Vector3[] nearCorners, Vector3[] farCorners)
        {
            Plane[] planes = new Plane[6];

            planes[0] = new Plane(nearCorners[0], nearCorners[1], farCorners[0]); // Left Plane
            planes[1] = new Plane(nearCorners[2], nearCorners[3], farCorners[3]); // Right Plane
            planes[2] = new Plane(nearCorners[3], nearCorners[0], farCorners[3]); // Bottom Plane
            planes[3] = new Plane(nearCorners[1], nearCorners[2], farCorners[2]); // Top Plane
            planes[4] = new Plane(nearCorners[1], nearCorners[0], nearCorners[2]); // Near Plane
            planes[5] = new Plane(farCorners[2], farCorners[3], farCorners[1]); // Far Plane

            return planes;
        }
    }
}