using System;

namespace Spectator
{

    /// <summary>
    /// Adjustable Options for Camera Boost.
    /// </summary>

    [Serializable]
    public struct CameraBoostOptions
    {
        /// <summary>
        /// Enables boost options for the camera controls.
        /// </summary>
        public bool EnableBoost;

        /// <summary>
        /// Speed applied when boost key is pressed.
        /// </summary>
        public float BoostSpeed;

        public CameraBoostOptions(bool enableBoost, float boostSpeed)
        {
            EnableBoost = enableBoost;
            BoostSpeed = boostSpeed;
        }

    }

}