using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Unity.Burst;
using UnityEngine;

namespace TRavljen.UnitSelection
{

    /// <summary>
    /// Screen Rectangle selection. This class represents the 2D rectangle on screen when
    /// selection is enabled and captures all objects that have position within the rectangle.
    /// This selection area depends on <see cref="UnitManager"/> as it retrieves list
    /// of possible selections from there.
    /// Supports different detection types that can be set with <see cref="detectionType"/>.
    /// </summary>
    public class RectangleSelectionArea : ASelectionArea
    {

        #region Configuration Properties

        [SerializeField]
        [Tooltip("Specifies the detection process used for detecting units. " +
            "Check type documentation for more information.")]
        private DetectionType detectionType = DetectionType.ScreenPosition;

        /// <summary>
        /// Specifies a custom fill texture. By default plain rectangle is used
        /// for most common purposes and tinted with <see cref="FillColor"/>.
        /// </summary>
        [SerializeField]
        [Tooltip("Specifies a custom fill texture. By default plain rectangle is used for most common purposes and tinted with 'FillColor'.")]
        private Texture2D customFillTexture;

        /// <summary>
        /// Specifies a custom border texture. By default plain rectangle is used
        /// for most common purposes and tinted with <see cref="BorderColor"/>.
        /// </summary>
        [SerializeField]
        [Tooltip("Specifies a custom fill texture. By default plain rectangle is used for most common purposes and tinted with 'BorderColor'.")]
        private Texture2D customBorderTexture;

        /// <summary>
        /// Specifies the color that will be applied as filling of the active
        /// selection rectangle. This will be applied on <see cref="customFillTexture"/>
        /// as well, for no effect use <see cref="Color.white"/>.
        /// </summary>
        public Color FillColor = Color.clear;

        /// <summary>
        /// Specifies the color that will be applied as border of the active
        /// selection rectangle. This will be applied on <see cref="customBorderTexture"/>
        /// as well, for no effect use <see cref="Color.white"/>.
        /// </summary>
        public Color BorderColor = Color.white;

        /// <summary>
        /// Specifies the thickness of the active selection rectangle border.
        /// </summary>
        [Range(0, 10)]
        public float BorderThickness = 1f;

        #endregion

        #region Private Properties

        /// <summary>
        /// Current mouse drag state.
        /// </summary>
        private MouseDragState state;

        private Texture2D whiteTexture;

        #endregion

        #region Lifecycle

        private void Awake()
        {
            whiteTexture = new Texture2D(1, 1);
            whiteTexture.SetPixel(0, 0, Color.white);
            whiteTexture.Apply();
        }

        private void OnGUI()
        {
            if (state.IsDragging)
            {
                DrawRectOnScreen();
            }
        }

        /// <summary>
        /// Updates the fill texture used for rectangle rendered for active screen selection.
        /// </summary>
        public void SetFillTexture(Texture2D newTexture)
            => customFillTexture = newTexture;

        /// <summary>
        /// Updates the border texture used for rectangle rendered for active screen selection.
        /// </summary>
        public void SetBorderTexture(Texture2D newTexture)
            => customBorderTexture = newTexture;

        #endregion

        #region SelectionArea

        public override bool ShouldMouseDragStartSelection(Vector2 startPosition)
        {
            state.StartingPosition = startPosition;
            state.IsDragging = true;

            state.Rect.position = new Vector2(
                startPosition.x,
                Screen.height - startPosition.y);
            return true;
        }

        public override void MouseDragContinues(Vector2 newPosition)
        {
            state.CurrentPosition = newPosition;

            state.Rect.size = new Vector2(
                newPosition.x - state.StartingPosition.x,
                state.StartingPosition.y - newPosition.y
            );
        }

        public override void MouseDragStops()
        {
            state = new MouseDragState();
        }

