using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TRavljen.PlacementSystem.Utility
{
    using UnityEngine;

    static class MathHelper
    {

        /// <summary>
        /// Calculates the distance between two angles.
        /// </summary>
        public static float AngleDistance(float angle1, float angle2)
        {
            // Convert angles to the range of -180 to 180
            angle1 = WrapAngle(angle1);
            angle2 = WrapAngle(angle2);

            // Calculate the absolute difference between the angles
            float diff = Mathf.Abs(angle2 - angle1);

            // Choose the smallest angle between the two possibilities
            float distance = Mathf.Min(diff, 360 - diff);

            return distance;
        }

        /// <summary>
        /// Wraps the angle within a valid range for calculation (euler).
        /// This means the returned value will be within -180 and 180 range
        /// by modifying the value as required (+/- 360).
        /// </summary>
        public static float WrapAngle(float angle)
        {
            angle %= 360;
            if (angle > 180)
            {
                angle -= 360;
            }
            else if (angle < -180)
            {
                angle += 360;
            }
            return angle;
        }

    }
}