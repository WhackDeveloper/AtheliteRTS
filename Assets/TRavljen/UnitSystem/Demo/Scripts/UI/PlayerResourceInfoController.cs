using System.Collections;
using System.Collections.Generic;
using TRavljen.UnitSystem;
using UnityEngine;
using UnityEngine.UI;

namespace TRavljen.UnitSystem.Demo
{

    /// <summary>
    /// Manages the UI display for player resources and population. Updates resource 
    /// and population valbues in real-time by observing changes in the player's 
    /// <see cref="ResourceModule"/> and <see cref="PopulationModule"/>.
    /// </summary>
    public class PlayerResourceInfoController : MonoBehaviour
    {

        [System.Serializable]
        struct ResourceText
        {
            [Tooltip("The resource type this text corresponds to.")]
            public ResourceSO resource;

            [Tooltip("The UI text element displaying the resource information.")]
            public Text text;
        }

        #region Properties

        private readonly List<(ResourceSO resource, ResourceTextElement text)> resources = new();
        private ResourceTextElement population;

        [Tooltip("Specifies the root transform where resource UI elements will be added to.")]
        [SerializeField]
        private Transform container;

        [Tooltip("Specifies the resource UI element prefab.")]
        [SerializeField]
        private ResourceTextElement resourceElementPrefab;

        /// <summary>
        /// The player component providing access to resource and population data.
        /// </summary>
        public APlayer player;

        private PopulationModule populationManager;
        private ResourceModule resourceManager;

        #endregion

        #region Lifecycle

        void Awake()
        {
            populationManager = player.GetModule<PopulationModule>();
            resourceManager = player.GetModule<ResourceModule>();

            if (resourceManager.IsNotNull())
                UpdateResourcesUI();
            else
            {
                Debug.LogWarning("Player missing resource module, disabling resources UI controller.");
                enabled = false;
            }
        }

        private void OnEnable()
        {
            if (populationManager.IsNotNull())
            {
                UpdatePopulationUI(
                    populationManager.CurrentPopulation,
                    populationManager.MaxPopulation);
            }

            // Subscribe to events
            resourceManager?.OnResourceUpdate.AddListener(OnResourceUpdated);
            populationManager?.OnPopulationUpdate.AddListener(OnPopulationUpdated);
        }

        private void OnDisable()
        {
            // Unsubscribe from events
            resourceManager?.OnResourceUpdate.RemoveListener(OnResourceUpdated);
            populationManager?.OnPopulationUpdate.RemoveListener(OnPopulationUpdated);
        }

        #endregion

        #region Convenience

        /// <summary>
        /// Updates the UI for all resources based on the player's current resource data.
        /// </summary>
        private void UpdateResourcesUI()
        {
            var resources = resourceManager.GetResources();
            
            for (int index = 0; index < resources.Length; index++)
            {
                var resourceQuantity = resources[index];
                string name = resourceQuantity.Resource.Name;
                int quantity = (int)resourceQuantity.Quantity;
                long capacity = resourceManager.GetResourceCapacity(resourceQuantity.Resource.ID);

                if (TryGetResouceText(resourceQuantity.Resource, out ResourceTextElement resText))
                {
                    string content = string.Format("{0}: {1} / {2}", name, quantity, capacity);
                    resText.SetText(content);
                }
            }
        }

        /// <summary>
        /// Updates the UI for population information based on current and maximum population values.
        /// </summary>
        /// <param name="current">The current population count.</param>
        /// <param name="max">The maximum population capacity.</param>
        private void UpdatePopulationUI(int current, int max)
        {
            if (population == null)
            {
                population = Instantiate(resourceElementPrefab, container);
                population.transform.SetAsLastSibling();
                population.gameObject.SetActive(true);
            }

            if (populationManager.MaxPopulationEnabled)
            {
                population.SetText("Population: " + current + "/" + max);
            }
            else
            {
                // No maximal population
                population.SetText("Population: " + current);
            }

        }

        /// <summary>
        /// Attempts to find the corresponding <see cref="ResourceTextElement"/> for a given resource.
        /// </summary>
        /// <param name="resource">The resource to find.</param>
        /// <param name="resourceText">The corresponding <see cref="ResourceTextElement"/>, if found.</param>
        /// <returns><c>true</c> if the resource text was found; otherwise, <c>false</c>.</returns>
        private bool TryGetResouceText(ResourceSO resource, out ResourceTextElement resourceText)
        {
            foreach (var (_resource, text) in resources)
            {
                if (_resource.ID == resource.ID)
                {
                    resourceText = text;
                    return true;
                }
            }

            resourceText = Instantiate(resourceElementPrefab, container);
            resourceText.gameObject.SetActive(true);
            resourceText.transform.SetAsFirstSibling();
            resources.Add((resource, resourceText));

            return true;
        }

        #endregion

        #region Event Handlers

        /// <summary>
        /// Handles the resource update event and refreshes the UI.
        /// </summary>
        /// <param name="resource">The resource that was updated.</param>
        private void OnResourceUpdated(ResourceSO resource)
        {
            UpdateResourcesUI();
        }

        /// <summary>
        /// Handles the population update event and refreshes the UI.
        /// </summary>
        /// <param name="current">Current population count.</param>
        /// <param name="max">Max population capacity.</param>
        private void OnPopulationUpdated(int current, int max)
        {
            UpdatePopulationUI(current, max);
        }

        #endregion
    }

}