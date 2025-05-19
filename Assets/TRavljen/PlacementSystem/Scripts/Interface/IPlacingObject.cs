using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TRavljen.PlacementSystem
{

    /// <summary>
    /// Interface for notifying a specific game object of its placement status.
    /// Implement this interface by subclassing a <see cref="MonoBehaviour"/> component
    /// and then update your game object with custom behaviour upon these events.
    /// </summary>
    public interface IPlacingObject
    {

        /// <summary>
        /// Invoked when placement on this object has started. In such case the
        /// object is a template instance created for active placement only.
        /// </summary>
        public void PlacementStarted();

        /// <summary>
        /// Invoked when placement on this object is validated.
        /// </summary>
        /// <param name="isValid">If placement of the object is valid at this moment.</param>
        public void PlacementValidated(bool isValid);

        /// <summary>
        /// Invoked when placement of this object is finished. This will be invoked
        /// on final instance created when placement is complete.
        /// </summary>
        public void PlacementEnded();

        /// <summary>
        /// Invoked when placement of this object has failed. This happens when
        /// placement is attempted with invalid position and rotation.
        /// </summary>
        public void PlacementFailed();

        /// <summary>
        /// Invoked when placement of this object is cancelled.
        /// </summary>
        public void PlacementCancelled();
    }

}