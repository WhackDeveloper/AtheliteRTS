using UnityEngine;

namespace TRavljen.UnitSystem.Interactions.PredefinedPositions
{
    using TRavljen.UnitSystem.Navigation;

    /// <summary>
    /// A class that generates predefined positions around a target area.
    /// The target area can be a sphere or a box, and the positions can be generated at a specified distance.
    /// </summary>
    [System.Serializable, DisallowMultipleComponent]
    public class PredefinedPositionGenerator: IPositionGenerator
    {

        /// <summary>
        /// The shape used for positioning: either a sphere or a box.
        /// </summary>
        [System.Serializable]
        private enum Shape { Sphere, Box }

        #region Properties

        [SerializeField]
        [Tooltip("Enables visualization of generated positions and bounds in the editor.")]
        private bool showGizmos = false;

        [SerializeField]
        [Tooltip("Enables gizmos of generated positions while in play mode.")]
        private bool enableGizmosInPlayMode = true;

        [SerializeField]
        [Tooltip("The shape of the target area (sphere or box).")]
        private Shape shape = Shape.Sphere;

        [SerializeField]
        [Tooltip("Defines the dimensions of the rectangular shape.")]
        private Vector3 rectangleSize = new(4, 0, 4);

        [SerializeField]
        [Tooltip("Offsets the center of the shape from the transform position.")]
        private Vector3 shapeOffset = new();

        [SerializeField, PositiveFloat]
        [Tooltip("Distance used for generating preview positions in the editor.")]
        private float previewDistance = 0f;

        [SerializeField]
        [Tooltip("When this is enabled and distance reaches thresholdForSphere," +
            "it will start using sphere shape. By default this is disabled.")]
        private bool boxToSphere = false;

        [SerializeField, Range(1, 100)]
        [Tooltip("Distance threshold after which positions for boxes are generated as rings (sphere).")]
        private float thresholdForSphere = 2;

        [SerializeField]
        [Tooltip("Radius of the spherical shape for position generation.")]
        private float sphereRadius = 2f;

        [SerializeField, Range(0.1f, 10f)]
        [Tooltip("Spacing between units when generating positions.")]
        private float unitSpacing = 1f;

        [SerializeField]
        [Tooltip("Radius of gizmos representing generated positions.")]
        private float gizmosRadius = 0.3f;

        [SerializeField, HideInInspector]
        public Transform transform;

        /// <summary>
        /// Computed bounds of the target area based on its size and configuration.
        /// </summary>
        public Bounds Bounds
        {
            get
            {
                Vector3 scale = transform.lossyScale;
                Vector3 offset = Vector3.Scale(shapeOffset, scale);
                Vector3 size = shape switch
                {
                    Shape.Box => Vector3.Scale(rectangleSize, scale),
                    Shape.Sphere => Vector3.Scale(Vector3.one * sphereRadius, scale),
                    _ => Vector3.one,
                };

                return new Bounds(transform.position + transform.rotation * offset, size);
            }
        }

        #endregion

        /// <summary>
        /// Initializes a new instance of the <see cref="PredefinedPositionGenerator"/> class.
        /// </summary>
        /// <param name="transform">The transform of the target area.</param>
        public PredefinedPositionGenerator(Transform transform)
        {
            if (transform == null)
                throw new System.ArgumentNullException("Transform is required for generating positions, null was passed");

            this.transform = transform;
        }

        #region Positioning

        /// <summary>
        /// Attempts to generate positions around the target area within the specified range.
        /// </summary>
        /// <param name="distance">The range within which positions should be generated.</param>
        /// <param name="result">
        /// An array of positions generated within the specified range. Returns an empty array if no positions are generated.
        /// </param>
        /// <returns>True if positions were generated successfully; otherwise, false.</returns>
        public bool TryGeneratePositionsInRange(float distance, out Vector3[] result)
        {
            Shape shape = this.shape;

            float sphereRadius = this.sphereRadius;
            Vector3 lossyScale = transform.lossyScale;
            Vector3 offset = Vector3.Scale(shapeOffset, lossyScale);

            switch (shape)
            {
                case Shape.Sphere:
                    result = CirclePositioner
                        .GeneratePositionsAroundSphereCollider(offset, sphereRadius, lossyScale, unitSpacing, distance)
                        .ToArray();
                    result = ValidatePositions(result);
                    return true;

                case Shape.Box:
                    Vector3 size = Vector3.Scale(rectangleSize, lossyScale);

                    // Starts creating ring positions instead of any other, once certain
                    // distance is reached.
                    if (boxToSphere && distance > thresholdForSphere)
                    {
                        // Calculate optimal radius
                        float maxSide = Mathf.Max(rectangleSize.x, rectangleSize.z);
                        float radius = Mathf.Sqrt(Mathf.Pow(maxSide, 2) * 2) / 2;

                        // Use offset to move positions down to same position as with rectangle
                        offset.y -= rectangleSize.y / 2 * lossyScale.y;
                        result = CirclePositioner
                            .GeneratePositionsAroundSphereCollider(offset, radius, lossyScale, unitSpacing, distance)
                            .ToArray();
                        return true;
                    }

                    Bounds bounds = new(offset, size);
                    result = RectanglePositioner.GeneratePositionsAroundBounds(bounds, unitSpacing, distance).ToArray();
                    result = ValidatePositions(result);
                    return true;
            }

            // No result.
            result = default;
            return false;
        }

        /// <summary>
        /// Validates the generated positions to ensure they are valid on the navigation surface.
        /// </summary>
        /// <param name="positions">The positions to validate.</param>
        /// <returns>The validated positions.</returns>
        private Vector3[] ValidatePositions(Vector3[] positions)
        {
            GroundValidationManager manager = GroundValidationManager.GetOrCreate();

            // Convert to world positions
            for (int index = 0; index < positions.Length; index++)
            {
                Vector3 worldPos = transform.position + transform.rotation * positions[index];
                positions[index] = worldPos;
            }

            // Validate positions based on any Navigation System available.
            positions = manager.ValidatePositions(positions);

            // Revert back, duo to any potential movement
            for (int index = 0; index < positions.Length; index++)
            {
                Vector3 localPos = Quaternion.Inverse(transform.rotation) * (positions[index] - transform.position);
                positions[index] = localPos;
            }

            return positions;
        }

        /// <summary>
        /// Draws gizmos in the editor to visualize the target area's shape and generated positions.
        /// </summary>
        public void DrawGizmos()
        {
            // Do this when in Editor, not playing only.
            if (!showGizmos) return;

            // Either its not playing, or its enabled it play mode.
            if (!Application.isPlaying || enableGizmosInPlayMode)
            {
                Gizmos.color = Color.white;

                if (TryGeneratePositionsInRange(previewDistance, out Vector3[] result))
                {
                    foreach (var position in result)
                    {
                        Vector3 worldPos = transform.position + transform.rotation * position;
                        Gizmos.DrawSphere(worldPos, gizmosRadius);
                    }
                }
            }

            Matrix4x4 current = Gizmos.matrix;
            Gizmos.matrix = Matrix4x4.TRS(transform.position, transform.rotation, transform.lossyScale);

            switch (shape)
            {
                case Shape.Sphere:
                    Gizmos.DrawWireSphere(shapeOffset, sphereRadius);
                    break;

                case Shape.Box:
                    Gizmos.DrawWireCube(shapeOffset, rectangleSize);
                    break;
            }

            Gizmos.matrix = current;
        }

        #endregion
    }

}