        public override List<ISelectable> GetCurrentObjectsWithinArea(bool sortByDistance)
        {
            // If using ManageUnitObject, unit manager is instantiated on demand,
            // when drag selectable unit is first spawned in world and
            // also destroyed when last one is removed.
            if (UnitManager == null) return new List<ISelectable>();

            List<SortUnit> unitsWithinSelectionArea;
            switch (detectionType)
            {
                case DetectionType.CustomBounds:
                    unitsWithinSelectionArea = PerformBoundsCheck(sortByDistance, CustomSelectionCheck);
                    break;

                case DetectionType.RendererBounds:
                    unitsWithinSelectionArea = PerformBoundsCheck(sortByDistance, RendererBoundsCheck);
                    break;

                case DetectionType.WorldPosition:
                    unitsWithinSelectionArea = PerformBoundsCheck(sortByDistance, WorldPositionCheck);
                    break;

                case DetectionType.ScreenPosition:
                    unitsWithinSelectionArea = PerformScreenPositionCheck(sortByDistance);
                    break;

                default:
                    throw new System.NotImplementedException("New detection type not implemented: " + detectionType.ToString());
            }

            if (sortByDistance)
            {
                // Sort units by distance in screen points, using distance from the mouse starting point
                unitsWithinSelectionArea.Sort((a, b) => a.Distance.CompareTo(b.Distance));
            }
            return unitsWithinSelectionArea.ConvertAll(pair => pair.Unit);
        }

        private List<SortUnit> PerformScreenPositionCheck(bool sortByDistance)
        {
            var unitsWithinSelectionArea = new List<SortUnit>();
            var checkDistance = MaxSelectionDistance > 0;

            var viewBounds = GetSelectionBounds(state.StartingPosition, state.CurrentPosition);

            // Go through the list of managed units and check whether
            // the units position on screen in within the rectangle bounds.
            // But first position must be casted from world to screen point.
            foreach (ISelectable selectable in UnitManager.GetSingleSelectableUnits())
            {
                var position = selectable.gameObject.transform.position;
                if (checkDistance &&
                    Vector3.Distance(position, Camera.transform.position) > MaxSelectionDistance)
                    continue;

                Vector3 screenPoint = Camera.WorldToScreenPoint(position);

                if (viewBounds.Contains(screenPoint))
                {
                    float distance = -1;
                    if (sortByDistance)
                        distance = Vector3.Distance(screenPoint, state.StartingPosition) * 100;

                    unitsWithinSelectionArea.Add(new SortUnit(selectable, (int)distance));
                }
            }

            return unitsWithinSelectionArea;
        }

        private List<SortUnit> PerformBoundsCheck(bool sortByDistance, System.Func<GameObject, Plane[], bool> frustumCheck)
        {
            var selectionBounds = GetSelectionBounds(state.StartingPosition, state.CurrentPosition);

            // Size should never be 0 as it can mess with frustum plane normals
            // when testing AABB. Keep at minimum at 1 pixel.
            selectionBounds.width = Mathf.Max(selectionBounds.width, 1);
            selectionBounds.height = Mathf.Max(selectionBounds.height, 1);

            // Normalize
            selectionBounds = new Rect(
                selectionBounds.x / Screen.width,
                selectionBounds.y / Screen.height,
                selectionBounds.width / Screen.width,
                selectionBounds.height / Screen.height
            );

            // Calculate frustum planes for selection
            Plane[] planes = Utility.FrustumUtils.GetPlanes(Camera, selectionBounds, MaxSelectionDistance);

            List<SortUnit> unitsWithinSelectionArea = new List<SortUnit>();

            foreach (ISelectable selectable in UnitManager.GetSingleSelectableUnits())
            {
                if (!frustumCheck(selectable.gameObject, planes))
                    continue;

                int distance = -1;
                if (sortByDistance)
                {
                    var position = selectable.gameObject.transform.position;
                    Vector3 screenPoint = Camera.WorldToScreenPoint(position);
                    distance = (int)(Vector3.Distance(screenPoint, state.StartingPosition) * 100);
                }

                unitsWithinSelectionArea.Add(new SortUnit(selectable, distance));
            }

            return unitsWithinSelectionArea;
        }

        #endregion

        #region Helpers

        private bool WorldPositionCheck(GameObject obj, Plane[] planes)
        {
            return IsPointInsideFrustum(obj.transform.position, planes);
        }

        private static bool RendererBoundsCheck(GameObject obj, Plane[] planes)
        {
            var renderer = obj.GetComponentInChildren<Renderer>();
            return renderer != null & GeometryUtility.TestPlanesAABB(planes, renderer.bounds);
        }

        private static bool CustomSelectionCheck(GameObject obj, Plane[] planes)
        {
            var selection = obj.GetComponentInChildren<ISelectionBounds>();
            return selection != null && GeometryUtility.TestPlanesAABB(planes, selection.SelectionBounds);
        }

