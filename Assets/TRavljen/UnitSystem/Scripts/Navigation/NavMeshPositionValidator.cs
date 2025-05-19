using System.Collections.Generic;
using UnityEngine;

namespace TRavljen.UnitSystem.Navigation
{
    using UnityEngine.AI;

    #if UNITY_EDITOR
    using UnityEditor;
    [CustomEditor(typeof(NavMeshPositionValidator))]
    internal class NavMeshPositionValidatorEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            EditorGUILayout.HelpBox("Validates world positions against the NavMesh to ensure they are within navigable areas.", MessageType.Info);
            base.OnInspectorGUI();
        }
    }
    #endif

    /// <summary>
    /// Validates world positions against the NavMesh to ensure they are within navigable areas.
    /// </summary>
    [ExecuteAlways]
    public class NavMeshPositionValidator : MonoBehaviour, IValidateWorldPositions
    {

        [Tooltip("The maximum allowed vertical deviation when validating positions.")]
        [SerializeField, Range(0, 20)]
        private float maxDeltaY = 5;
        
        [Tooltip("The maximum allowed horizontal deviation when validating positions.")]
        [SerializeField, Range(0, 20)]
        private float maxDeltaXZ = 1;

        [Tooltip("Determines whether position validation is enabled.")]
        [SerializeField]
        private bool validationEnabled = true;

        [SerializeField, HideInInspector]
        private float maxDistance;

        private void Awake()
        {
            maxDistance = Mathf.Max(maxDeltaXZ, maxDeltaY);
        }

        private void Start()
        {
            GroundValidationManager.GetOrCreate().SetValidator(this);
        }
        
        private void OnValidate()
        {
            maxDistance = Mathf.Max(maxDeltaXZ, maxDeltaY);
        }

        /// <summary>
        /// Validates an array of world positions, ensuring they are within valid NavMesh areas.
        /// </summary>
        /// <param name="positions">The array of positions to validate.</param>
        /// <returns>An array of validated positions that conform to the NavMesh constraints.</returns>
        public Vector3[] ValidatePositions(Vector3[] positions)
        {
            if (!validationEnabled)
                return positions;

            List<Vector3> validatedPoints = new();

            foreach (var position in positions)
            {
                if (ValidatePosition(position, out Vector3 validPos))
                    validatedPoints.Add(validPos);
            }

            return validatedPoints.ToArray();
        }

        /// <summary>
        /// Validates a world position, ensuring it is within a valid NavMesh area.
        /// </summary>
        /// <param name="position">Position to validate</param>
        /// <param name="validPosition">Validated position</param>
        /// <returns>Returns true if position was validated; otherwise false.</returns>
        public bool ValidatePosition(Vector3 position, out Vector3 validPosition)
        {
            if (NavMesh.SamplePosition(position, out NavMeshHit hit, maxDistance, NavMesh.AllAreas))
            {
                Vector3 diff = hit.position - position;
                float y = diff.y;
                diff.y = 0;

                if (Mathf.Abs(y) < maxDeltaY && diff.magnitude < maxDeltaXZ)
                {
                    validPosition = hit.position;
                    return true;
                }
            }

            validPosition = Vector3.one;
            return false;
        }
    }

}