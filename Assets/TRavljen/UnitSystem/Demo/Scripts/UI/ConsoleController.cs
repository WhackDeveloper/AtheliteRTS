using UnityEngine;
using UnityEngine.UI;
using System.Text;

namespace TRavljen.UnitSystem.Demo
{

    using TRavljen.UnitSystem;
    using Build;
    using Combat;

    /// <summary>
    /// Component for displaying game events in a scrollable console UI.
    /// Observes various game events like unit production, combat, and building completions,
    /// and updates the console with relevant messages.
    /// </summary>
    public class ConsoleController : MonoBehaviour
    {

        [Tooltip("Reference to the player whose events will be observed and displayed.")]
        public APlayer player;

        [Tooltip("UI text element used to display console messages.")]
        [SerializeField]
        private Text text;

        [Tooltip("ScrollRect component used to keep the console scrolled to the latest message.")]
        [SerializeField]
        private ScrollRect scrollRect;

        /// <summary>
        /// Internal buffer for accumulating console messages.
        /// </summary>
        private readonly StringBuilder stringBuilder = new();

        /// <summary>
        /// Subscribes to game events when the component is enabled.
        /// </summary>
        private void OnEnable()
        {
            BuildEvents.Instance.OnBuildCompleted.AddListener(HandleEntityBuilt);
            ProductionEvents.Instance.OnProductionFinished.AddListener(HandleProductionFinished);
            CombatEvents.Instance.OnHealthDepleted.AddListener(HandleUnitKilled);
        }

        /// <summary>
        /// Unsubscribes from game events when the component is disabled.
        /// </summary>
        private void OnDisable()
        {
            BuildEvents.Instance.OnBuildCompleted.RemoveListener(HandleEntityBuilt);
            ProductionEvents.Instance.OnProductionFinished.RemoveListener(HandleProductionFinished);
            CombatEvents.Instance.OnHealthDepleted.RemoveListener(HandleUnitKilled);
        }

        /// <summary>
        /// Initializes the console with a starting message.
        /// </summary>
        private void Start()
        {
            AppendText("Game started...");
        }

        /// <summary>
        /// Updates the console's scroll position to ensure the latest messages are visible.
        /// </summary>
        private void Update()
        {
            scrollRect.verticalNormalizedPosition = 0;
        }

        /// <summary>
        /// Handles the event when a unit is killed, and logs the information to the console.
        /// </summary>
        private void HandleUnitKilled(IHealth health, APlayer owner, AUnitComponent attacker)
        {
            // Check if player is the owner and cast to expected type duo to naming data.
            if (health.Entity is Entity entity && owner == player)
            {
                AppendText($"Lost {entity.Data.Name}");
            }
        }

        /// <summary>
        /// Handles the event when a unit building is completed, and logs the information to the console.
        /// </summary>
        private void HandleEntityBuilt(EntityBuilding entityBuilding)
        {
            // Confirm ownership, other players may build as well
            if (entityBuilding.Entity.Owner == player)
                AppendText("Unit building finished " + entityBuilding.Entity.Data.Name);
        }

        /// <summary>
        /// Handles the event when production is finished, and logs the information to the console.
        /// </summary>
        private void HandleProductionFinished(ActiveProduction unitProduction, ProducibleQuantity producibleQuantity)
        {
            // Confirm ownership of the production entity.
            if (unitProduction.Entity.Owner != player) return;

            switch (producibleQuantity.Producible)
            {
                // Too frequent for passive production
                case ResourceSO resource:
                    AppendText("Resource producted: " + resource.Name + " " + producibleQuantity.Quantity);
                    break;

                case AUnitSO unit:
                    AppendText("Produced " + producibleQuantity.Quantity + " " + unit.Name);
                    break;

                case ResearchSO research:
                    AppendText("Researched " + research.Name);
                    break;
            }
        }

        /// <summary>
        /// Appends a formatted message to the console.
        /// </summary>
        private void AppendText(string text)
        {
            int seconds = (int)(Time.time % 60f);
            int minutes = (int)(Time.time / 60f);

            stringBuilder.Append("[");
            stringBuilder.Append(minutes);
            stringBuilder.Append(":");
            stringBuilder.Append(seconds);
            stringBuilder.Append("]: ");
            stringBuilder.Append(text);
            stringBuilder.Append("\n");

            this.text.text = stringBuilder.ToString();
        }

    }

}