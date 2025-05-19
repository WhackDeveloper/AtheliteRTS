using System;
using UnityEngine;

namespace Spectator
{

    [RequireComponent(typeof(Rigidbody))]
    public class SpectatorPlayer : MonoBehaviour
    {

        #region Public Members

        /// <summary>
        /// Specifies if this component functionality is enabled or not. If this
        /// is disabled, all events and actions are ignored except for the
        /// <see cref="IInputControl.OnTogglePressed"/>.
        /// </summary>
        public bool SpectatorEnabled = false;

        /// <summary>
        /// Specifies if following an object is enabled.
        /// </summary>
        public bool FollowEnabled = true;

        /// <summary>
        /// Specifies that will be controlled by this component based on user
        /// input/actions.
        /// By default main camera is selected on Start if this is not set.
        /// </summary>
        public Camera TargetCamera;

        /// <summary>
        /// Specifies spectators general options.
        /// </summary>
        [Header("General")]
        public GeneralOptions Options = new GeneralOptions()
        {
            Speed = 10f,
            CenterDistance = 10f,
            RequiredRotationTrigger = false,
            RotationSpeed = 200f,
            ForwardMoveIsRelative = true,
            TopAngleLimit = -90,
            BottomAngleLimit = 90,
            LockRotationX = false,
            LockRotationY = false,
            CameraSmooth = 8f,
            ZoomSensitivity = 5f,
            InvertZoom = false,
            CollisionEnabled = true,
            RestrictPosition = false,
            LockCursor = false,
            AllowedAreaBounds = new Bounds(
                Vector3.zero, new Vector3(15, 10, 15))
        };

        /// <summary>
        /// Specifies camera movement when cursor is at the edge of the screen.
        /// </summary>
        public ScreenEdgeMovement ScreenEdgeMovement = new()
        {
            enabled = false, 
            threshold = 0.05f,
            delay = 0.3f
        };

        /// <summary>
        /// Specifies selection options.
        /// </summary>
        [Header("Selection")]
        public SelectionOptions SelectionOptions = new()
        {
            Enabled = true,
            DoubleClickEnabled = true,
            MaxSelectDistance = 100f
        };

        /// <summary>
        /// Specifies camera boost options.
        /// </summary>
        [Header("Boost")]
        public CameraBoostOptions BoostOptions = new(true, 20);

        /// <summary>
        /// Returns current follow target. If none is being followed the
        /// collider is null.
        /// </summary>
        public Collider SelectedObject { get; private set; }

        /// <summary>
        /// Returns current selected object. If none is selected the
        /// collider is null.
        /// </summary>
        public Collider FollowTarget
        {
            get => followTarget;
            private set
            {
                followTarget = value;
            }
        }
        [SerializeField]
        private Collider followTarget;

        #endregion

        #region Events

        /// <summary>
        /// Action called when object is selected with mouse click.
        /// </summary>
        public Action<GameObject> OnObjectSelected;

        public Action<Collider> OnFollowStart;
        public Action<Collider> OnFollowEnd;

        public Action<bool> OnEnableToggle;

        #endregion

        #region Private Members

        /// <summary>
        /// Specifies the initial state of the camera saved when camera movement
        /// was enabled and player may return to the initial position by
        /// triggering action <see cref="IInputControl.OnCameraResetPressed"/>.
        /// </summary>
        private CameraState _initialState;

        /// <summary>
        /// Specifies the collider (optional). In case collider component is
        /// present and <see cref="EnableCollision"/> is set to false, then this
        /// collider component will be disabled by internal behaviour.
        /// </summary>
        private Collider _collider;

        /// <summary>
        /// Specifies the rigidbody attached to the game object used for camera
        /// movement and collision by using built in physics.
        /// </summary>
        private Rigidbody _rigidbody; 

        /// <summary>
        /// Specifies the input control, required for one of the components
        /// to implement it in order to use functionality in this class
        /// (movement, rotation).
        /// </summary>
        private IInputControl _inputControl;

        /// <summary>
        /// Helper class for selecting objects in scene.
        /// </summary>
        private ObjectSelector _objectSelector;

