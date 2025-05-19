namespace TRavljen.PlacementSystem.Utility
{
    using UnityEngine;

    /// <summary>
    /// Class used to calculate snapping positions for a grid based system.
    /// </summary
    [System.Serializable]
    sealed public class GridSnapper
    {

        [SerializeField]
        [Tooltip("Specifies the grid size (space between snapping points).")]
        private Vector2 gridScale;

        [SerializeField]
        [Tooltip("Specifies the origin of the snapping grid. Y axis is not used.")]
        private Vector3 gridOrigin;

        [SerializeField]
        [Tooltip("Specifies the color of the grid origin sphere gizmo.")]
        private Color gizmoGridOriginColor = Color.red;

        [SerializeField]
        [Tooltip("Specifies the color of the grid point spheres gizmo.")]
        private Color gizmoGridPointsColor = Color.blue;

        [SerializeField]
        [Tooltip("Specifies the point of placement on the grid. " +
            "Grid gizmos will follow this point to avoid rendering too many.")]
        private Color gizmoGridPlacementPointColor = new Color(1f, 165f / 255f, 0f);

        [SerializeField]
        [Range(0.1F, 10F)]
        [Tooltip("Specifies the sphere size for grid points.")]
        private float gizmosSphereSize = 0.25f;

        /// <summary>
        /// Clears Y axis from origin.
        /// </summary>
        private Vector3 ClearOrigin
        {
            get
            {
                Vector3 origin = gridOrigin;
                origin.y = 0;
                return origin;
            }
        }

        /// <summary>
        /// Max cell count for single row/column.
        /// If changed, this should always remain an even number.
        /// </summary>
        private const float MaxCellRowCount = 10;

        /// <summary>
        /// Specifies the last point on grid that was calculated.
        /// <see cref="Vector3.zero"/> is the default value.
        /// </summary>
        private Vector3 lastNereastPoint;

        /// <summary>
        /// Creates a grid of specified origin and size.
        /// </summary>
        public GridSnapper(Vector3 gridOrigin, Vector2 gridScale)
        {
            this.gridOrigin = gridOrigin;
            this.gridScale = gridScale;
        }

        /// <summary>
        /// Renders spheres of the grid points, used for snapping.
        /// </summary>
        public void RenderDebugPoints()
        {
            Gizmos.color = gizmoGridPointsColor;
            
            Vector3 gridOrigin = ClearOrigin;
            Vector3 origin = new Vector3(MaxCellRowCount * gridScale.x, lastNereastPoint.y, MaxCellRowCount * gridScale.y) / -2;

            bool isLastPointNotSet = Mathf.Approximately(0f, lastNereastPoint.magnitude);
            origin += isLastPointNotSet ? gridOrigin : lastNereastPoint;

            for (int i = 0; i < MaxCellRowCount + 1; i++)
            {
                for (int j = 0; j < MaxCellRowCount + 1; j++)
                {
                    Vector3 cellPos = origin + new Vector3(i * gridScale.x, lastNereastPoint.y, j * gridScale.y);
                    Gizmos.DrawSphere(cellPos, gizmosSphereSize);
                }
            }

            if (isLastPointNotSet)
            {
                // Draw origin sphere
                Gizmos.color = gizmoGridOriginColor;
                Gizmos.DrawSphere(gridOrigin, gizmosSphereSize * 1.4f);
            }
            else
            {
                // Draw last placement point here
                Gizmos.color = gizmoGridPlacementPointColor;
                Gizmos.DrawSphere(lastNereastPoint, gizmosSphereSize * 1.4f);
            }
        }

        /// <summary>
        /// Validates and clamps the <see cref="gridScale"/>.
        /// </summary>
        public void Validate()
        {
            gridScale.x = Mathf.Abs(gridScale.x);
            gridScale.y = Mathf.Abs(gridScale.y);
        }

        /// <summary>
        /// Snaps a position to the nearest grid point.
        /// </summary>
        public Vector3 SnapToGrid(Vector3 position)
        {
            Vector3 gridOrigin = ClearOrigin;

            // Calculate the difference from the origin
            Vector3 offset = position - gridOrigin;

            // Compute the closest grid index for each dimension
            int xCount = Mathf.RoundToInt(offset.x / gridScale.x);
            int zCount = Mathf.RoundToInt(offset.z / gridScale.y);

            // Calculate the nearest grid point based on the grid size
            Vector3 nearestPoint = new Vector3(xCount * gridScale.x, position.y, zCount * gridScale.y) + gridOrigin;
            return lastNereastPoint = nearestPoint;
        }
    }

}