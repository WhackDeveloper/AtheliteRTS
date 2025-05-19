using UnityEngine;
using System;

namespace Spectator
{

    /// <summary>
    /// Extra spectator component that can be attached with the
    /// <see cref="SpectatorPlayer"/> in order to achieve
    /// smooth following with camera collision. If this is not
    /// added to the object follow feature will do nothing.
    /// </summary>
    [RequireComponent(typeof(SpectatorPlayer))]
    public class FollowCamera : MonoBehaviour
    {
        #region Public Members

        /// <summary>
        /// Specifies the distance that the object will be placed at the start
        /// of following. May be altered by zooming in/out.
        /// </summary>
        [Header("General")]
        public float InitialDistance = 8f;

        /// <summary>
        /// Specifies minimal distance that can be zoomed while following
        /// a selected target.
        /// </summary>
        public float MinDistance = 4f;

        /// <summary>
        /// Specifies maximal distance that can be zoomed while following
        /// a selected target.
        /// </summary>
        public float MaxDistance = 15f;

        /// <summary>
        /// Specifies the offset from the target on which the follow will be
        /// focused on. To keep it at the position of the target set this
        /// to <see cref="Vector3.zero"/>.
        /// </summary>
        [Header("Target")]
        public Vector3 TargetOffset = new Vector3(0f, 1f, 0f);

        /// <summary>
        /// Specifies if the camera should constantly look at the target position.
        /// Keep this on FALSE if you wish to use this for top down follow,
        /// anything above 70 degree angle should have this disabled.
        /// </summary>
        public bool AlwaysLookAtTarget = false;

        /// <summary>
        /// Specifies if the X angle can be changed with the input.
        /// </summary>
        [Header("X Axis")]
        public bool LockRotationX = false;

        /// <summary>
        /// Specifies if the X rotation axes is inverted.
        /// </summary>
        public bool InvertRotationX = false;

        /// <summary>
        /// Specifies the minimal angle for X rotation.
        /// </summary>
        [Range(-90, 90)]
        public float MinXAngle = 0f;

        /// <summary>
        /// Specifies the minimal angle for Y rotation.
        /// </summary>
        [Range(-90, 90)]
        public float MaxXAngle = 90f;

        /// <summary>
        /// Specifies if rotation of the camera will be set
        /// to <see cref="StartRotationX"/> when spectator
        /// starts to follow a target.
        /// </summary>
        public bool UseStartRotationX = false;

        /// <summary>
        /// Specifies the starting rotation of the camera when
        /// <see cref="UseStartRotationX"/> is set to true.
        /// </summary>
        public float StartRotationX = 60f;

        /// <summary>
        /// Specifies if the Y angle can be changed with the input.
        /// </summary>
        [Header("Y Axis")]
        public bool LockRotationY = false;

        /// <summary>
        /// Specifies if the Y rotation axes is inverted.
        /// </summary>
        public bool InvertRotationY = false;

        /// <summary>
        /// Specifies if rotation of the camera will be set
        /// to <see cref="StartRotationY"/> when spectator
        /// starts to follow a target.
        /// </summary>
        public bool UseStartRotationY = false;

        /// <summary>
        /// Specifies the starting rotation of the camera when
        /// <see cref="UseStartRotationY"/> is set to true.
        /// </summary>
        public float StartRotationY = 0f;

        #endregion

        #region Private Members

        private Transform TargetCamera =>
            _player.TargetCamera ? _player.TargetCamera.transform : null;

        private Transform Target =>
            _player.FollowTarget ? _player.FollowTarget.transform : null;

        /// <summary>
        /// Calculated camera position for each frame.
        /// </summary>
        private Vector3 _cameraPos;

        /// <summary>
        /// Reference to the spectator component that holds most of
        /// the configurations spectator.
        /// </summary>
        private SpectatorPlayer _player;

        /// <summary>
        /// Clamped rotation input.
        /// </summary>
        private Vector2 _rotation = new Vector2();

        private IInputControl _inputControl;
        private float _currentFollowZoom = 0f;

        #endregion

        #region Lifecycle

        private void Awake()
        {
            _player = GetComponent<SpectatorPlayer>();
            _inputControl = _player.GetComponent<IInputControl>();

            if (_player.SpectatorEnabled && TargetCamera && Target)
            {
                StartFollowing();
            }

            _player.OnFollowStart += _ => StartFollowing();
            _player.OnEnableToggle += enabled =>
            {
                if (enabled && Target)
                    StartFollowing();
            };
        }