        /// <summary>
        /// Current calculating position that is usually applied to the
        /// game objects at the end of update or fixed update and sometimes
        /// through the actions directly.
        /// </summary>
        private Vector3 _nextPosition;

        /// <summary>
        /// Flag that indicates if the actions callbacks were already bound to
        /// the functions.
        /// </summary>
        private bool _userInteractionConnected;

        /// <summary>
        /// Initial cursor.
        /// </summary>
        private CursorLockMode _cursorInitialState;

        /// <summary>
        /// Current rotation updated by input.
        /// </summary>
        private Vector2 _rotation;
        
        /// <summary>
        /// Defines the duration of active edge movement. This is used to apply edge movement
        /// after certain delay defined with configuration field.
        /// </summary>
        private float edgeMovementTime = 0;

        #endregion

        #region Lifecycle

        private void Awake()
        {
            _inputControl = GetComponent<IInputControl>();
            _collider = GetComponent<Collider>();
            _rigidbody = GetComponent<Rigidbody>();
            _objectSelector = new ObjectSelector(this, _inputControl);

            if (_objectSelector != null)
            {
                _objectSelector.OnObjectSelect = collider =>
                {
                    SelectedObject = collider;
                    OnObjectSelected?.Invoke(collider.gameObject);
                };

                _objectSelector.OnObjectDoubleClick = collider =>
                {
                    FocusCameraOnPoint(
                        SelectedObject.transform.position,
                        Options.CenterDistance);
                };
            }
        }

        private void Start()
        {
            _cursorInitialState = Cursor.lockState;

            if (TargetCamera == null)
            {
                TargetCamera = Camera.main;
            }

            if (SpectatorEnabled)
            {
                EnableSpectator();
            }
        }

        private void OnEnable()
        {
            _inputControl.OnTogglePressed += ToggleSpectator;

            if (SpectatorEnabled)
            {
                BindActions();
            }
        }

        private void OnDisable()
        {
            _inputControl.OnTogglePressed -= ToggleSpectator;

            if (SpectatorEnabled)
            {
                UnBindActions();
            }
        }

        private void FixedUpdate()
        {
            if (SpectatorEnabled)
            {
                // If spectator is following ignore input here
                if (!FollowTarget)
                {
                    UpdateSpectatorMovement();
                }
            }
        }

        #endregion

        #region Updates

        private void UpdateSpectatorMovement()
        {
            _nextPosition = _rigidbody.position;

            // Check if rotation is enabled without extra trigger
            // action, if it is not check if the trigger action is pressed.
            if (!Options.RequiredRotationTrigger ||
                _inputControl.IsRotateActionPressed)
            {
                RotateCamera();
            }

            MoveCamera();
            
            // Restrict position if needed
            if (Options.RestrictPosition)
            {
                ApplyPositionRestriction();
            }

            UpdateRigidBody();
            UpdateCamera();
        }

        private void UpdateRigidBody()
        {
            // Set velocity based on newly calculated position
            _rigidbody.linearVelocity = (_nextPosition - _rigidbody.position).normalized * GetMoveSpeed();
        }

        private void UpdateCamera()
        {
            var camera = TargetCamera.transform;
            var newPosition = Vector3.Lerp(
                camera.position,
                _rigidbody.position,
                Options.CameraSmooth * Time.fixedDeltaTime);

            var newRotation = Quaternion.Lerp(
                    camera.rotation,
                    Quaternion.Euler(_rotation.x, _rotation.y, 0),
                    Options.CameraSmooth * Time.fixedDeltaTime);

            camera.SetPositionAndRotation(newPosition, newRotation);
        }

        #endregion

        #region Action Callbacks

        private void BindActions()
        {
            if (_userInteractionConnected) return;

            _userInteractionConnected = true;
            _inputControl.OnCameraResetPressed += OnCameraReset;
            _inputControl.OnCenterSelectedObjectPressed += OnCenterSelectedObjectPressed;
            _inputControl.OnFollowSelectedObjectPressed += OnFollowSelectedObjectPressed;
            _inputControl.OnCollisionTogglePressed += OnCollisionTogglePressed;
            _inputControl.OnCancelSelectionPressed += OnCancelSelectionPressed;

            _objectSelector.BindActions();
        }

