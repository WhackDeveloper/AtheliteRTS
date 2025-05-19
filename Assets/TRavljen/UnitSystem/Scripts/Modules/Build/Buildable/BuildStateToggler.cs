using UnityEngine;

namespace TRavljen.UnitSystem.Build
{

    /// <summary>
    /// A component that visualizes the build state of a unit by enabling or disabling
    /// a specified GameObject (e.g., a sprite or mesh) when build events occur.
    /// </summary>
    [DisallowMultipleComponent]
    public class BuildStateToggler : MonoBehaviour
    {

        /// <summary>
        /// The <see cref="EntityBuilding"/> associated with this visualizer.
        /// </summary>
        [Tooltip("The UnitBuilding component this visualizer will observe.")]
        [SerializeField] private EntityBuilding entityBuilding;

        /// <summary>
        /// The GameObjects used to represent the build state visually (e.g., a sprite or mesh).
        /// </summary>
        [Tooltip("The GameObject array that will be toggled to indicate the build state.")]
        [SerializeField]
        private GameObject[] objects = new GameObject[0];

        private void OnValidate()
        {
            // Try to help and find component if it is on the gameobject itself.
            if (entityBuilding == null)
                entityBuilding = GetComponent<EntityBuilding>();
        }

        /// <summary>
        /// Subscribes to the relevant build events when this component is enabled.
        /// </summary>
        private void OnEnable()
        {
            entityBuilding.OnBuildStarted.AddListener(ShowIndicator);
            entityBuilding.OnBuildCompleted.AddListener(HideIndicator);
            entityBuilding.OnBuildCanceled.AddListener(HideIndicator);
        }

        /// <summary>
        /// Subscribes to the relevant build events when this component is enabled.
        /// </summary>
        private void OnDisable()
        {
            entityBuilding.OnBuildStarted.RemoveListener(ShowIndicator);
            entityBuilding.OnBuildCompleted.RemoveListener(HideIndicator);
            entityBuilding.OnBuildCanceled.RemoveListener(HideIndicator);
        }

        /// <summary>
        /// Activates the indicator to show that the build process has started.
        /// </summary>
        private void ShowIndicator() => SetObjectState(true);

        /// <summary>
        /// Deactivates the indicator to hide it when the build process completes or is canceled.
        /// </summary>
        private void HideIndicator() => SetObjectState(false);

        /// <summary>
        /// Sets <see cref="objects"/> active state.
        /// </summary>
        /// <param name="active">New active state</param>
        private void SetObjectState(bool active)
        {
            foreach (var obj in objects)
            {
                obj.SetActive(active);
            }
        }

    }

}