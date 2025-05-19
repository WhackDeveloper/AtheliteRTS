
namespace TRavljen.PlacementSystem
{
    using UnityEngine;
#if UNITY_EDITOR
    using UnityEditor;
    using EditorUtility;
    
    [CustomEditor(typeof(KeyInputControl))]
    public class KeyInputControlEditor : HiddenScriptPropertyEditor { }
#endif

    /// <summary>
    /// Behaviour component for defining players input by using Unity's OLD
    /// <see cref="Input"/>.
    /// </summary>
    [System.Serializable]
    public class KeyInputControl : AInputControl
    {

        [Header("General")]
        [Tooltip("Specifies the key for starting placement process with " +
            "currently selected game object prefab.")]
        public KeyCode toggleActivePlacementKey = KeyCode.B;

        [Tooltip("Specifies the key used for canceling active placement.")]
        public KeyCode cancelPlacementKey = KeyCode.Escape;

        [Tooltip("Specifies the key for finishing placement process " +
            "(placing object in world).")]
        public KeyCode finishPlacementKey = KeyCode.Mouse0;

        [Header("Rotation")]
        [Tooltip("Specifies if rotation should be applied on key hold or key press.")]
        public bool holdToRotate = false;

        [Tooltip("Specifies the clockwise rotation input key.")]
        public KeyCode rotateClockwiseKey = KeyCode.E;

        [Tooltip("Specifies the counter clockwise rotation input key.")]
        public KeyCode rotateCounterClockwiseKey = KeyCode.Q;

        public KeyCode nextPrefabKey = KeyCode.Plus;
        public KeyCode previousPrefabKey = KeyCode.Minus;

        public KeyCode[] prefabKeys = new KeyCode[10] {
            KeyCode.Alpha1,
            KeyCode.Alpha2,
            KeyCode.Alpha3,
            KeyCode.Alpha4,
            KeyCode.Alpha5,
            KeyCode.Alpha6,
            KeyCode.Alpha7,
            KeyCode.Alpha8,
            KeyCode.Alpha9,
            KeyCode.Alpha0
        };

        private void Start()
        {
            // Bridge for input & placement.
            gameObject.AddComponent<InputControlResponder>();
        }

        private void Update()
        {
            if (Input.GetKeyDown(toggleActivePlacementKey))
            {
                OnPlacementActiveToggle.Invoke();
            }
            else if (Input.GetKeyDown(cancelPlacementKey))
            {
                OnPlacementCancel.Invoke();
            }
            else if (Input.GetKeyDown(finishPlacementKey))
            {
                OnPlacementFinish.Invoke();
            }

            if (holdToRotate)
            {
                if (Input.GetKey(rotateClockwiseKey))
                {
                    OnRotate.Invoke(Time.deltaTime);
                }
                else if (Input.GetKey(rotateCounterClockwiseKey))
                {
                    OnRotate.Invoke(-1 * Time.deltaTime);
                }
            }
            else
            {
                if (Input.GetKeyDown(rotateClockwiseKey))
                {
                    OnRotate.Invoke(1);
                }
                else if (Input.GetKeyDown(rotateCounterClockwiseKey))
                {
                    OnRotate.Invoke(-1);
                }
            }

            if (Input.GetKeyDown(nextPrefabKey))
            {
                OnNextPrefab.Invoke();
            }
            if (Input.GetKeyDown(previousPrefabKey))
            {
                OnPreviousPrefab.Invoke();
            }

            for (int index = 0; index < prefabKeys.Length; index++)
            {
                KeyCode key = prefabKeys[index];
                if (Input.GetKeyDown(key))
                {
                    OnPrefabAction.Invoke(index);
                }
            }
        }

    }

}