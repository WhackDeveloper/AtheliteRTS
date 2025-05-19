using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace TRavljen.PlacementSystem
{

    /// <summary>
    /// Observer component that implements <see cref="IPlacingObject"/> and supports
    /// referencing events within the Unity Editor. This is for cases where a
    /// simple action needs to be performed based on an event to avoid implementing
    /// a custom <see cref="IPlacingObject"/> <see cref="MonoBehaviour"/>.
    /// </summary>
    public class PlacingObjectObserver : MonoBehaviour, IPlacingObject
    {


        [Tooltip("Invoked when placement on this object has started. " +
            "In such case the object is a template instance created for active placement only.")]
        public UnityEvent OnPlacementStart = new UnityEvent();

        [Tooltip("Invoked when placement on this object is validated.\n" +
            "Value passed is true if placement of the object is valid at this moment.")]
        public UnityEvent<bool> OnPlacementValidated = new UnityEvent<bool>();

        [Tooltip("Invoked when placement of this object is finished. " +
            "This will be invoked on final instance created when placement is complete.")]
        public UnityEvent OnPlacementEnd = new UnityEvent();

        [Tooltip("Invoked when placement of this object is cancelled.")]
        public UnityEvent OnPlacementCancel = new UnityEvent();

        [Tooltip("Invoked when placement of this object has failed. " +
            "This happens when placement is attempted with invalid position and rotation.")]
        public UnityEvent OnPlacementFail = new UnityEvent();
        
        public void PlacementStarted() => OnPlacementStart.Invoke();
        public void PlacementValidated(bool isValid) => OnPlacementValidated.Invoke(isValid);
        public void PlacementEnded() => OnPlacementEnd.Invoke();
        public void PlacementCancelled() => OnPlacementCancel.Invoke();
        public void PlacementFailed() => OnPlacementFail.Invoke();

    }

}