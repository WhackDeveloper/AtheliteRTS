using System;
using UnityEngine;

namespace Spectator
{

    [Serializable]
    public struct GeneralOptions
    {
        /// <summary>
        /// Specifies the default camera movement speed without the boost.
        /// </summary>
        [Header("Movement")]
        public float Speed;

        /// <summary>
        /// Specifies the camera movement smooth.
        /// </summary>
        [Range(0.1f, 15f)]
        public float CameraSmooth;


        /// <summary>
        /// Specifies if trigger action needs to be pressed in order to
        /// rotate around the camera. If this is set to false, rotation
        /// for every frame when <see cref="IInputControl.CameraRotationDelta"/>
        /// is not zero.
        /// </summary>
        [Header("Rotation")]
        public bool RequiredRotationTrigger;

        /// <summary>
        /// Specifies the speed of camera rotation.
        /// </summary>
        public float RotationSpeed;

        /// <summary>
        /// Specifies if the movement forward should use all rotation angles.
        /// All angles are used when this is true. If this is false, then only
        /// Y angle is used for calculating the move forward.
        /// </summary>
        public bool ForwardMoveIsRelative;
        
        /// <summary>
        /// If enabled, movement is swapped between Z and Y axis. This is only useful when Unity is set up
        /// with Z pointing upward.
        /// </summary>
        public bool SwapZAndYForMovement;

        /// <summary> Specifies the rotation limit up. </summary>
        [Range(-180, 180)] public float TopAngleLimit;
        /// <summary> Specifies the rotation limit down. </summary>
        [Range(-180, 180)] public float BottomAngleLimit;

        /// <summary> Specifies if the rotation on X axis is locked. </summary>
        public bool LockRotationX;
        /// <summary> Specifies if the rotation on Y axis is locked. </summary>
        public bool LockRotationY;

        [Header("Zoom")]

        /// <summary> Specifies zoom sensitivity. </summary>
        public float ZoomSensitivity;

        /// <summary> Specifies if zooming is inverted. </summary>
        public bool InvertZoom;

        [Header("Collision")]

        /// <summary>
        /// Specifies if the camera collision is enabled, thus preventing
        /// camera from going through objects with colliders.
        /// </summary>
        public bool CollisionEnabled;

        /// <summary>
        /// Specifies the layer mask for collisions used to prevent moving
        /// the object when the path detects a collision. It is good to make
        /// sure that the player spectator itself does not use a layer that is
        /// selected here.
        /// </summary>
        public LayerMask CollisionLayerMask;

        /// <summary>
        /// Specifies if the position for camera is restricted. If this is
        /// set to true <see cref="AllowedAreaBounds"/> will be used to clamp
        /// cameras position.
        /// </summary>
        [Header("Permissions")]
        public bool RestrictPosition;

        /// <summary>
        /// Specifies the allowed position of the camera. If
        /// <see cref="RestrictPosition"/> is enabled then this is applied
        /// otherwise it is ignored.
        /// </summary>
        public Bounds AllowedAreaBounds;

        [Header("Misc")]

        /// <summary>
        /// Specifies the distance that the object will be placed at from the
        /// centered object. Object is centered when its double clicked or
        /// center action is pressed when game object is selected.
        /// </summary>
        public float CenterDistance;

        /// <summary>
        /// Specifies if the cursor should be locked when spectator is enabled.
        /// This can simply be set to false if customised behaviour for cursor
        /// is required.
        /// </summary>
        public bool LockCursor;

    }

}