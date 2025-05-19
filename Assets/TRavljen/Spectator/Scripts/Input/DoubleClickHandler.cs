using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Timers;

namespace Spectator
{

    /// <summary>
    /// Double click handler internally checks time interval between
    /// clicks and <see cref="HandleClick"/> returns TRUE once there
    /// is a second click within the time frame.
    /// </summary>
    class DoubleClickHandler
    {

        private Timer timer = new Timer();

        /// <summary>
        /// Initialises double click handler with clickInterval as the
        /// max time allowed between two clicks.
        /// </summary>
        /// <param name="clickInterval">Max interval between the clicks,
        /// default value is 500</param>
        public DoubleClickHandler(float clickInterval = 500)
        {
            timer.Interval = clickInterval;
            timer.Elapsed += (obj, e) => DoubleClickTimerElapsed();
        }

        /// <summary>
        /// Handles clicks internally, if this is the second click
        /// within the time interval, it will return TRUE.
        /// </summary>
        /// <returns>Returns true once double click is triggered</returns>
        public bool HandleClick()
        {
            // If timer is disabled, this is the first click
            if (timer.Enabled == false)
            {
                timer.Start();
                return false;
            }
            // Otherwise double click has been triggered
            else
            {
                timer.Stop();
                return true;
            }
        }

        private void DoubleClickTimerElapsed()
        {
            // No second mouse click followed the first one
            timer.Stop();
        }
    }

}