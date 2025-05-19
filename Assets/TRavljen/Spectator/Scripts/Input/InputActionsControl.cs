using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

namespace Spectator
{

#if ENABLE_INPUT_SYSTEM
    using UnityEngine.InputSystem;

    /// <summary>
    /// Input control component for new input system that uses <see cref="InputAction"/>.
    /// This component defines actions that can be used for the spectator
    /// functionality.
    /// </summary>
    public class InputActionsControl : MonoBehaviour, IInputControl
    {

        public InputActionConfiguration Configuration = new InputActionConfiguration();

        #region Public Members (IInputControl)

        public Action OnTogglePressed
        {
            get => _onTogglePressed;
            set => _onTogglePressed = value;
        }

        public Action OnCameraResetPressed
        {
            get => _onCameraResetPressed;
            set => _onCameraResetPressed = value;
        }

        public Action OnSelectActionPressed
        {
            get => _onSelectActionPressed;
            set => _onSelectActionPressed = value;
        }

        public Action OnCenterSelectedObjectPressed
        {
            get => _onCenterSelectedObjectPressed;
            set => _onCenterSelectedObjectPressed = value;
        }

        public Action OnFollowSelectedObjectPressed
        {
            get => _onFollowSelectedObjectPressed;
            set => _onFollowSelectedObjectPressed = value;
        }

        public Action OnCollisionTogglePressed
        {
            get => _onCollisionTogglePressed;
            set => _onCollisionTogglePressed = value;
        }

        public Action OnCancelSelectionPressed
        {
            get => _onCancelSelectionPressed;
            set => _onCancelSelectionPressed = value;
        }

        public Vector2 CameraRotationDelta { get; private set; } = new Vector2();

        public Vector2 MousePosition { get; private set; } = new Vector2();

        public Vector3 MovementInput { get; private set; } = new Vector3();

        public Vector2 DragMovementInput { get; private set; } = new Vector2();

        public float ZoomDelta => Configuration.ZoomDeltaAction.ReadValue<float>();

        public bool IsRotateActionPressed =>
            IsActionPressed(Configuration.RotateCameraAction);

        public bool IsDragActionPressed =>
            IsActionPressed(Configuration.DragTriggerMoveAction);

        public bool IsBoostActionPressed =>
            IsActionPressed(Configuration.BoostAction);

        #endregion

        #region Private Members

        private Action _onTogglePressed;
        private Action _onCameraResetPressed;
        private Action _onSelectActionPressed;
        private Action _onCenterSelectedObjectPressed;
        private Action _onFollowSelectedObjectPressed;
        private Action _onCollisionTogglePressed;
        private Action _onCancelSelectionPressed;

        #endregion

        #region Lifecycle

        private void Start()
        {
            Configuration.ToggleAction.started += _ => OnTogglePressed?.Invoke();
            Configuration.ResetCameraAction.started += _ => OnCameraResetPressed?.Invoke();
            Configuration.SelectAction.started += _ => OnSelectActionPressed?.Invoke();
            Configuration.CenterSelectedObjectAction.started += _ => OnCenterSelectedObjectPressed?.Invoke();
            Configuration.ToggleFollowSelectedObjectAction.started += _ => OnFollowSelectedObjectPressed?.Invoke();
            Configuration.ToggleCollisionAction.started += _ => OnCollisionTogglePressed?.Invoke();
            Configuration.CancelSelectionAction.started += _ => OnCancelSelectionPressed?.Invoke();
        }

        private void OnEnable()
        {
            Configuration.ToggleAction.Enable();
            Configuration.ResetCameraAction.Enable();
            Configuration.RotateCameraAction.Enable();
            Configuration.DragMoveAction.Enable();
            Configuration.DragTriggerMoveAction.Enable();
            Configuration.ToggleCollisionAction.Enable();
            Configuration.BoostAction.Enable();
            Configuration.ZoomDeltaAction.Enable();
            
            // Mouse
            Configuration.MousePositionAction.Enable();
            Configuration.RotateAction.Enable();

            // Selection
            Configuration.SelectAction.Enable();
            Configuration.CancelSelectionAction.Enable();
            Configuration.CenterSelectedObjectAction.Enable();
            Configuration.ToggleFollowSelectedObjectAction.Enable();

            // Movement
            Configuration.ForwardMoveAction.Enable();
            Configuration.MoveAction.Enable();
        }