        private void UnBindActions()
        {
            if (!_userInteractionConnected) return;

            _userInteractionConnected = false;
            _inputControl.OnCameraResetPressed -= OnCameraReset;
            _inputControl.OnCenterSelectedObjectPressed -= OnCenterSelectedObjectPressed;
            _inputControl.OnFollowSelectedObjectPressed -= OnFollowSelectedObjectPressed;
            _inputControl.OnCollisionTogglePressed -= OnCollisionTogglePressed;
            _inputControl.OnCancelSelectionPressed -= OnCancelSelectionPressed;

            _objectSelector.UnbindActions();
        }

        private void OnCenterSelectedObjectPressed()
        {
            // If selected object is different from currently followed
            // one and center is pressed, stop following current object
            // since this was user initiated action (desired trigger).
            if (FollowTarget != SelectedObject)
            {
                StopFollowing();
            }

            if (SelectedObject && SelectedObject != FollowTarget)
            {
                FocusCameraOnPoint(
                    SelectedObject.transform.position,
                    Options.CenterDistance);
            }
        }

        private void OnFollowSelectedObjectPressed()
        {
            // Check if feature is enabled.
            if (!FollowEnabled) return;

            // If spectator is following
            if (FollowTarget)
            {
                // Do the check before its cleared when follow is stopped
                var isDifferentThanSelected = SelectedObject && SelectedObject != FollowTarget;

                StopFollowing();

                // And start following selected object only if it is
                // different than the one that was followed
                if (SelectedObject && isDifferentThanSelected)
                {
                    StartFollowing(SelectedObject);
                }
            }
            // If spectator is not following but has selected object
            else if (SelectedObject)
            {
                StartFollowing(SelectedObject);
            }
        }

        private void OnCancelSelectionPressed()
        {
            SelectedObject = null;
            StopFollowing();
        }

        private void OnCollisionTogglePressed()
        {
            Options.CollisionEnabled = !Options.CollisionEnabled;
            UpdateCollider();
        }

        private void OnCameraReset()
        {
            _nextPosition = _initialState.Position;
            _rotation = _initialState.Rotation.eulerAngles;
            _rigidbody.position = _nextPosition;

            UpdateCamera();
        }

        private void ToggleSpectator()
        {
            SpectatorEnabled = !SpectatorEnabled;

            if (SpectatorEnabled)
            {
                EnableSpectator();
            }
            else
            {
                DisableSpectatorLogic();
            }

            OnEnableToggle?.Invoke(SpectatorEnabled);
        }

        private void EnableSpectator()
        {
            _nextPosition = TargetCamera.transform.position;

            _initialState.Position = _nextPosition;
            _initialState.Rotation = TargetCamera.transform.localRotation;

            _rotation = _initialState.Rotation.eulerAngles;

            UpdateCollider();
            _rigidbody.position = _nextPosition;

            BindActions();

            // Lock cursor if the locking cursor is set to TRUE
            // and if the mouse rotation is done without triggers
            if (Options.LockCursor)
            {
                Cursor.lockState = CursorLockMode.Locked;
            }
        }

        private void DisableSpectatorLogic()
        {
            _initialState = new CameraState();

            // Disable collider if exists, when spectator is disabled
            // it should not be checking for collisions.
            if (_collider != null) _collider.enabled = false;

            UnBindActions();

            StopFollowing();

            if (Options.LockCursor)
            {
                Cursor.lockState = _cursorInitialState;
            }
        }

        #endregion

        #region Select & Follow

        /// <summary>
        /// If selection is enabled, this functions sets the
        /// passed object to be the <see cref="SelectedObject"/>.
        /// </summary>
        /// <param name="collider">Game object with collider to select</param>
        public void SetSelection(Collider collider)
        {
            if (SelectionOptions.Enabled)
            {
                SelectedObject = collider;
                OnObjectSelected?.Invoke(collider.gameObject);
            }
        }

