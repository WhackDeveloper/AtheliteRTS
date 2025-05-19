using System;
using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TRavljen.UnitSystem.Demo
{

    using Combat;

    /// <summary>
    /// Manages the UI display for information about the currently selected entity or building in the demo scene. 
    /// Displays details such as health, passive resource production, current production progress, and production queue.
    /// </summary>
    public class SelectedEntityInfoController : MonoBehaviour
    {

        #region Properties

        [Tooltip("The text element displaying production-related information for the selected entity.")]
        [SerializeField]
        private Text productionText;

        [Tooltip("The text element displaying general information about the selected entity.")]
        [SerializeField]
        private Text entityInfoText;

        [Tooltip("The entity selection manager responsible for tracking selected entities.")]
        public EntitySelectionManager selectionManager;

        /// <summary>
        /// The currently selected entity.
        /// </summary>
        private Entity selectedEntity;

        #endregion

        #region Lifecycle

        private void OnValidate()
        {
            if (selectionManager.IsNull())
                selectionManager = UnityEngine.Object.FindFirstObjectByType<EntitySelectionManager>();
        }

        private void Update()
        {
            // Continuously update the UI to reflect real-time production
            // progress and node resource amount.
            // This could be replaced with event-based updates.
            productionText.text = GetProductionInfoText();
            entityInfoText.text = GetEntityInfoText();
        }

        private void OnEnable()
        {
            selectionManager.OnSelectionChange.AddListener(SelectionChanged);
        }

        private void OnDisable()
        {
            selectionManager.OnSelectionChange.RemoveListener(SelectionChanged);
        }

        private void SelectionChanged(List<Entity> selectedEntities)
        {
            if (selectedEntities.Count > 0)
                selectedEntity = selectedEntities[0];
            else
                selectedEntity = null;

            productionText.text = GetProductionInfoText();
            entityInfoText.text = GetEntityInfoText();
        }

        #endregion

        #region Convenience

        /// <summary>
        /// Generates the production-related information for the selected entity.
        /// </summary>
        /// <returns>A string containing production-related information.</returns>
        private string GetProductionInfoText()
        {
            var text = "";

            // Check if any entity is selected
            if (selectedEntity == null) return text;

            // This can be cached and done once after selection, since it is defined
            // as static data.
            text += GetPassiveProductionInfo(selectedEntity);

            // Here you can hook to production events on selection to avoid real-time updated.
            // Constant creation of strigs can be expensive.
            if (selectedEntity.TryGetComponent(out IActiveProduction production))
            {
                text += GetCurrentProductionProgress(production);
                text += GetQueuedProductionInfo(production);
            }

            return text;
        }

        /// <summary>
        /// Generates the general information for the selected entity.
        /// </summary>
        /// <returns>A string containing entity-related information.</returns>
        private string GetEntityInfoText()
        {
            if (selectedEntity == null) return "No selected entity";

            StringBuilder builder = new();

            // Could be improved by caching after selection
            if (selectedEntity.Data.Description.Length > 0)
            {
                builder.Append(selectedEntity.Data.Description);
                builder.Append("\n\n");
            }

            // Can be improved with observers similar to health bar component.
            if (selectedEntity.TryGetComponent(out Health health))
            {
                builder.Append("Health: ");
                builder.Append(health.CurrentHealth);
                builder.Append(" / ");
                builder.Append(health.MaxHealth);
                builder.Append("\n\n");
            }

            if (selectedEntity.TryGetEntityComponent(out Build.EntityBuilding building) &&
                building.Entity.IsOperational == false)
            {
                if (building.IsBuilt == false)
                {
                    builder.Append(string.Format("Building unit: {0}%", (int)(building.Progress * 100)));
                }
                else
                {
                    builder.Append("Unit not operational");
                }

                // Not operational, stop here
                return builder.ToString();
            }

            if (selectedEntity.TryGetComponent(out Collection.ResourceCollector collector) &&
                collector.CollectedResource.Resource != null)
            {
                builder.Append(collector.CollectedResource.Resource.Name);
                builder.Append(": ");
                builder.Append(collector.CollectedResource.Quantity);
                builder.Append("\n\n");
            }

            if (selectedEntity.TryGetEntityComponent(out Collection.ResourceNode node))
            {
                builder.Append("<b>RESOURCE</b>\n");
                builder.Append(node.ResourceAmount.Resource.Name + ": " + node.ResourceAmount.Quantity);
            }

            return builder.ToString();
        }

        /// <summary>
        /// Retrieves information about passive resource production.
        /// </summary>
        /// <param name="selectedEntity">The selected entity.</param>
        /// <returns>A string containing passive resource production information.</returns>
        private string GetPassiveProductionInfo(Entity selectedEntity)
        {
            string text = "";

            if (selectedEntity.TryGetCapability(out IPassiveResourceProductionCapability passiveProduction) &&
                passiveProduction.ProducesResource.Length > 0)
            {
                text += "Passive Resources: \n\n";

                foreach (var resource in passiveProduction.ProducesResource)
                {
                    text += resource.Resource.Name + ": " + resource.Quantity + " per second\n";
                }

                if (selectedEntity is Unit selectedUnit && !selectedUnit.IsOperational)
                {
                    text += "\nResources will start producing after construction.";
                }

                text += "\n";
            }

            return text;
        }

        /// <summary>
        /// Retrieves information about the current production progress of the selected unit.
        /// </summary>
        /// <param name="production">The production component of the selected unit.</param>
        /// <returns>A string containing the production progress information.</returns>
        private string GetCurrentProductionProgress(IActiveProduction production)
        {
            var progress = production.CurrentProductionProgress;
            var queue = production.GetProductionQueue();

            // Check if factory is producing
            if (progress != -1 && queue.Length > 0)
            {
                // Then display its name and progress
                return queue[0].Producible.Name + " (" + queue[0].Quantity + ")" + ": " + Mathf.Round(progress * 100) + "%\n";
            }

            return "";
        }

        /// <summary>
        /// Retrieves information about the production queue of the selected unit.
        /// </summary>
        /// <param name="production">The production component of the selected unit.</param>
        /// <returns>A string containing the production queue information.</returns>
        private string GetQueuedProductionInfo(IActiveProduction production)
        {
            List<AProducibleSO> remainingQueue = new List<AProducibleSO>(
                production.GetProductionQueue().Select(item => item.Producible));

            // First remove the one in progress
            if (remainingQueue.Count > 0)
            {
                remainingQueue.RemoveAt(0);
            }

            string text = "";

            // Check if remainder is more than 0, then add name and count
            // to the text
            if (remainingQueue.Count > 0)
            {
                // Remove first as its in progress

                text += "\nQUEUED: \n";

                var queuedItems =
                    from producible in remainingQueue
                    group producible by producible.Name into Name
                    select new { Name = Name.Key, Count = Name.Count() };

                var size = queuedItems.Count();
                for (int index = 0; index < size; index++)
                {
                    var item = queuedItems.ElementAt(index);
                    text += " (" + item.Count + ") " + item.Name;

                    if (index + 1 != size)
                    {
                        text += ",\n";
                    }
                }
            }

            return text;
        }

        #endregion

    }

}