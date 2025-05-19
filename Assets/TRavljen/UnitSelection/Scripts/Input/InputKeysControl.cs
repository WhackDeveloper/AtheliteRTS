using UnityEngine;
using System.Collections.Generic;
using System;

namespace TRavljen.UnitSelection
{

    /// <summary>
    /// Component for handling selection input. It supports built-in Input type.
    /// If you are using the new InputSystem, you should use this component
    /// instead <see cref="InputActionsControl"/>.
    /// </summary>
    public class InputKeysControl : AInputControl
    {

        #region Properties

        /// <summary>
        /// Returns the current mouse position.
        /// </summary>
        public override Vector3 MousePosition => Input.mousePosition;

        /// <summary>
        /// Returns true if the key for modifying current selection is pressed.
        /// </summary>
        public override bool IsModifyCurrentSelectionPressed =>
            Input.GetKey(modifyCurrentSelectionKey);

        /// <summary>
        /// Returns true if the quick SAVE action is pressed.
        /// </summary>
        public override bool IsQuickSavePressed =>
            Input.GetKey(quickSaveKey);

        /// <summary>
        /// Key used for multi selection by clicking. Generally clicks replace
        /// current selection, but in combination with this key they will added
        /// or removed from selection
        /// </summary>
        [SerializeField]
        [Tooltip("Key used for modifying current selection by clicking or dragging. " +
            "Generally clicks replace current selection, but in combination with " +
            "this key they will be added or removed from selection.")]
        private KeyCode modifyCurrentSelectionKey = KeyCode.LeftShift;

        /// <summary>
        /// Key used for cancelling selection.
        /// </summary>
        [SerializeField]
        [Tooltip("Key used for cancelling selection")]
        private KeyCode cancelSelectionKey = KeyCode.Escape;

        /// <summary>
        /// Primary selection action. By default this is a mouse click
        /// </summary>
        [SerializeField]
        [Tooltip("Primary selection action. By default this is a mouse click")]
        private KeyCode selectionKey = KeyCode.Mouse0;

        /// <summary>
        /// Specifies the key that needs to be held down for saving current selection
        /// when any of the <see cref="quickSelectionKeys"/> is pressed.
        /// </summary>
        [Tooltip("Specifies the key that needs to be held down for saving current " +
            "selection when any of the 'quickSelectionKeys' is pressed.")]
        [SerializeField]
        private KeyCode quickSaveKey = KeyCode.LeftControl;

        /// <summary>
        /// Specifies the keys available for quick unit access.
        /// </summary>
        [Tooltip("Specifies the keys available for quick unit access.")]
        [SerializeField]
        private List<KeyCode> quickSelectionKeys = new List<KeyCode>()
        {
            KeyCode.Alpha1,
            KeyCode.Alpha2,
            KeyCode.Alpha3,
            KeyCode.Alpha4,
            KeyCode.Alpha5,
            KeyCode.Alpha6,
            KeyCode.Alpha7,
            KeyCode.Alpha8,
            KeyCode.Alpha9,
            KeyCode.Alpha0
        };

        #endregion

        #region Lifecycle

        private void Update()
        {
            if (Input.GetKeyDown(cancelSelectionKey))
            {
                OnCancelTriggered?.Invoke();
            }
            else if (Input.GetKeyDown(selectionKey))
            {
                OnMouseDown?.Invoke();
            }
            else if (Input.GetKeyUp(selectionKey))
            {
                OnMouseUp?.Invoke();
            }
            else
            {
                for (int index = 0; index < quickSelectionKeys.Count; index++)
                {
                    if (Input.GetKeyDown(quickSelectionKeys[index]))
                    {
                        OnQuickSelectionToggle.Invoke(index);
                        // Allow only first key for selection (if 2 keys are down)
                        // so after one is valid, skip the rest.
                        break;
                    }
                }
            }
        }

        #endregion

        /// <summary>
        /// Resets keys to default values.
        /// </summary>
        public void SetDefaultKeys()
        {
            modifyCurrentSelectionKey = KeyCode.LeftShift;
            cancelSelectionKey = KeyCode.Escape;
            selectionKey = KeyCode.Mouse0;

            quickSaveKey = KeyCode.LeftControl;
            quickSelectionKeys = new List<KeyCode>()
                {
                    KeyCode.Alpha1,
                    KeyCode.Alpha2,
                    KeyCode.Alpha3,
                    KeyCode.Alpha4,
                    KeyCode.Alpha5,
                    KeyCode.Alpha6,
                    KeyCode.Alpha7,
                    KeyCode.Alpha8,
                    KeyCode.Alpha9,
                    KeyCode.Alpha0
                };
        }
    }
}