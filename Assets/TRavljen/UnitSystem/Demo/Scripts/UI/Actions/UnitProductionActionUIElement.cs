using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TRavljen.UnitSystem.Demo
{

    /// <summary>
    /// Action element for production actions using <see cref="ProductionAction"/>
    /// and <see cref="UnitProductionView"/>.
    /// </summary>
    public class UnitProductionActionUIElement : AEntityActionUIElement
    {

        [SerializeField]
        UnitProductionView productionView;

        private IEntity entity;
        private IEntityUIAction action;

        public override void SetAction(IEntityUIAction action)
        {
            this.action = action;
            productionView.OnActionButtonClicked += ClickHandler;

            if (action is ProductionAction prodAction)
            {
                var state = GetButtonState(prodAction, entity);
                bool enabled = IsActionButtonEnabled(state);

                productionView.Configure(prodAction, state, enabled);
            }
            else
            {
                Debug.LogWarning($"Invalid action passed, expected ProductionAction type, received: ${action}");
            }
        }

        public override bool CheckActionHotKey()
        {
            if (action is not ProductionAction prodAction ||
                !Input.GetKeyDown(prodAction.KeyCode) ||
                !entity.Owner.TryGetModule(out ProductionModule module)) 
                return false;

            module.RequestProduction(prodAction.ProducibleQuantity, entity);
            return true;
        }

        public override void Configure(IEntity entity)
        {
            this.entity = entity;
        }

        private void ClickHandler(ProductionAction action)
        {
            action.Execute(entity);
        }

        /// <summary>
        /// Determines the state of a production action button based on the action's requirements and current state.
        /// </summary>
        private UnitProductionView.ProductionActionButtonState GetButtonState(
            ProductionAction action, IEntity entity)
        {
            var producible = action.ProducibleQuantity.Producible;
            var cancellable = producible is ResearchSO
                && entity is Unit unit
                && unit.ActiveProduction != null &&
                unit.ActiveProduction.IsProducing(producible);

            var fulfillsRequirements = entity.Owner
                .FulfillsRequirements(producible, action.ProducibleQuantity.Quantity);

            var complete = !cancellable && producible is ResearchSO && entity.Owner
                .HasRegisteredProducible(new(producible, 1));

            if (entity.Owner.TryGetModule(out UnitLimitationModule unitLimits) &&
                unitLimits.HasReachedLimit(producible.ID))
            {
                return UnitProductionView.ProductionActionButtonState.LimitReached;
            }

            if (complete)
            {
                return UnitProductionView.ProductionActionButtonState.Completed;
            }
            else if (cancellable)
            {
                return UnitProductionView.ProductionActionButtonState.Cancellable;
            }
            else if (fulfillsRequirements)
            {
                return UnitProductionView.ProductionActionButtonState.Default;
            }

            return UnitProductionView.ProductionActionButtonState.Disabled;
        }

        private bool IsActionButtonEnabled(UnitProductionView.ProductionActionButtonState buttonState)
        {
            return buttonState == UnitProductionView.ProductionActionButtonState.Default ||
                buttonState == UnitProductionView.ProductionActionButtonState.Cancellable;
        }
    }

}