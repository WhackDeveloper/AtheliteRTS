namespace TRavljen.PlacementSystem
{
    using UnityEngine;
    using UnityEngine.Events;

    /// <summary>
    /// Abstract class for defining player's interface with placement system.
    /// It is intendede that this component listens to player input for specific
    /// input packages, while <see cref="InputControlResponder"/> handles
    /// interactions with the system.
    /// </summary>
    [System.Serializable]
    public abstract class AInputControl: MonoBehaviour
    {

        /// <summary>
        /// Invoke this when action for canceling selection was triggered.
        /// </summary>
        public UnityEvent OnPlacementCancel { get; set; } = new();

        /// <summary>
        /// Invoke this once mouse down action is true (mouse click).
        /// </summary>
        public UnityEvent OnPlacementActiveToggle { get; set; } = new();

        /// <summary>
        /// Invoke this once mouse up action is true (mouse released).
        /// </summary>
        public UnityEvent OnPlacementFinish { get; set; } = new();

        /// <summary>
        /// Invoke this with direction and magnitude of the rotation,
        /// positive is clockwise and negative is counter clockwise.
        /// </summary>
        public UnityEvent<float> OnRotate { get; set; } = new();

        /// <summary>
        /// Invoke this when placement prefab should be changed to the next one.
        /// </summary>
        public UnityEvent OnNextPrefab { get; set; } = new();

        /// <summary>
        /// Invoke this when placement prefab should be changed to the previous one.
        /// </summary>
        public UnityEvent OnPreviousPrefab { get; set; } = new();

        /// <summary>
        /// Invoke this when placement prefab should be changed for one on specific index.
        /// </summary>
        public UnityEvent<int> OnPrefabAction { get; set; } = new();

    }

}