        private void OnDisable()
        {
            Configuration.ToggleAction.Disable();
            Configuration.ResetCameraAction.Disable();
            Configuration.RotateCameraAction.Disable();
            Configuration.DragMoveAction.Disable();
            Configuration.DragTriggerMoveAction.Disable();
            Configuration.ToggleCollisionAction.Disable();
            Configuration.BoostAction.Disable();
            Configuration.ZoomDeltaAction.Disable();

            // Mouse
            Configuration.MousePositionAction.Disable();
            Configuration.RotateAction.Disable();

            // Selection
            Configuration.SelectAction.Disable();
            Configuration.CancelSelectionAction.Disable();
            Configuration.CenterSelectedObjectAction.Disable();
            Configuration.ToggleFollowSelectedObjectAction.Disable();

            // Movement
            Configuration.ForwardMoveAction.Disable();
            Configuration.MoveAction.Disable();
        }

        private void Update()
        {
            Vector3 movementInput = Configuration.MoveAction.ReadValue<Vector2>();
            movementInput.z = Configuration.ForwardMoveAction.ReadValue<float>();
            MovementInput = movementInput;

            MousePosition = Configuration.MousePositionAction.ReadValue<Vector2>();

            CameraRotationDelta = Configuration.RotateAction.ReadValue<Vector2>();

            if (IsDragActionPressed)
            {
                DragMovementInput = Configuration.DragMoveAction.ReadValue<Vector2>();
            }
            else
            {
                DragMovementInput = Vector2.zero;
            }
        }

        #endregion

        #region Convenience

        /// <summary>
        /// Checks if passed input is pressed by reading float value and
        /// then return true if the value is larger than 0.
        /// </summary>
        private static bool IsActionPressed(InputAction input) => input.ReadValue<float>() > 0f;

        #endregion

        #region Input Configuration

        [Serializable]
        public struct InputActionConfiguration
        {

            /// <summary>
            /// Specifies the toggle action to enable or disable the spectator.
            /// </summary>
            [Header("General")]
            public InputAction ToggleAction;

            /// <summary>
            /// Specifies the action for reseting camera positon and rotation.
            /// Every time spectator is enabled (toggled) the position will be
            /// stored and when this is pressed spectator will be returned
            /// to the stored position.
            /// </summary>
            public InputAction ResetCameraAction;

            /// <summary>
            /// Specifies the action that rotates camera based on
            /// <see cref="CameraRotationDelta"/> values.
            /// </summary>
            public InputAction RotateCameraAction;

            /// <summary>
            /// Specifies the action for zooming the camera.
            /// </summary>
            public InputAction ZoomDeltaAction;

            /// <summary>
            /// Specifies the action button that enables dragging camera
            /// with <see cref="DragMoveAction"/>
            /// </summary>
            public InputAction DragTriggerMoveAction;

            /// <summary>
            /// Specifies the action for dragging camera when
            /// <see cref="DragTriggerMoveAction"/> is pressed.
            /// </summary>
            public InputAction DragMoveAction;

            /// <summary>
            /// Specifies the action to enable or disable camera collision detection.
            /// </summary>
            public InputAction ToggleCollisionAction;

            /// <summary>
            /// Specifies the action for activating boost speed.
            /// </summary>
            public InputAction BoostAction;

            /// <summary>
            /// Specifies the action that returns the mouse position (cursor).
            /// Primarily used for object selection.
            /// </summary>
            [Header("Mouse")]
            public InputAction MousePositionAction;

            /// <summary>
            /// Specifies the action that returns input values for
            /// rotation changes.
            /// </summary>
            public InputAction RotateAction;

            /// <summary>
            /// Specifies the action for selecting an object that is hovered by
            /// the mouse.
            /// </summary>
            [Header("Selection")]
            public InputAction SelectAction;

            /// <summary>
            /// Specifies the action that cancels current selection (deselects object).
            /// </summary>
            public InputAction CancelSelectionAction;

            /// <summary>
            /// Specifies the key that centers the selected object on screen.
            /// This will make camera move towards the object and look at it.
            /// </summary>
            public InputAction CenterSelectedObjectAction;

            /// <summary>
            /// Specifies the action that toggles camera follow option on selected
            /// game object.
            /// </summary>
            public InputAction ToggleFollowSelectedObjectAction;

            /// <summary>
            /// Specifies the action for moving camera on X and Y axes (left/right, up/down).
            /// </summary>
            [Header("Basic Movement")]
            public InputAction MoveAction;

            /// <summary>
            /// Specifies the action for moving camera on Z axis (forward/backward).
            /// </summary>
            public InputAction ForwardMoveAction;

        }

        #endregion

    }

#endif
}