        /// <summary>
        /// Draws the mouse drag rectangle on the screen to represent the
        /// selection area to the player.
        /// </summary>
        private void DrawRectOnScreen()
        {
            var rect = state.Rect;

            // Draw rectangle
            GUI.color = FillColor;

            GUI.DrawTexture(rect, customFillTexture != null ? customFillTexture : whiteTexture);

            // Draw borders if thickness is larger than zero
            if (BorderThickness > 0)
            {
                GUI.color = BorderColor;
                var borderLines = new List<Rect>()
                {
                    new(rect.xMin, rect.yMin, rect.width, BorderThickness),
                    new(rect.xMin, rect.yMin, BorderThickness, rect.height),
                    new(rect.xMax - BorderThickness, rect.yMin, BorderThickness, rect.height),
                    new(rect.xMin, rect.yMax - BorderThickness, rect.width, BorderThickness)
                };

                var texture = customBorderTexture != null ? customBorderTexture : whiteTexture;
                foreach (var borderRect in borderLines)
                {
                    GUI.DrawTexture(borderRect, texture);
                }
            }

            GUI.color = Color.white;
        }

        /// <summary>
        /// Creates Bounds for the selection area based on current mouse drag
        /// state and uses camera to convert mouse input screen points to the
        /// viewport points used for Bounds.
        /// </summary>
        private static Rect GetSelectionBounds(Vector3 startPos, Vector3 endPos)
        {
            var minX = Mathf.Max(0, Mathf.Min(startPos.x, endPos.x));
            var maxX = Mathf.Min(Screen.width, Mathf.Max(startPos.x, endPos.x));
            var minY = Mathf.Max(0, Mathf.Min(startPos.y, endPos.y));
            var maxY = Mathf.Min(Screen.height, Mathf.Max(startPos.y, endPos.y));

            Rect rect = new Rect(minX, minY, maxX - minX, maxY - minY);

            // Size should never be 0 as it can mess with AABB or bounds tests.
            rect.width = Mathf.Max(rect.width, 1);
            rect.height = Mathf.Max(rect.height, 1);

            return rect;
        }

        private static bool IsPointInsideFrustum(Vector3 point, Plane[] planes)
        {
            foreach (Plane plane in planes)
            {
                if (plane.GetDistanceToPoint(point) < 0)
                {
                    return false;
                }
            }

            return true;
        }

        #endregion

        /// <summary>
        /// Data structure for mouse dragging. It keeps all the data
        /// that is necessary for calculating the rectangle area for mouse
        /// selection.
        /// </summary>
        struct MouseDragState
        {

            /// <summary>
            /// Initial position of the mouse drag.
            /// </summary>
            public Vector3 StartingPosition;

            /// <summary>
            /// Current position of the mouse drag.
            /// </summary>
            public Vector2 CurrentPosition;

            /// <summary>
            /// Is mouse down/dragging.
            /// </summary>
            public bool IsDragging;

            /// <summary>
            /// Rectangle representing the selection space of the mouse drag.
            /// </summary>
            public Rect Rect;

        }

        /// <summary>
        /// Rectangle area supported detection types. <see cref="RendererBounds"/>
        /// and <see cref="CustomBounds"/> do not consider rotation as they use
        /// AABB tests.
        /// If you have units that are differ in X and Z a lot, consider using
        /// <see cref="WorldPosition"/> or <see cref="ScreenPosition"/>.
        /// </summary>
        enum DetectionType
        {
            /// <summary>
            /// Transforms units world to screen position and does a test if position
            /// is within the selection area.
            /// </summary>
            ScreenPosition,
            /// <summary>
            /// Creates frustum for selection area at max distance and checks if units
            /// world position is within it.
            /// </summary>
            WorldPosition,
            /// <summary>
            /// Creates frustum for selection area at max distance and does AABB test
            /// for renderer bounds. Renderer is retrieved by
            /// <see cref="GameObject.GetComponentInChildren(Renderer)"/>.
            /// Rotation is ignored.
            /// </summary>
            RendererBounds,
            /// <summary>
            /// Creates frustum for selection area at max distance and does AABB test
            /// for the custom selection bounds. Rotation is ignored.
            /// </summary>
            CustomBounds,
        }

    }

}