        /// <summary>
        /// Cancels current selection by clearing <see cref="SelectedObject"/>.
        /// </summary>
        public void CancelSelection()
        {
            SelectedObject = null;
        }

        /// <summary>
        /// If follow is enabled, this functions sets the
        /// passed object to be the <see cref="FollowTarget"/>.
        /// </summary>
        /// <param name="collider">Game object with collider to follow</param>
        public void SetFollowTarget(Collider collider)
        {
            if (FollowEnabled)
            {
                StartFollowing(collider);
            }
        }

        #endregion

        #region Camera Handling

        /// <summary>
        /// Updates camera rotation data. Actual rotation happens in the camera
        /// update.
        /// </summary>
        private void RotateCamera()
        {
            var rotationX = _inputControl.CameraRotationDelta.x;
            var rotationY = _inputControl.CameraRotationDelta.y;
            var rotationSpeed = Time.fixedDeltaTime * Options.RotationSpeed;

            // These two are intentionally "swapped"
            float moveX = -rotationY * rotationSpeed;
            float moveY = rotationX * rotationSpeed;

            if (!Options.LockRotationX)
            {
                float bottomLimit = Mathf.Min(Options.TopAngleLimit, Options.BottomAngleLimit);
                float topLimit = Mathf.Max(Options.TopAngleLimit, Options.BottomAngleLimit);

                float newAngleX = Mathf.Clamp(_rotation.x + moveX, bottomLimit, topLimit);
                _rotation.x = newAngleX;
            }

            if (!Options.LockRotationY)
            {
                float newAngleY = _rotation.y + moveY;
                _rotation.y = newAngleY;
            }
        }

        /// <summary>
        /// Moves the camera position based on input and in case there was
        /// any movement while following was enabled, following an object will
        /// then be disabled and movement applied.
        /// </summary>
        private void MoveCamera()
        {
            var positionDelta = GetCameraPositionChange(Time.fixedDeltaTime);
            var deltaY = positionDelta.y;
            positionDelta.y = 0;

            Vector3 move;
            if (Options.ForwardMoveIsRelative)
                move = TargetCamera.transform.TransformDirection(positionDelta);
            else
                move = Quaternion.Euler(0, TargetCamera.transform.eulerAngles.y, 0) * positionDelta;

            _nextPosition += move;
            
            // Apply Y axis changes separately in world position, not relative
            // to the camera rotation.
            _nextPosition.y += deltaY;
        }

        /// <summary>
        /// Clamps the position within the <see cref="AllowedAreaBounds"/>.
        /// </summary>
        private void ApplyPositionRestriction()
        {
            var clampedPosition = _nextPosition;
            var areaBounds = Options.AllowedAreaBounds;
            clampedPosition.x = Mathf.Clamp(clampedPosition.x, areaBounds.min.x, areaBounds.max.x);
            clampedPosition.y = Mathf.Clamp(clampedPosition.y, areaBounds.min.y, areaBounds.max.y);
            clampedPosition.z = Mathf.Clamp(clampedPosition.z, areaBounds.min.z, areaBounds.max.z);
            _nextPosition = clampedPosition;
        }

        public void StopFollowing()
        {
            if (!FollowTarget) return;

            _rotation = TargetCamera.transform.rotation.eulerAngles;

            var collider = FollowTarget;
            FollowTarget = null;
            UpdateCollider();

            // Update spectator object when follow stops
            _rigidbody.position = TargetCamera.transform.position;

            OnFollowEnd?.Invoke(collider);
        }

        private void StartFollowing(Collider collider)
        {
            FollowTarget = collider;
            UpdateCollider();

            OnFollowStart?.Invoke(collider);
        }

