using System.Collections.Generic;
using UnityEngine;
using TRavljen.UnitSelection.Utility;

namespace TRavljen.UnitSelection
{

    /// <summary>
    /// World Cube selection. This class represents the 3D Cube in Scene when
    /// selection is enabled.
    /// Supports different detection types that can be set with <see cref="detectionType"/>.
    /// </summary>
    public class CubeSelectionArea : ASelectionArea
    {
        
        #region Properties

        /// <summary>
        /// Specifies the layer mask that will be used for detecting the ground
        /// where the selection will occur. If there is no layer selected,
        /// selection will not work, because raycasting will not hit any objects.
        /// </summary>
        [SerializeField]
        private LayerMask groundLayerMask = ~0;

        [SerializeField]
        [Tooltip("Specifies the detection process used for detecting units. " +
            "Check type documentation for more information.")]
        private DetectionType detectionType = DetectionType.Collision;

        [SerializeField]
        [Range(0.05f, 5f)]
        [Tooltip("Specifies minimal height of the selection cube. Cube height " +
            "changes based on Y change in start and current drag positions.")]
        private float minimalHeight = 0.5f;

        /// <summary>
        /// Current mouse drag state.
        /// </summary>
        private MouseDragState state = new MouseDragState();

        #endregion

        #region Lifecycle

        private void Awake()
        {
            // Make sure the rotation is setup properly.
            transform.rotation = Quaternion.identity;
        }

        private void Update()
        {
            if (state.IsDragging)
            {
                UpdateTransform();
            }
        }

        #endregion

        public override bool ShouldMouseDragStartSelection(Vector2 startPosition)
        {
            // Get starting position once its enabled
            if (!GetGroundRaycastHit(startPosition, out RaycastHit hit))
                return false;

            gameObject.SetActive(true);

            state = new MouseDragState(hit.point);

            UpdateTransform();

            return true;
        }

        public override void MouseDragContinues(Vector2 newPosition)
        {
            if (!GetGroundRaycastHit(newPosition, out RaycastHit hit)) return;

            Vector3 diff = state.StartingPosition - hit.point;

            // Scale should always be positive. Add extra 0.1f to make sure that
            // the scale goes over the hit objects, because on straight planes
            // it will overlap with the ground and have rendering "glitches".
            float newScaleY = Mathf.Max(minimalHeight, Mathf.Abs(diff.y)) + 0.1f;

            // Rescale the selection object
            state.Scale =
                new Vector3(Mathf.Abs(diff.x), newScaleY, Mathf.Abs(diff.z));

            // Calculate point/center between starting and current drag point
            Vector3 newCenter = state.StartingPosition - diff / 2;

            // Reposition the selection object
            state.Center = newCenter;
        }

        public override void MouseDragStops()
        {
            state = new MouseDragState();

            gameObject.SetActive(false);

            // Update transform manually as it will not be triggered in the
            // update function.
            UpdateTransform();
        }

        public override List<ISelectable> GetCurrentObjectsWithinArea(bool sortByDistance)
        {
            // If drag is not active, return empty list
            if (!gameObject.activeSelf) return new List<ISelectable>();

            if (UnitManager == null)
                throw new System.ArgumentNullException("UnitManager", "Reference was not set before trying to find units within the area!");

            List<SortUnit> hits = GetUnits(sortByDistance);

            // Sort units by distance from starting position.
            // Closest unit to the start position is at the start of the list.
            if (sortByDistance)
            {
                hits.Sort((a, b) => a.Distance.CompareTo(b.Distance));
            }
            return hits.ConvertAll(pair => pair.Unit);
        }

        private List<SortUnit> GetUnits(bool sortByDistance)
        {
            switch (detectionType)
            {
                case DetectionType.Collision:
                    return PerformBoxOverlap(sortByDistance);

                case DetectionType.Position:
                    return PerformCheck(sortByDistance, PositionCheck);

                case DetectionType.RendererBounds:
                    return PerformCheck(sortByDistance, RendererCheck);

                case DetectionType.CustomBounds:
                    return  PerformCheck(sortByDistance, CustomSelectionCheck);

                default:
                    throw new System.NotImplementedException("New detection type not implemented: " + detectionType.ToString());
            }
        }

        #region Helpers

        private List<SortUnit> PerformBoxOverlap(bool sortByDistance)
        {
            List<SortUnit> collisionHits = new List<SortUnit>();

            Collider[] colliders = Physics.OverlapBox(
                        state.Center,
                        state.Scale / 2,
                        Quaternion.identity,
                        SelectableLayerMask);

            // Filter out any units that are not provided by the manager
            foreach (Collider collider in colliders)
            {
                if (collider.TryGetComponent(out ISelectable selectable))
                {
                    float distance = -1;
                    if (sortByDistance)
                        distance = Vector3.Distance(collider.transform.position, state.StartingPosition) * 100;

                    collisionHits.Add(new SortUnit(selectable, (int)distance));
                }
            }

            return collisionHits;
        }

