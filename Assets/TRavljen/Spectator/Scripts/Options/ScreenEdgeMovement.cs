using System;
using UnityEngine;

namespace Spectator
{
    /// <summary>
    /// Defines options for moving the camera with moving cursor at the edge of the screen.
    /// </summary>
    [Serializable]
    public struct ScreenEdgeMovement
    {
        public bool enabled;
     
        [Tooltip("Specifies threshold in screen height percentage. Ideal value is between 0.05 and 0.2")]
        [Range(0.001f, 0.5f)]
        public float threshold;

        [Tooltip("Specifies the delay between reaching edge of the screen for movement and actual movement.")]
        public float delay;
    }
}