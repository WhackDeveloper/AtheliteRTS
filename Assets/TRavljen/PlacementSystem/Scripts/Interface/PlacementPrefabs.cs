using UnityEngine;

namespace TRavljen.PlacementSystem
{

    /// <summary>
    /// Manages the selection and placement of prefabs within the scene.
    /// This class handles cycling through a list of prefabs and updating the active prefab for placement.
    /// </summary>
    public class PlacementPrefabs : MonoBehaviour
    {

        #region Properties

        [Tooltip("Array of prefabs that can be cycled through for placement.")]
        [SerializeField]
        private GameObject[] prefabs = new GameObject[0];

        /// <summary>
        /// Input control used to listen for user input to change the selected prefab.
        /// </summary>
        private AInputControl inputControl;

        [Tooltip("Specifies whether the prefab selection will cycle (wrap around) when reaching the start or end of the list.")]
        [SerializeField]
        private bool cyclesRotation = true;

        /// <summary>
        /// Index of the currently selected prefab.
        /// </summary>
        private int index = 0;

        /// <summary>
        /// Gets the array of prefabs that can be selected for placement.
        /// </summary>
        public GameObject[] Prefabs => prefabs;

        #endregion

        #region Lifecycle

        /// <summary>
        /// Initializes the component by setting up the input control listeners.
        /// </summary>
        private void Start()
        {
            inputControl = ObjectPlacement.Instance.GetComponentInChildren<AInputControl>();

            if (inputControl != null)
            {
                inputControl.OnNextPrefab.AddListener(Next);
                inputControl.OnPreviousPrefab.AddListener(Back);
                inputControl.OnPrefabAction.AddListener(SetPrefabIndex);
            }
        }

        #endregion

        #region Prefab selection

        /// <summary>
        /// Sets the array of prefabs that can be selected for placement.
        /// </summary>
        /// <param name="prefabs">Array of prefabs to assign.</param>
        public void SetPrefabs(GameObject[] prefabs)
        {
            this.prefabs = prefabs;
        }

        /// <summary>
        /// Selects the next prefab in the array. If cycling is enabled, wraps around to the first prefab.
        /// </summary>
        public void Next() => UpdateIndex(ClampIndex(index + 1));

        /// <summary>
        /// Selects the previous prefab in the array. If cycling is enabled, wraps around to the last prefab.
        /// </summary>
        public void Back() => UpdateIndex(ClampIndex(index - 1));

        /// <summary>
        /// Sets the prefab index to the specified value if within valid range.
        /// </summary>
        /// <param name="index">The index of the prefab to select.</param>
        public void SetPrefabIndex(int index)
        {
            if (index < 0 || index >= prefabs.Length)
            {
                if (ObjectPlacement.Instance.debugLogsEnabled)
                {
                    Debug.Log("Action for selecting placement prefabs has gone out of range with index: " + index + "!");
                }
            }
            else
            {
                UpdateIndex(index);
            }
        }

        /// <summary>
        /// Clamps the index to ensure it remains within the valid range, optionally cycling if enabled.
        /// </summary>
        /// <param name="newIndex">The new index to clamp.</param>
        /// <returns>The clamped index.</returns>
        private int ClampIndex(int newIndex)
        {
            if (newIndex < 0)
            {
                return cyclesRotation ? prefabs.Length - 1 : 0;
            }
            else if (newIndex >= prefabs.Length)
            {
                return cyclesRotation ? 0 : prefabs.Length - 1;
            }

            return newIndex;
        }

        /// <summary>
        /// Updates the index of the selected prefab and sets the active placement prefab in the ObjectPlacement system.
        /// </summary>
        /// <param name="newIndex">The new index to set.</param>
        private void UpdateIndex(int newIndex)
        {
            if (newIndex != index)
            {
                index = newIndex;

                if (ObjectPlacement.Instance != null)
                {
                    ObjectPlacement.Instance.SetPlacementPrefab(prefabs[index]);
                }
            }
        }

        #endregion
    }

}