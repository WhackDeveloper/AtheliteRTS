using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TRavljen.UnitSelection
{

    /// <summary>
    /// Defines the types of raycast supported for hover and click.
    /// </summary>
    internal enum SelectionRaycastType {
        /// <summary>
        /// Performs raycast with single hit result.
        /// </summary>
        SingleHit,
        /// <summary>
        /// Performs raycast by capturing all hits in the ray and finds the
        /// nearest object hit.
        /// </summary>
        Nearest,
        /// <summary>
        /// Performs raycast by capturing all hits in the ray and finds the
        /// furthest object hit.
        /// </summary>
        Furthest
    }

    [System.Serializable]
    public struct SelectorConfiguration
    {

        /// <summary>
        /// Specifies the layer mask that will be used to detect game objects
        /// for selection.
        /// </summary>
        //[SerializeField]
        [Tooltip("Specifies the layer mask that will be used to " +
             "detect game objects for selection.")]
        public LayerMask SelectableLayerMask;

        /// <summary>
        /// Specifies if the mouse drag selects the units on mouse release.
        /// </summary>
        [SerializeField]
        [Tooltip("Specifies if the mouse drag selects the units on mouse release.")]
        public bool DragSelectionEnabled;

        /// <summary>
        /// Specifies if mouse drag highlights the units before mouse release.
        /// </summary>
        [SerializeField]
        [Tooltip("Specifies if mouse drag highlights the units before mouse release.")]
        public bool DragHighlightEnabled;

        /// <summary>
        /// Specifies if add and remove to current selection with mouse clicks
        /// -or add by dragging- is enabled.
        /// </summary>
        [SerializeField]
        [Tooltip("Specifies if add and remove to current selection " +
            "with mouse clicks -or add by dragging- is enabled.")]
        public bool ModifyingCurrentSelectionEnabled;

        /// <summary>
        /// Specifies if double clicking on an object triggers selection of all
        /// visible selectable objects.
        /// </summary>
        [SerializeField]
        [Tooltip("Specifies if double clicking on an object " +
             "triggers selection of all visible selectable objects.")]
        public bool DoubleClickSelectionEnabled;

        /// <summary>
        /// Specifies if the clicks/touches over UI are ignored.
        /// This will prevent selector from responding when
        /// button is clicked UI layer.
        /// </summary>
        [Tooltip("Specifies if the clicks/touches over UI components are ignored.")]
        public bool IgnoreWhenOverUI;

        /// <summary>
        /// Specifies if units should be sorted by distance on selection.
        /// This has small impact on performance so it makes sense
        /// to enable this only when active selection limitation is enabled.
        /// </summary>
        [SerializeField]
        [Tooltip("Specifies if units should be sorted by distance on selection.")]
        public bool SortSelectionByDistance;

        /// <summary>
        /// Specifies raycast behaviour for hover and click features.
        /// </summary>
        [Tooltip("Specifies raycast behaviour for hover and click features.")]
        [SerializeField]
        internal SelectionRaycastType RaycastType;

        /// <summary>
        /// Specifies maximal ray hits supported for click and hover.
        /// When <see cref="RaycastType"/> is set to <see cref="SelectionRaycastType.SingleHit"/>,
        /// this value is ignored.
        /// </summary>
        [Tooltip("Specifies maximal ray hits supported for click and hover. " +
            "When RaycastType is set to SingleHit, this value is ignored.")]
        [SerializeField, Range(2, 15)]
        internal int MaxRaycastHits;

        [Header("Limits")]

        /// <summary>
        /// Maximum distance allowed on for click, hover or drag selection.
        /// </summary>
        [SerializeField]
        [Tooltip("Maximum distance allowed on for click, hover or drag selection.")]
        public float MaxSelectionDistance;

        /// <summary>
        /// Specifies threshold for mouse click. If mouse movement is larger
        /// than this value, it will trigger drag selection.
        /// </summary>
        [SerializeField]
        [Tooltip("Specifies threshold for mouse click. If mouse " +
            "movement is larger than this value, it will trigger drag selection.")]
        public float MouseDragThreshold;

        [Header("Highlight")]

        /// <summary>
        /// Enables highlighting a unit on hover.
        /// </summary>
        public bool HighlightOnMouseHover;

    }

}