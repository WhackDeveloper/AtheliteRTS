using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TRavljen.UnitFormation
{
    using UnityEngine.Events;

    /// <summary>
    /// Input communication interface used for unit placement.
    /// </summary>
    public interface IInputControl
    {

        /// <summary>
        /// Invoke this on placment action press (start).
        /// </summary>
        public UnityEvent OnPlacementActionPress { get; set; }

        /// <summary>
        /// Invoke this on placement action release (end).
        /// </summary>
        public UnityEvent OnPlacementActionRelease { get; set; }

        /// <summary>
        /// Invoke this when placement action cancel is pressed.
        /// </summary>
        public UnityEvent OnPlacementActionCancel { get; set; }

        /// <summary>
        /// Return current mouse position.
        /// </summary>
        public Vector3 MousePosition { get; }

    }

}