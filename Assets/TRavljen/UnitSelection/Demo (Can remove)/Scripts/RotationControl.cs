using UnityEngine;

namespace TRavljen.UnitSelection.Demo
{
    /// <summary>
    /// Demonstration class, used for rotating game objects.
    /// </summary>
    public class RotationControl : MonoBehaviour
    {

        [Tooltip("Rotating direction")]
        public Vector3 RotatingAxis = Vector3.up;

        [Tooltip("Angle in degrees rotated in a single second.")]
        public float RotationSpeed = 90f;

        private void Update()
        {
            transform.Rotate(RotatingAxis, RotationSpeed * Time.deltaTime);
        }

    }
}