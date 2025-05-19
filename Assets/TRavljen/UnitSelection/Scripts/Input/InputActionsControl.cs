using UnityEngine;

#if ENABLE_INPUT_SYSTEM

namespace TRavljen.UnitSelection
{
    using UnityEngine.InputSystem;

    /// <summary>
    /// Component for handling selection input. It supports the new InputSystem.
    /// If you are using the old Input, you should use this component instead
    /// <see cref="InputKeysControl"/>.
    /// </summary>
    public class InputActionsControl : AInputControl
    {

        #region Properties

        [SerializeField]
        private Actions actions;

        #endregion

        #region AInputControl

        /// <summary>
        /// Returns the current mouse position.
        /// </summary>
        public override Vector3 MousePosition => Mouse.current.position.ReadValue();

        /// <summary>
        /// Returns true if the action for modifying current selection is pressed.
        /// </summary>
        public override bool IsModifyCurrentSelectionPressed
            => IsActionPressed(actions.ModifyCurrentSelectionAction);

        /// <summary>
        /// Returns true if the quick SAVE action is pressed.
        /// </summary>
        public override bool IsQuickSavePressed
            => IsActionPressed(actions.QuickSaveAction);

        [HideInInspector, SerializeField]
        private bool isSetup = false;

        #endregion

        #region Lifecycle

        private void Start()
        {
            SetupIfNeeded();
            SetUpActionListeners();
        }

        private void OnEnable() => actions.Enable();

        private void OnDisable() => actions.Disable();

        private void OnValidate() => SetupIfNeeded();

        private void SetupIfNeeded()
        {
            if (!isSetup)
            {
                isSetup = true;
                SetDefaultActions();
            }
        }

        /// <summary>
        /// Restores actions to default values.
        /// </summary>
        public void SetDefaultActions()
            => actions.SetDefaultActions();

        #endregion

        #region Action Listeners

        private void SetUpActionListeners()
        {
            actions.CancelAction.started += HandleCancel;
            actions.SelectionAction.performed += HandleSelection;

            for (int index = 0; index < actions.QuickAccessActions.Length; index++)
            {
                var action = actions.QuickAccessActions[index];
                int currentIndex = index;
                action.started += context =>
                {
                    OnQuickSelectionToggle?.Invoke(currentIndex);
                };
            }
        }

        private void HandleCancel(InputAction.CallbackContext context)
            => OnCancelTriggered?.Invoke();

        private void HandleSelection(InputAction.CallbackContext context)
        {
            bool isPressed = IsActionPressed(context.action);

            if (isPressed)
                OnMouseDown?.Invoke();
            else
                OnMouseUp?.Invoke();
        }

        #endregion

        /// <summary>
        /// Checks if passed input is pressed by reading float value and
        /// then return true if the value is larger than 0.
        /// </summary>
        private static bool IsActionPressed(InputAction input)
            => input.ReadValue<float>() > 0f;

        [System.Serializable]
        public struct Actions
        {

            [SerializeField]
            public InputAction CancelAction;

            [Space(4)]
            [SerializeField]
            public InputAction ModifyCurrentSelectionAction;

            [Space(4)]
            [SerializeField]
            public InputAction SelectionAction;

            [Space]
            [Header("Quick Selection")]
            [SerializeField]
            public InputAction QuickSaveAction;

            [Space(4)]
            [SerializeField]
            public InputAction[] QuickAccessActions;

            public void SetDefaultActions()
            {
                CancelAction = new InputAction("Cancel", InputActionType.Button, "<Keyboard>/escape");

                ModifyCurrentSelectionAction = new InputAction("Modify Current Selection", InputActionType.Button, "<Keyboard>/shift");
                QuickSaveAction = new InputAction("Quick save", InputActionType.Button, "<Keyboard>/ctrl");
                SelectionAction = new InputAction("Select", InputActionType.PassThrough, "<Mouse>/leftButton");

                QuickAccessActions = new InputAction[10] {
                    new InputAction("Action", InputActionType.Button, "<Keyboard>/1"),
                    new InputAction("Action", InputActionType.Button, "<Keyboard>/2"),
                    new InputAction("Action", InputActionType.Button, "<Keyboard>/3"),
                    new InputAction("Action", InputActionType.Button, "<Keyboard>/4"),
                    new InputAction("Action", InputActionType.Button, "<Keyboard>/5"),
                    new InputAction("Action", InputActionType.Button, "<Keyboard>/6"),
                    new InputAction("Action", InputActionType.Button, "<Keyboard>/7"),
                    new InputAction("Action", InputActionType.Button, "<Keyboard>/8"),
                    new InputAction("Action", InputActionType.Button, "<Keyboard>/9"),
                    new InputAction("Action", InputActionType.Button, "<Keyboard>/0")
                };
            }

            public readonly void Enable()
            {
                CancelAction.Enable();
                ModifyCurrentSelectionAction.Enable();
                QuickSaveAction.Enable();
                SelectionAction.Enable();

                foreach (var action in QuickAccessActions)
                    action.Enable();
            }

            public readonly void Disable()
            {
                CancelAction.Disable();
                ModifyCurrentSelectionAction.Disable();
                QuickSaveAction.Disable();
                SelectionAction.Disable();

                foreach (var action in QuickAccessActions)
                    action.Disable();
            }

        }

    }

}
#endif