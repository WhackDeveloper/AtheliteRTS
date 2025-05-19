using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TRavljen.UnitSystem.Collection;

namespace TRavljen.UnitSystem.Demo
{

    /// <summary>
    /// Displays the current state of a <see cref="ResourceNode"/> as a floating text for debugging or demo purposes.
    /// </summary>
    /// <remarks>
    /// This component visualizes the state of a resource node, such as its active interactions, interaction limit, 
    /// resource type, and remaining resource quantity. It provides a simple debugging utility to monitor resource node states.
    ///
    /// <para>
    /// **Features:**
    /// - Automatically hides the text when the node is depleted or has no active interactions.
    /// - Displays text in the format: "{ActiveInteractions}/{InteractionLimit}\n{ResourceName} {ResourceQuantity}".
    /// - Updates text dynamically and ensures it always faces the camera for readability.
    /// </para>
    ///
    /// <para>
    /// **Usage Notes:**
    /// - Assign a <see cref="ResourceNode"/> to the <see cref="node"/> field, or it will be auto-assigned 
    ///   from the <see cref="GameObject"/> this component is attached to.
    /// - Requires a <see cref="UnityEngine.UI.Text"/> and a <see cref="UnityEngine.Canvas"/> component for rendering the text.
    /// - The canvas should be childed to the node's <see cref="GameObject"/> for proper positioning.
    /// </para>
    /// </remarks>
    public class DebugNodeStateText : MonoBehaviour
    {

        #region Fields

        [SerializeField]
        [Tooltip("The node for which the state will be shown for.")]
        private ResourceNode node;

        [SerializeField]
        [Tooltip("The Unity UI Text component used to display the node state.")]
        private Text text;

        [SerializeField]
        [Tooltip("The Canvas component that contains the text.")]
        private Canvas canvas;

        private Transform cameraTransform;
        private Transform canvasTranform;

        #endregion

        /// <summary>
        /// Initializes references and caches required transforms.
        /// </summary>
        private void Awake()
        {
            // Try to get node from game object if its not set.
            if (node == null)
                node = GetComponent<ResourceNode>();

            // Cache transforms
            cameraTransform = Camera.main.transform;
            canvasTranform = canvas.transform;
        }

        /// <summary>
        /// Updates the text and visibility based on the node's state, ensuring the canvas faces the camera.
        /// </summary>
        private void Update()
        {
            if (node == null || node.IsDepleted || node.ActiveInteractions == 0)
            {
                canvas.enabled = false;
                return;
            }

            canvas.enabled = true;

            // Ensure the text always faces the camera
            canvasTranform.LookAt(cameraTransform.position);

            UpdateText();
        }

        /// <summary>
        /// Updates the displayed text with the node's current state.
        /// </summary>
        private void UpdateText()
        {
            text.text = string.Format("{0}/{1}\n{2} {3}",
                node.ActiveInteractions,
                node.InteractionLimit,
                node.ResourceAmount.Resource.Name,
                ((int)node.ResourceAmount.Quantity).ToString());
        }
    }

}