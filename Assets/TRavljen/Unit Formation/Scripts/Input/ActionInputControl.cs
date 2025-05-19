# if ENABLE_INPUT_SYSTEM
namespace TRavljen.UnitFormation
{
    using UnityEngine.InputSystem;
    using UnityEngine;

    /// <summary>
    /// Input control for formation placement using Unity' new Input System.
    /// </summary>
    public class ActionInputControl : AInputControl
    {

        [Tooltip("Specifies the action that starts and completes unit placement.\n\n" +
            "Important: Action must be Pass Through Button as it uses press and release events.")]
        [SerializeField]
        private InputAction placementAction;

        [Tooltip("Specifies the action used for canceling active selection.")]
        [SerializeField]
        private InputAction cancelAction;

        public override Vector3 MousePosition => Mouse.current.position.ReadValue();

        #region Lifecycle

        private void OnEnable()
        {
            SetupDefaultActionsIfNull();

            placementAction.Enable();
            placementAction.performed += HandlePlacementAction;

            cancelAction.Enable();
            cancelAction.performed += HandleCancelAction;
        }

        private void OnDisable()
        {
            placementAction.Disable();
            placementAction.performed -= HandlePlacementAction;

            cancelAction.Disable();
            cancelAction.performed -= HandleCancelAction;
        }

        #endregion

        public void SetupDefaultActionsIfNull()
        {
            if (placementAction == null)
            {
                placementAction = new InputAction(
                    "Right mouse button",
                    InputActionType.PassThrough,
                    "<Mouse>/rightButton");
            }
            if (cancelAction == null)
            {
                cancelAction = new InputAction(
                    "Escape",
                    InputActionType.Button,
                    "<Keyboard>/escape");
            }
        }

        private void HandlePlacementAction(InputAction.CallbackContext context)
        {
            bool isPressed = context.action.ReadValue<float>() > 0f;

            if (isPressed)
            {
                OnPlacementActionPress.Invoke();
            }
            else
            {
                OnPlacementActionRelease.Invoke();
            }
        }

        private void HandleCancelAction(InputAction.CallbackContext context)
        {
            bool isPressed = context.action.ReadValue<float>() > 0f;

            if (isPressed)
                OnPlacementActionCancel.Invoke();
        }
    }

}
#endif