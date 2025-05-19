using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TRavljen.UnitSystem.Utility
{
    public static class TransformRotationUtils
    {
        /// <summary>
        /// Checks if the transform is looking into target direction.
        /// </summary>
        public static bool IsLookingAt(
            this Transform transform,
            Vector3 targetDirection,
            Vector3 rotationThreshold,
            bool useYAxisOnly = false)
        {
            if (targetDirection == Vector3.zero)
                return false;

            Quaternion targetRotation = Quaternion.LookRotation(targetDirection);
            Vector3 currentEuler = transform.rotation.eulerAngles;
            Vector3 targetEuler = targetRotation.eulerAngles;

            if (useYAxisOnly)
            {
                float deltaY = Mathf.DeltaAngle(currentEuler.y, targetEuler.y);
                return Mathf.Abs(deltaY) <= rotationThreshold.y;
            }
            else
            {
                float deltaX = Mathf.DeltaAngle(currentEuler.x, targetEuler.x);
                float deltaY = Mathf.DeltaAngle(currentEuler.y, targetEuler.y);
                float deltaZ = Mathf.DeltaAngle(currentEuler.z, targetEuler.z);

                return Mathf.Abs(deltaX) <= rotationThreshold.x &&
                       Mathf.Abs(deltaY) <= rotationThreshold.y &&
                       Mathf.Abs(deltaZ) <= rotationThreshold.z;
            }
        }
    }
}