        private Vector3 GetCameraPositionChange(float delta)
        {
            Vector3 moveVector = new Vector3()
            {
                // If drag action is pressed, move left/right and up/down
                // based on mouse movement
                x = -_inputControl.DragMovementInput.x,
                z = -_inputControl.DragMovementInput.y
            };

            // Perform updates based on Keyboard input
            var speed = GetMoveSpeed();
            var multiplier = speed * delta;
            // Get default movement input
            var inputMove = _inputControl.MovementInput;

            if (moveVector.magnitude <= 0)
            {
                inputMove += GetScreenEdgeMovementDirection();
            }

            moveVector += inputMove * multiplier;

            if (Options.SwapZAndYForMovement)
            {
                (moveVector.z, moveVector.y) = (moveVector.y, moveVector.z);
            }

            return moveVector;
        }

        private Vector3 GetScreenEdgeMovementDirection()
        {
            if (!ScreenEdgeMovement.enabled) return Vector3.zero;
            
            var threshold = Screen.height * ScreenEdgeMovement.threshold;
            var mousePos = _inputControl.MousePosition;
            var additionalMovement = Vector3.zero;

            if (mousePos.x < threshold && mousePos.x >= 0)
                additionalMovement.x = -1;
            else if (mousePos.x > Screen.width - threshold &&
                mousePos.x <= Screen.width)
                additionalMovement.x = 1;

            if (mousePos.y < threshold && mousePos.y >= 0)
                additionalMovement.z = -1;
            else if (mousePos.y > Screen.height - threshold &&
                mousePos.y <= Screen.height)
                additionalMovement.z = 1;

            if (additionalMovement == Vector3.zero)
                edgeMovementTime = 0;
            else 
                edgeMovementTime += Time.deltaTime;
            
            // Return movement only after delay
            return edgeMovementTime > ScreenEdgeMovement.delay ? additionalMovement.normalized : Vector3.zero;
        }

        public void FocusCameraOnPoint(Vector3 point, float desiredDistance)
        {
            var distance = Vector3.Distance(_nextPosition, point);

            // If camera is closer than the required distance, move
            // camera backwards by mirroring the point.
            if (distance < desiredDistance)
            {
                var change = _nextPosition - point;
                point = _nextPosition + change * -1;
            }

            // Move towards object but stay 10 points away
            var newPosition = Vector3.MoveTowards(
                transform.position,
                point,
                distance - desiredDistance);

            // Checks if camera will have clear sight to the point
            // if not, move it to the collision point so that it has
            // clear view of the focused point
            if (_collider &&
                _collider.enabled &&
                Physics.Linecast(
                    point,
                    newPosition,
                    out RaycastHit hit,
                    Options.CollisionLayerMask))
            {
                newPosition = hit.point;
            }

            _rigidbody.position = newPosition;
            
            UpdateCamera();

            if (SelectedObject)
            {
                TargetCamera.transform.LookAt(SelectedObject.transform);
                _rotation = TargetCamera.transform.eulerAngles;
            }
        }

        #endregion

        #region Convenience

        /// <summary>
        /// Returns appropriate speed for normal or boost mode.
        /// If boost is enabled and boost key is held down then
        /// boost speed will be returned. Otherwise regular speed is returned.
        /// </summary>
        /// <returns>Returns appropriate speed, either boost or regular.</returns>
        private float GetMoveSpeed()
        {
            if (BoostOptions.EnableBoost && _inputControl.IsBoostActionPressed)
            {
                return BoostOptions.BoostSpeed;
            }

            return Options.Speed;
        }

        /// <summary>
        /// Updates collider by enabling or disabling it based
        /// <see cref="GeneralOptions.CollisionEnabled"/>.
        /// </summary>
        private void UpdateCollider()
        {
            var expectedState = Options.CollisionEnabled && FollowTarget == null;
            if (_collider != null && _collider.enabled != expectedState)
            {
                _collider.enabled = expectedState;
            }
        }

        #endregion

        #region CameraState

        /// <summary>
        /// Camera state, defines all properties that are manipulated by the
        /// <see cref="SpectatorPlayer"/>. This can be used to save camera state
        /// at any given moment.
        /// </summary>
        struct CameraState
        {

            public Vector3 Position;
            public Quaternion Rotation;

        }

        #endregion

    }

}
