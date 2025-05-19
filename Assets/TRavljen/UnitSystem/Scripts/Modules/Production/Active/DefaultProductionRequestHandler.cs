using System;
using TRavljen.UnitSystem.Build;

namespace TRavljen.UnitSystem
{

    /// <summary>
    /// Default implementation for handling production requests, including resource checks, unit limitations, and production initiation.
    /// </summary>
    public class DefaultProductionRequestHandler : IProductionRequestHandler
    {

        private readonly ResourceModule resourceModule;
        private readonly UnitLimitationModule limitModule;

        private readonly bool isLimitModuleSet;
        private readonly bool isResourceModuleSet;

        /// <summary>
        /// Invoked when production is requested for an entity which does not support production of it,
        /// neither can it be placed.
        /// </summary>
        public Action<IEntity, ProducibleQuantity> OnProductionMissingOnEntity;
        
        /// <summary>
        /// Invoked when production is requested but player does not have enough resources.
        /// </summary>
        public Action<ProducibleQuantity> OnNotEnoughResources;
        
        /// <summary>
        /// Invoked when production is requested but player does not fulfill requirements.
        /// </summary>
        public Action<ProducibleQuantity> OnRequirementsNotFulfilled;
    
        public DefaultProductionRequestHandler(APlayer player)
        {
            isResourceModuleSet = player.TryGetModule(out resourceModule);
            isLimitModuleSet = player.TryGetModule(out limitModule);
        }

        public void HandleProductionRequest(ProducibleQuantity productionQuantity, IEntity entity, Action<PlacementRequiredInfo> placementRequired)
        {
            var producible = productionQuantity.Producible;

            if (isLimitModuleSet && limitModule.HasReachedLimit(producible.ID))
            {
                // Limit reached
                return;
            }

            var quantity = productionQuantity.Quantity;

            // Start placement process if it requires construction
            if (producible is AEntitySO entityToProduce && entityToProduce.RequiresBuilding())
            {
                StartPlacement(entity.Owner, entityToProduce, quantity, placementRequired);
                return;
            }

            // If action is not for Buildable unit, then it is expected to be for Unit Production.
            if (!entity.TryGetEntityComponent(out IActiveProduction production))
            {
                OnProductionMissingOnEntity?.Invoke(entity, productionQuantity);
                return;
            }

            // Check if producible is research & if research production is already in progress
            if (producible is ResearchSO && production.IsProducing(producible))
            {
                // Cancel production and refund resources
                production.CancelProductionOrder(producible);

                if (isResourceModuleSet)
                {
                    resourceModule.AddResources(producible.Cost);
                }
                return;
            }

            // Otherwise if it fulfills requirements & cost, start production
            if (entity.Owner.FulfillsRequirements(producible, quantity))
            {
                var fullCost = ResourceQuantity.GetFullCost(producible.Cost, quantity);

                // Make sure that full cost of the action is calculate because if
                // you only use action.Producible.Cost and action.Quantity is not
                // equal 1, then check for enough resources and consuming them won't
                // be the correct quantity.
                if (isResourceModuleSet)
                {
                    if (resourceModule.HasEnoughResources(fullCost))
                    {
                        resourceModule.RemoveResources(fullCost);
                        production.StartProduction(productionQuantity, false);
                    }
                    else
                    {
                        OnNotEnoughResources?.Invoke(productionQuantity);
                    }
                }
                else
                {
                    // No resource module present, simply start production.
                    production.StartProduction(productionQuantity, false);
                }
            }
            else
            {
                OnRequirementsNotFulfilled?.Invoke(productionQuantity);
            }
        }

        /// <summary>
        /// Start placement with action by retrieving it's prefab, but before hat
        /// validates that the player has enough resources to start the placement.
        /// </summary>
        /// <param name="owner">Owner of the entity to be placed.</param>
        /// <param name="entity">Entity to be placed</param>
        /// <param name="quantity">Number of units to be placed</param>
        /// <param name="placementRequired">Callback invoked when placement is required, after loading the prefab.</param>
        private void StartPlacement(APlayer owner, AEntitySO entity, long quantity, Action<PlacementRequiredInfo> placementRequired)
        {
            if (limitModule.IsNotNull() && limitModule.HasReachedLimit(entity.ID))
            {
                // Limit reached
                return;
            }
            
            // Otherwise if it fulfills requirements & cost, start production
            if (owner.FulfillsRequirements(entity, quantity))
            {
                var fullCost = ResourceQuantity.GetFullCost(entity.Cost, quantity);

                // Make sure that full cost of the action is calculate because if
                // you only use action.Producible.Cost and action.Quantity is not
                // equal 1, then check for enough resources and consuming them won't
                // be the correct quantity.
                if (!isResourceModuleSet || resourceModule.HasEnoughResources(fullCost))
                {
                    entity.LoadEntityPrefab(prefab =>
                    {
                        placementRequired.Invoke(new(owner, prefab, quantity));
                    });
                }
                else
                {
                    OnNotEnoughResources?.Invoke(new ProducibleQuantity(entity, quantity));
                }
            }
            else
            {
                OnRequirementsNotFulfilled?.Invoke(new ProducibleQuantity(entity, quantity));
            }
        }
    }

}