namespace TRavljen.Tooltip.Input
{
    using UnityEngine;
    
    /// <summary>
    /// Default implementation for determining the current mouse position on the screen.
    /// Used by the <see cref="TooltipManager"/> to update tooltip positions.
    /// Provides compatibility with both the new and legacy Unity input systems.
    /// </summary>
    sealed class DefaultMousePositionProvider: ICursorPositionProvider
    {
        /// <summary>
        /// Gets the current position of the mouse cursor in screen coordinates.
        /// Handles both the legacy and new Unity input systems. 
        /// If neither input system is enabled, logs a warning and returns <see cref="Vector3.zero"/>.
        /// </summary>
        public Vector3 CursorPosition
        {
            get
            {
#if ENABLE_INPUT_SYSTEM
                // New input system backends are enabled.
                return UnityEngine.InputSystem.Mouse.current.position.ReadValue();
#elif ENABLE_LEGACY_INPUT_MANAGER
            // Old input backends are enabled.
            return Input.mousePosition;
#else
            // No input detected at all, unexpected!
            Debug.LogWarning("No input detected! Default cursor position provider will not behave as expected.");
            return Vector3.zero;
#endif
            }
        }
    }

}