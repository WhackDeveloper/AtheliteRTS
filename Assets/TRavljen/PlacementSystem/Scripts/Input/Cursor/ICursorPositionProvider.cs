using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TRavljen.PlacementSystem
{
    /// <summary>
    /// Interface for implementing a custom cursor position handling.
    /// It can be used for mouse, touch screen and even controller sticks.
    /// </summary>
    public interface ICursorPositionProvider
    {
        /// <summary>
        /// Current cursor position, most commonly this would be current mouse position.
        /// </summary>
        public Vector3 CursorPosition { get; }
    }
}