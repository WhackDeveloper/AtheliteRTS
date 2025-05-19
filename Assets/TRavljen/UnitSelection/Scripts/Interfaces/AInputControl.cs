using System;
using UnityEngine;

namespace TRavljen.UnitSelection
{

    /// <summary>
    /// Abstract <see cref="MonoBehaviour"/> for input control by <see cref="UnitSelector"/>.
    /// With this approach unit selector can support both new and old
    /// input system.
    /// </summary>
    public abstract class AInputControl : MonoBehaviour, IInputControl
    {

        /// <summary>
        /// Invoke this when action for canceling selection was triggered.
        /// </summary>
        public Action OnCancelTriggered { get; set; }

        /// <summary>
        /// Invoke this once mouse down action is true (mouse click).
        /// </summary>
        public Action OnMouseDown { get; set; }

        /// <summary>
        /// Invoke this once mouse up action is true (mouse released).
        /// </summary>
        public Action OnMouseUp { get; set; }

        /// <summary>
        /// Implement this to return the current mouse position.
        /// </summary>
        public abstract Vector3 MousePosition { get; }

        /// <summary>
        /// Invoke this when a quick selection action has been pressed for certain
        /// index.
        /// </summary>
        public Action<int> OnQuickSelectionToggle { get; set; }

        /// <summary>
        /// Implement this to handle controls for modifying current selection.
        /// Feature to add/remove units from current selection.
        /// </summary>
        public abstract bool IsModifyCurrentSelectionPressed { get; }

        /// <summary>
        /// Implement this to handle controls for saving the current selection.
        /// Along with this control, player must also select which action will
        /// be used to access the saved selection.
        /// </summary>
        public abstract bool IsQuickSavePressed { get; }

    }

}