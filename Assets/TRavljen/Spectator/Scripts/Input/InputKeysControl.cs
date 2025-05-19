using System;
using UnityEngine;

namespace Spectator
{

    /// <summary>
    /// Input control component for old input system that uses <see cref="KeyCode"/>.
    /// This component defines actions that can be used for the spectator
    /// functionality.
    /// </summary>
    public class InputKeysControl : MonoBehaviour, IInputControl
    {
        #region Options

        /// <summary>
        /// Specifies the key configurations that will be used to control
        /// the spectator camera functionality.
        /// </summary>
        public InputKeyConfiguration Configuration = new InputKeyConfiguration() {
            // General
            ToggleKey = KeyCode.T,
            ResetCameraKey = KeyCode.R,
            ToggleCollisionKey = KeyCode.H,
            BoostKey = KeyCode.LeftShift,

            // Rotation
            RotateCameraKey = KeyCode.Mouse1,
            RotationAxisX = "Mouse X",
            RotationAxisY = "Mouse Y",

            // Camera drag
            DragCameraKey = KeyCode.Mouse2,
            DragAxisX = "Mouse X",
            DragAxisY = "Mouse Y",

            // Selection
            SelectKey = KeyCode.Mouse0,
            CancelSelectionKey = KeyCode.Escape,
            CenterSelectedObjectKey = KeyCode.F,
            ToggleFollowSelectedObjectKey = KeyCode.G,

            // Basic Movement
            UpKey = KeyCode.E,
            DownKey = KeyCode.Q,
            ForwardKey = KeyCode.W,
            BackwardKey = KeyCode.S,
            LeftKey = KeyCode.A,
            RightKey = KeyCode.D
        };

        #endregion

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

        public Vector2 MousePosition => Input.mousePosition;

        public Vector3 MovementInput { get; private set; } = new Vector3();

        public Vector2 DragMovementInput { get; private set; } = new Vector2();

        public float ZoomDelta => Input.mouseScrollDelta.y;

        public bool IsRotateActionPressed =>
            Input.GetKey(Configuration.RotateCameraKey);

        public bool IsDragActionPressed =>
            Input.GetKey(Configuration.DragCameraKey);

        public bool IsBoostActionPressed =>
            Input.GetKey(Configuration.BoostKey);

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

        private void Update()
        {
            InvokePressedActions();

            CameraRotationDelta = new Vector2(
                Input.GetAxis(Configuration.RotationAxisX),
                Input.GetAxis(Configuration.RotationAxisY));

            if (IsDragActionPressed)
            {
                DragMovementInput = new Vector2(
                    Input.GetAxis(Configuration.DragAxisX),
                    Input.GetAxis(Configuration.DragAxisY));
            } else
            {
                DragMovementInput = Vector3.zero;
            }

            UpdateMovementInput();
        }

        private void InvokePressedActions()
        {
            if (Input.GetKeyDown(Configuration.ToggleKey))
            {
                OnTogglePressed?.Invoke();
            }

            if (Input.GetKeyDown(Configuration.ResetCameraKey))
            {
                OnCameraResetPressed?.Invoke();
            }

            if (Input.GetKeyDown(Configuration.SelectKey))
            {
                OnSelectActionPressed?.Invoke();
            }

            if (Input.GetKeyDown(Configuration.CenterSelectedObjectKey))
            {
                OnCenterSelectedObjectPressed?.Invoke();
            }

            if (Input.GetKeyDown(Configuration.ToggleFollowSelectedObjectKey))
            {
                OnFollowSelectedObjectPressed?.Invoke();
            }

            if (Input.GetKeyDown(Configuration.ToggleCollisionKey))
            {
                OnCollisionTogglePressed?.Invoke();
            }

            if (Input.GetKeyDown(Configuration.CancelSelectionKey))
            {
                OnCancelSelectionPressed?.Invoke();
            }
        }