        private List<SortUnit> PerformCheck(bool sortByDistance, System.Func<Bounds, GameObject, bool> IsWithinSelection)
        {
            List<SortUnit> units = new List<SortUnit>();
            Bounds bounds = new Bounds(state.Center, state.Scale);

            foreach (var selectable in UnitManager.GetSingleSelectableUnits())
            {
                if (!IsWithinSelection(bounds, selectable.gameObject))
                    continue;

                float distance = -1;
                if (sortByDistance)
                    distance = Vector3.Distance(selectable.gameObject.transform.position, state.StartingPosition) * 100;

                units.Add(new SortUnit()
                {
                    Unit = selectable,
                    Distance = (int)distance
                });
            }

            return units;
        }

        /// <returns>Returns true if the objects position is within bounds</returns>
        private bool PositionCheck(Bounds bounds, GameObject obj)
        {
            Vector3 pos = obj.transform.position;
            return bounds.Contains(pos);
        }

        /// <returns>Returns true if bounds intersects with renderer bounds</returns>
        private bool RendererCheck(Bounds bounds, GameObject obj)
        {
            Renderer renderer = obj.GetComponentInChildren<Renderer>();
            return bounds.Intersects(renderer.bounds);
        }

        /// <returns>Returns true if bounds intersects with <see cref="ISelectionBounds.SelectionBounds"/></returns>
        private bool CustomSelectionCheck(Bounds bounds, GameObject obj)
        {
            ISelectionBounds selectionBounds = obj.GetComponentInChildren<ISelectionBounds>();
            return bounds.Intersects(selectionBounds.SelectionBounds);
        }

        /// <summary>
        /// Updates position and local scale on the transform component.
        /// </summary>
        private void UpdateTransform()
        {
            transform.position = state.Center;
            transform.localScale = state.Scale;
        }

        /// <returns>Returns true if the mouse position hits the ground</returns>
        private bool GetGroundRaycastHit(Vector3 mousePosition, out RaycastHit hit)
        {
            Ray ray = Camera.ScreenPointToRay(mousePosition);
            return Physics.Raycast(ray, out hit, MaxSelectionDistance, groundLayerMask);
        }

        #endregion

        /// <summary>
        /// Data structure for mouse dragging. It keeps all the data
        /// that is necessary for calculating the cube area for mouse selection.
        /// </summary>
        struct MouseDragState
        {

            /// <summary>
            /// Initial position of the mouse drag.
            /// </summary>
            public Vector3 StartingPosition;

            /// <summary>
            /// Center between starting position and the current mouse
            /// position.
            /// </summary>
            public Vector3 Center;

            /// <summary>
            /// Scale or size that is required to cover entire
            /// selection area from <see cref="Center"/>.
            /// </summary>
            public Vector3 Scale;

            /// <summary>
            /// Is mouse down/dragging.
            /// </summary>
            public bool IsDragging;

            public MouseDragState(Vector3 startingPosition)
            {
                StartingPosition = startingPosition;
                Center = startingPosition;
                Scale = Vector3.zero;
                IsDragging = true;
            }

        }

        /// <summary>
        /// Cubes supported detection types. <see cref="RendererBounds"/> and
        /// <see cref="CustomBounds"/> do not consider rotation at this moment.
        /// If you have units that are differ in X and Z a lot, consider using
        /// <see cref="Position"/> or <see cref="Collision"/>.
        /// </summary>
        enum DetectionType
        {
            /// <summary>
            /// Uses physics overlap method to detect colliders within.
            /// Might not be best choice if camera view can be big with thousands of units for selection.
            /// </summary>
            Collision,
            /// <summary>
            /// Checks if selection area bounds contain the position of the unit.
            /// </summary>
            Position,
            /// <summary>
            /// Checks if selection area bounds intersect with renderer bounds.
            /// </summary>
            RendererBounds,
            /// <summary>
            /// Checks if selection area bounds intersect with <see cref="ISelectionBounds.SelectionBounds"/>.
            /// To use this, your selectable game object must also contain one of the provided solutions
            /// (<see cref="ColliderSelectionBounds"/>, <see cref="CustomSelectionBounds"/>),
            /// <see cref="RendererSelectionBounds"/> or your own implementation of <see cref="ISelectionBounds"/>.
            /// </summary>
            CustomBounds
        }

    }

}