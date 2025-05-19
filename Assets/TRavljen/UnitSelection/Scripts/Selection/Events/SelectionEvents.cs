using System.Collections.Generic;
using UnityEngine.Events;

namespace TRavljen.UnitSelection
{

    /// <summary>
    /// Class holding references to public events invoked when selection system
    /// is active, respectively.
    /// </summary>
    public class SelectionEvents
    {
        public static SelectionEvents Instance = new();

        /// <summary>
        /// Event invoked when list of selections has changed.
        /// </summary>
        public UnityEvent<List<ISelectable>> OnSelectionChange = new();

        /// <summary>
        /// Event invoked when list of highlights has changed.
        /// </summary>
        public UnityEvent<List<ISelectable>> OnHighlightChange = new();

        /// <summary>
        /// Event invoked when hovering selectable has changed.
        /// Either it was set, cleared or updated.
        /// </summary>
        public UnityEvent<ISelectable> OnUnitHoverChange = new();

    }

}