        private void UpdateMovementInput()
        {
            var newMoveInput = new Vector3();

            // Handle up/down actions
            if (Input.GetKey(Configuration.UpKey))
            {
                newMoveInput.y = 1;
            }
            else if (Input.GetKey(Configuration.DownKey))
            {
                newMoveInput.y = -1;
            }

            // Handle forward/backward actions
            if (Input.GetKey(Configuration.ForwardKey))
            {
                newMoveInput.z = 1;
            }
            else if (Input.GetKey(Configuration.BackwardKey))
            {
                newMoveInput.z = -1;
            }

            // Handle left and right actions
            if (Input.GetKey(Configuration.RightKey))
            {
                newMoveInput.x = 1;
            }
            else if (Input.GetKey(Configuration.LeftKey))
            {
                newMoveInput.x = -1;
            }

            MovementInput = newMoveInput;
        }

        #endregion

    }

    #region Input Configuration

    [Serializable]
    public struct InputKeyConfiguration
    {

        /// <summary>
        /// Specifies the toggle key to enable or disable the spectator.
        /// </summary>
        [Header("General")]
        public KeyCode ToggleKey;

        /// <summary>
        /// Specifies the key for reseting camera positon and rotation.
        /// Every time spectator is enabled (toggled) the position will be
        /// stored and when this is pressed spectator will be returned
        /// to the stored position.
        /// </summary>
        public KeyCode ResetCameraKey;

        /// <summary>
        /// Specifies the key that rotates camera based on mouse movement
        /// around its center.
        /// </summary>
        public KeyCode RotateCameraKey;

        /// <summary>
        /// Specifies the string/name of the Input read for rotating camera
        /// on X axis.
        /// </summary>
        public string RotationAxisX;

        /// <summary>
        /// Specifies the string/name of the Input read for rotating camera
        /// on Y axis.
        /// </summary>
        public string RotationAxisY;

        /// <summary>
        /// Specifies the key that enables dragging camera with
        /// <see cref="DragAxisX"/> & <see cref="DragAxisY"/>.
        /// </summary>
        public KeyCode DragCameraKey;

        /// <summary>
        /// Specifies the Input axis used for dragging camera on X axis.
        /// </summary>
        public string DragAxisX;

        /// <summary>
        /// Specifies the Input axis used for dragging camera on Y axis.
        /// </summary>
        public string DragAxisY;

        /// <summary>
        /// Specifies the key to enable or disable camera collision detection.
        /// </summary>
        public KeyCode ToggleCollisionKey;

        /// <summary>
        /// Specifies the key for activating boost speed.
        /// </summary>
        public KeyCode BoostKey;

        /// <summary>
        /// Specifies the key for selecting an object that is hovered by the mouse.
        /// </summary>
        [Header("Selection")]
        public KeyCode SelectKey;

        /// <summary>
        /// Specifies the key that cancels current selection (deselects object)
        /// </summary>
        public KeyCode CancelSelectionKey;

        /// <summary>
        /// Specifies the key that centers the selected object on screen.
        /// This will make camera move towards the object and look at it.
        /// </summary>
        public KeyCode CenterSelectedObjectKey;

        /// <summary>
        /// Specifies the key that toggles camera follow option on selected
        /// game object.
        /// </summary>
        public KeyCode ToggleFollowSelectedObjectKey;

        /// <summary>
        /// Specifies the key for moving camera on Y axis up (increasing).
        /// </summary>
        [Header("Basic Movement")]
        public KeyCode UpKey;

        /// <summary>
        /// Specifies the key for moving camera on Y axis down (decreasing).
        /// </summary>
        public KeyCode DownKey;

        /// <summary>
        /// Specifies the key for moving camera foward.
        /// </summary>
        public KeyCode ForwardKey;

        /// <summary>
        /// Specifies the key for moving camera backward.
        /// </summary>
        public KeyCode BackwardKey;

        /// <summary>
        /// Specifies the key for moving camera right.
        /// </summary>
        public KeyCode RightKey;

        /// <summary>
        /// Specifies the key for moving camera left.
        /// </summary>
        public KeyCode LeftKey;

    }

    #endregion

}