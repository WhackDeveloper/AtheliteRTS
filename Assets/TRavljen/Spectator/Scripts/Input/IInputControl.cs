using UnityEngine;
using System;

namespace Spectator
{

    /// <summary>
    /// Defines user input contract for <see cref="SpectatorPlayer"/>.
    /// </summary>
    interface IInputControl
    {

        /// <summary>
        /// Action triggered when toggle for spectator is pressed.
        /// </summary>
        Action OnTogglePressed { get; set; }

        /// <summary>
        /// Action triggered when reset camera position and rotation is pressed.
        /// </summary>
        Action OnCameraResetPressed { get; set; }

        /// <summary>
        /// Action triggered when select action is pressed.
        /// </summary>
        Action OnSelectActionPressed { get; set; }

        /// <summary>
        /// Action triggered when center selected object was pressed.
        /// </summary>
        Action OnCenterSelectedObjectPressed { get; set; }

        /// <summary>
        /// Action triggered when follow selected object was pressed.
        /// </summary>
        Action OnFollowSelectedObjectPressed { get; set; }

        /// <summary>
        /// Action triggered when collision toggle is pressed.
        /// </summary>
        Action OnCollisionTogglePressed { get; set; }

        /// <summary>
        /// Action triggered when cancel selection is pressed.
        /// </summary>
        Action OnCancelSelectionPressed { get; set; }

        /// <summary>
        /// Mouse position used for selection (position of mouse cursor).
        /// </summary>
        Vector2 MousePosition { get; }

        /// <summary>
        /// Camera rotation delta used for rotating the camera around
        /// the followed object or around itself.
        /// </summary>
        Vector2 CameraRotationDelta { get; } 

        /// <summary>
        /// Movement input actions presented as a movement direction.
        /// </summary>
        Vector3 MovementInput { get; }

        /// <summary>
        /// Movement input for the dragging of the camera on X and Y axes.
        /// </summary>
        Vector2 DragMovementInput { get; }

        /// <summary>
        /// Specifies the change in zoom input value from the last frame.
        /// If this is not 0 zooming will be performed.
        /// </summary>
        float ZoomDelta { get; }

        /// <summary>
        /// Returns true if the action for rotation is pressed.
        /// </summary>
        bool IsRotateActionPressed { get; }

        /// <summary>
        /// Returns true if the action for boost is pressed.
        /// </summary>
        bool IsBoostActionPressed { get; }

    }

}