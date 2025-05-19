using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TRavljen.PlacementSystem
{
    /// <summary>
    /// Internal implementation for default mouse position behaviour.
    /// <see cref="ActionInputControl"/> implements the <see cref="ICursorPositionProvider"/>
    /// to add touch screen support.
    /// </summary>
    sealed class DefaultMousePositionProvider : ICursorPositionProvider
    {
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