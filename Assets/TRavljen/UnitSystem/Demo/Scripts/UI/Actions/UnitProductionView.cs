using UnityEngine;
using UnityEngine.UI;
using TRavljen.Tooltip;

namespace TRavljen.UnitSystem.Demo
{
    using Build;

    /// <summary>
    /// Basic DEMO view component for controlling visual elements
    /// of the production unit action (<see cref="ProductionAction"/>).
    /// </summary>
    public class UnitProductionView : MonoBehaviour
    {

        public enum ProductionActionButtonState
        {
            /// <summary>
            /// Production is enabled.
            /// </summary>
            Default,
            /// <summary>
            /// Production is completed.
            /// </summary>
            Completed,
            /// <summary>
            /// Production is in progress and may be canceled. For
            /// demo this will only happen on ResearchSO producible types.
            /// </summary>
            Cancellable,
            /// <summary>
            /// Production is disabled, does not meet requirements (Cost excluded).
            /// </summary>
            Disabled,
            /// <summary>
            /// Limit on this specific unit type has been reached.
            /// </summary>
            LimitReached
        }

        #region Properties

        [SerializeField]
        private Text titleText;

        [SerializeField]
        private Button actionButton;

        private ProductionAction _action;
        private Text _buttonText;

        public System.Action<ProductionAction> OnActionButtonClicked;

        #endregion

        #region Lifecycle

        private void Awake()
        {
            _buttonText = actionButton.GetComponentInChildren<Text>();
        }

        /// <summary>
        /// Configure UI with action and its state.
        /// </summary>
        /// <param name="action">Production action to be displayed.</param>
        /// <param name="buttonState">Button state of the action.</param>
        /// <param name="enabled">If action is enabled</param>
        public void Configure(ProductionAction action, ProductionActionButtonState buttonState, bool enabled)
        {
            this._action = action;

            var producible = action.ProducibleQuantity.Producible;
            var quantity = action.ProducibleQuantity.Quantity;
            var fullName = GetName(producible, quantity);

            titleText.text = $"{fullName} [{action.KeyCode.ToString()}]";
            _buttonText.text = GetActionButtonText(producible, buttonState);

            if (TryGetComponent(out HoverTooltip tooltip))
            {
                tooltip.information = new TextTooltipInformation(GetTooltipMessage(fullName, producible, quantity));
            }

            actionButton.interactable = enabled;
        }

        private static string GetName(AProducibleSO producible, long quantity)
        {
            var producibleName = producible.name;

            if (quantity > 1)
            {
                return quantity + " " + producibleName + "s";
            }

            return producibleName;
        }

        private static string GetTooltipMessage(string name, AProducibleSO producible, long quantity)
        {
            var message = name + "\n\n";

            if (producible.Description is { Length: > 0 })
                message += producible.Description + "\n\n";

            if (producible.Requirements.Length > 0)
            {
                message += "Requirements: \n";

                foreach (var requirement in producible.Requirements)
                {
                    message += "- ";
                    if (requirement.Quantity > 1)
                    {
                        message += requirement.Quantity + "x ";
                    }
                    message += requirement.Producible.name + "\n";
                }
            }

            message += "Cost: \n";

            foreach (var price in producible.Cost)
            {
                var combinedPrice = price.Quantity * quantity;
                message += "- " + combinedPrice + " " + price.Resource.name + "\n";
            }

            return message;
        }

        #endregion

        #region Actions

        private static string GetActionButtonText(AProducibleSO producible, ProductionActionButtonState buttonState)
        {
            switch (buttonState)
            {
                case ProductionActionButtonState.Cancellable: return "Cancel";
                case ProductionActionButtonState.LimitReached: return "Limit reached";
                case ProductionActionButtonState.Disabled: return "Doesn't meet requirements";
                case ProductionActionButtonState.Completed: return "Researched";
                case ProductionActionButtonState.Default:
                    switch (producible)
                    {
                        case AUnitSO unit: return unit.RequiresBuilding() ? "Build" : "Train";
                        case ResearchSO _: return "Research";
                        default: return "Not defined";
                    }
                default: return null;
            }
        }

        public void ActionButtonClicked()
        {
            OnActionButtonClicked?.Invoke(_action);
        }

        #endregion

    }

}