        private void FixedUpdate()
        {
            if (_player.SpectatorEnabled && _player.FollowTarget)
            {
                if (_inputControl.ZoomDelta != 0)
                {
                    ZoomCamera();
                }
            }
        }

        private void Update()
        {
            if (_player.SpectatorEnabled &&
                (!_player.Options.RequiredRotationTrigger||
                _inputControl.IsRotateActionPressed))
            {
                var RotationSpeed = _player.Options.RotationSpeed * Time.deltaTime;

                if (!LockRotationY)
                {
                    var desiredY = _rotation.y + _inputControl.CameraRotationDelta.x * RotationSpeed * (InvertRotationY ? -1 : 1);
                    _rotation.y = (desiredY < 0 ? desiredY + 360f : desiredY) % 360;
                }

                if (!LockRotationX)
                {
                    _rotation.x = Mathf.Clamp(
                        _rotation.x - _inputControl.CameraRotationDelta.y * RotationSpeed * (InvertRotationX ? -1 : 1),
                        MinXAngle,
                        MaxXAngle);
                }
            }
        }

        void LateUpdate()
        {
            if (_player.SpectatorEnabled && _player.FollowTarget)
            {
                UpdateCameraPosition();
            }
        }

        #endregion

        #region Camera Updates

        private void StartFollowing()
        {
            // Reset zoom value
            _currentFollowZoom = InitialDistance;

            // Set custom X rotation if enabled
            if (UseStartRotationX)
            {
                _rotation.x = StartRotationX;
            }
            else
            {
                _rotation.x = TargetCamera.eulerAngles.x;

                if (_rotation.x > 180)
                {
                    _rotation.x -= 360f;
                }
            }

            // Set custom Y rotation if enabled
            if (UseStartRotationY)
            {
                _rotation.y = Target.eulerAngles.y - StartRotationY;
            }
            else
            {
                _rotation.y = TargetCamera.eulerAngles.y;
            }
        }

        private void UpdateCameraPosition()
        {
            var offsetTarget = Target.position + TargetOffset;
            var smooth = Mathf.Clamp(Time.deltaTime * _player.Options.CameraSmooth, 0f, 1f);

            // Check if collision is enabled.
            if (_player.Options.CollisionEnabled)
            {
                // Detect collisions from actual target position so that its fully
                // visible when followed close to the wall in cases where the
                // position is on the ground and camera pushed above the wall
                HandleCollision(offsetTarget);
            }

            // Move camera to new position
            CameraSmoothMove(smooth);

            Quaternion yRotation = Quaternion.AngleAxis(_rotation.y, Vector3.up);
            Quaternion xRotation = Quaternion.AngleAxis(_rotation.x, Vector3.right);
            Quaternion newRotation = yRotation * xRotation;

            if (AlwaysLookAtTarget)
            {
                TargetCamera.LookAt(offsetTarget);
            }
            else
            {
                TargetCamera.rotation = Quaternion.Lerp(
                    TargetCamera.rotation, newRotation, smooth);
            }

            _cameraPos = offsetTarget + newRotation * Vector3.forward * -1 * _currentFollowZoom;
        }

        private void CameraSmoothMove(float smooth)
        {
            TargetCamera.position = Vector3.Lerp(
                TargetCamera.position, _cameraPos, smooth);
        }

        /// <summary>
        /// Makes linecast from target object to the camera to see if there are
        /// any colliders in the way (camera needs clear line of sight).
        /// </summary>
        /// <param name="targetFollow">Target position to which the linecast
        /// will be done</param>
        private void HandleCollision(Vector3 targetFollow)
        {
            if (Physics.Linecast(
                targetFollow,
                _cameraPos,
                out RaycastHit hit,
                _player.Options.CollisionLayerMask))
            {
                // Distance to push from wall
                var pushDistance = 0.5f;
                // Push camera on X and Z axes
                _cameraPos = hit.point + hit.normal * pushDistance;
            }
        }

        #endregion

        #region Zoom

        private void ZoomCamera()
        {
            var zoomDelta = _inputControl.ZoomDelta * _player.Options.ZoomSensitivity;
            if (_player.Options.InvertZoom)
            {
                zoomDelta *= -1;
            }

            _currentFollowZoom = Mathf.Clamp(
                _currentFollowZoom + zoomDelta, MinDistance, MaxDistance);
        }

        #endregion

    }

}
