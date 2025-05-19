using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace TRavljen.PlacementSystem.Demo
{

    /// <summary>
    /// Demonstrates how to create UI buttons dynamically to allow the user to select different placement prefabs.
    /// This script is designed for a demo scene and shows how prefabs can be presented as clickable buttons in a UI.
    /// </summary>
    public class TemplateActionsUI : MonoBehaviour
    {
        #region Fields

        /// <summary>
        /// The button template used to create new buttons for each prefab.
        /// This template is duplicated and populated with data for each prefab in the list.
        /// </summary>
        [SerializeField]
        private Button template;

        /// <summary>
        /// Reference to the <see cref="PlacementPrefabs"/> component that manages the list of prefabs.
        /// The selected prefab will be set through this component.
        /// </summary>
        [SerializeField]
        private PlacementPrefabs placement;

        /// <summary>
        /// A list of buttons generated for each prefab.
        /// Stored to manage their lifecycle and to clean them up when the UI is disabled.
        /// </summary>
        [SerializeField, HideInInspector]
        private List<Button> buttons = new();

        #endregion

        #region Lifecycle Methods

        /// <summary>
        /// Called when the UI is enabled. Dynamically creates buttons for each prefab in the <see cref="PlacementPrefabs"/>.
        /// Each button will set the corresponding prefab as the active prefab for placement when clicked.
        /// </summary>
        private void OnEnable()
        {
            if (placement == null)
            {
                placement = ObjectPlacement.Instance.PlacementPrefabs;
            }

            if (placement.Prefabs != null)
            {
                // Iterate through each prefab and create a corresponding button
                for (int index = 0; index < placement.Prefabs.Length; index++)
                {
                    var prefab = placement.Prefabs[index];
                    // Create a new button from the template
                    var button = Instantiate(template, transform);

                    // Ensure the button is active (scene template is disabled by default)
                    button.gameObject.SetActive(true);
                    // Set the button text to the prefab's name
                    button.GetComponentInChildren<Text>().text = prefab.name;

                    // Capture the index to use in the lambda expression
                    int currentIndex = index;
                    button.onClick.AddListener(delegate
                    {
                        // Set the active prefab when the button is clicked
                        placement.SetPrefabIndex(currentIndex);
                    });

                    // Add the button to the list for future reference
                    buttons.Add(button);
                }
            }
        }

        /// <summary>
        /// Called when the UI is disabled. Destroys all buttons created by this script to clean up the scene.
        /// </summary>
        private void OnDisable()
        {
            // Destroy each button game object
            foreach (var button in buttons)
                Destroy(button.gameObject);

            // Clear the list to remove references to destroyed buttons
            buttons.Clear();
        }

        #endregion
    }


}