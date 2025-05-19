using UnityEngine;
using UnityEngine.Events;

namespace TRavljen.PlacementSystem
{

    /// <summary>
    /// Class holding references to public events invoked when placement system
    /// is active, respectively.
    /// </summary>
    sealed public class ObjectPlacementEvents
    {
        public static readonly ObjectPlacementEvents Instance = new();

        /// <summary>
        /// Event invoked when placement has started, receiving the object instance created for placement.
        /// </summary>
        public readonly UnityEvent<GameObject> OnPlacementStart = new();

        /// <summary>
        /// Event invoked when placement of an object is finished validating.
        /// </summary>
        public readonly UnityEvent<bool> OnPlacementValidate = new();

        /// <summary>
        /// Event invoked when placement has ended successfully,
        /// receiving the new game object instance and the result of placement.
        /// 
        /// In case <see cref="ObjectPlacement.TryEndPlacement(out PlacementResult)"/> is invoked,
        /// then the new object received will be null.
        /// </summary>
        public readonly UnityEvent<GameObject, PlacementResult> OnPlacementEnd = new();

        /// <summary>
        /// Event invoked when placement has been attempted, placement validation prevented the placement.
        /// </summary>
        public readonly UnityEvent OnPlacementFail = new();

        /// <summary>
        /// Event invoked when an active placement has been canceled.
        /// </summary>
        public readonly UnityEvent OnPlacementCancel = new();
        
        /// <summary>
        /// Event invoked when an object will or should be destroyed.
        /// Depending on placement configuration.
        /// </summary>
        public readonly UnityEvent<GameObject> OnObjectDestroy = new();

    }
}