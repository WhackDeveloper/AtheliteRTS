using UnityEngine;
using System.Collections.Generic;

namespace TRavljen.UnitSelection
{

    /// <summary>
    /// Convenience data structure for sorting.
    /// </summary>
    internal struct SortUnit
    {
        public ISelectable Unit;
        public int Distance;

        public SortUnit(ISelectable unit, int distance)
        {
            Unit = unit;
            Distance = distance;
        }
    }

    /// <summary>
    /// Abstract class for area of selection behaviour.
    /// </summary>
    public abstract class ASelectionArea : MonoBehaviour
    {

        #region Properties

        /// <summary>
        /// Specifies the manager responsible for providing
        /// units for selection.
        /// This reference is managed by the <see cref="UnitSelector"/>.
        /// </summary>
        public IUnitManager UnitManager { get; internal set; }

        /// <summary>
        /// Specifies the layer mask that will be used to filter units when
        /// detection is using colliders.
        /// This reference is managed by the <see cref="UnitSelector"/>.
        /// </summary>
        [HideInInspector]
        public LayerMask SelectableLayerMask { get; internal set; }

        /// <summary>
        /// Specifies the camera that will be used for any computation for players
        /// view perspective. Set internally by the system, exposed for use by
        /// custom selection areas.
        /// This reference is managed by the <see cref="UnitSelector"/>.
        /// </summary>
        public Camera Camera { get; internal set; }

        /// <summary>
        /// Specifies maximal selection distance defined by the selection
        /// system configuration on start.
        /// </summary>
        public float MaxSelectionDistance { get; internal set; }

        #endregion

        #region Abstract

        /// <summary>
        /// This function is called once the mouse dragging starts, it should
        /// return FALSE if for some reason dragging should not be initiated.
        /// This means that <see cref="MouseDragContinues(Vector2)"/> and
        /// <see cref="MouseDragStops"/> will not be called for this drag action.
        /// </summary>
        /// <param name="startPosition">Starting position of the drag</param>
        /// <returns>Should return TRUE if mouse drag is eligable to start
        /// or FALSE if not.</returns>
        public abstract bool ShouldMouseDragStartSelection(Vector2 startPosition);

        /// <summary>
        /// This function is called once the mouse start dragging, one frame
        /// after the <see cref="ShouldMouseDragStartSelection"/> function call.
        /// </summary>
        /// <param name="newPosition">New position of the mouse</param>
        public abstract void MouseDragContinues(Vector2 newPosition);

        /// <summary>
        /// This function is called on the last frame of the drag when mouse is
        /// released and selection has been processed by using
        /// <see cref="GetCurrentObjectsWithinArea"/> function.
        /// </summary>
        public abstract void MouseDragStops();

        /// <summary>
        /// This function should return list of all viable selectable objects
        /// within the selection area of the mouse drag.
        /// </summary>
        /// <param name="sortByDistance">If units should be sorted by distance
        /// from start position</param>
        /// <returns>Returs list of selectable units.</returns>
        public abstract List<ISelectable> GetCurrentObjectsWithinArea(bool sortByDistance);

        #endregion

    }

}