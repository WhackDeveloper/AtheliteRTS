using UnityEngine;
using UnityEngine.Events;

namespace TRavljen.PlacementSystem
{

#if UNITY_EDITOR
    using UnityEditor;
    using EditorUtility;
    [CustomEditor(typeof(ObjectPlacementObserver))]
    public class ObjectPlacementObserverEditor : HiddenScriptPropertyEditor { }
#endif

    /// <summary>
    /// MonoBehaviour component used for observing all the events on
    /// <see cref="ObjectChangeEvents"/> and expose them in Unity Editor.
    ///
    /// This is a convenience script that can be used in Editor instead of
    /// adding listeners through the code.
    /// </summary>
    /// <remarks>These events are only observed when this script is active & enabled.</remarks>
    public class ObjectPlacementObserver : MonoBehaviour
    {

        [Tooltip("Event invoked when placement has begun, receiving the object instance created for placement.")]
        public UnityEvent<GameObject> OnPlacementStart = new();

        [Tooltip("Event invoked when placement of an object is finished validating.")]
        public UnityEvent<bool> OnPlacementValidate = new();

        [Tooltip("Event invoked when placement has ended successfully, receiving the new game object instance and the result of placement.\n" +
            "In case 'ObjectPlacement.TryEndPlacement(out PlacementResult)' is invoked, then the new object received will be null.")]
        public UnityEvent<GameObject, PlacementResult> OnPlacementEnd = new();

        [Tooltip("Event invoked when placement has been attempted, placement validation prevented the placement.")]
        public UnityEvent OnPlacementFail = new();

        [Tooltip("Event invoked when an active placement has been canceled.")]
        public UnityEvent OnPlacementCancel = new();

        [Tooltip("Event invoked when an object will or should be destroyed. Depending on placement configuration.")]
        public UnityEvent<GameObject> OnObjectDestroy = new();
        
        private ObjectPlacementEvents events = ObjectPlacementEvents.Instance;

        private void OnEnable()
        {
            events.OnPlacementStart.AddListener(OnPlacementStart.Invoke);
            events.OnPlacementValidate.AddListener(OnPlacementValidate.Invoke);
            events.OnPlacementEnd.AddListener(OnPlacementEnd.Invoke);
            events.OnPlacementFail.AddListener(OnPlacementFail.Invoke);
            events.OnPlacementCancel.AddListener(OnPlacementCancel.Invoke);
            events.OnObjectDestroy.AddListener(OnObjectDestroy.Invoke);
        }

        private void OnDisable()
        {
            events.OnPlacementStart.RemoveListener(OnPlacementStart.Invoke);
            events.OnPlacementValidate.RemoveListener(OnPlacementValidate.Invoke);
            events.OnPlacementEnd.RemoveListener(OnPlacementEnd.Invoke);
            events.OnPlacementFail.RemoveListener(OnPlacementFail.Invoke);
            events.OnPlacementCancel.RemoveListener(OnPlacementCancel.Invoke);
            events.OnObjectDestroy.RemoveListener(OnObjectDestroy.Invoke);
        }
    }
}