using UnityEngine;

namespace TRavljen.UnitSystem
{
    
#if UNITY_EDITOR
    using UnityEditor;
    using TRavljen.EditorUtility;

    internal static class PlayerEditorTools
    {
        [MenuItem("GameObject/TRavljen/UnitSystem/Player")]
        public static void CreateNewPlayerInScene()
        {
            if (EditorTools.CreateObjectFromMenu<Player>("New Player", false))
            {
                Debug.Log("New player created. Now set its faction & add modules.");
            }
        }
    }
#endif

    /// <summary>
    /// Default implementation of <see cref="APlayer"/>. Provides integration with standard player modules,
    /// such as resource management, population control, and research. 
    /// Allows for managing player units, resources, and production.
    /// 
    /// <para>
    /// This implementation can be replaced with a custom version by deriving from <see cref="APlayer"/>.
    /// </para>
    /// </summary>
    public class Player : APlayer
    {

        #region Properties

        protected ResearchModule researchModule;
        protected ResourceModule resourceModule;
        protected PopulationModule populationModule;

        /// <summary>
        /// Gets the player's resource management module.
        /// </summary>
        public ResourceModule ResourceModule => resourceModule;

        #endregion

        #region Lifecycle

        /// <inheritdoc/>
        protected override void Awake()
        {
            base.Awake();

            populationModule = GetModule<PopulationModule>();
            resourceModule = GetModule<ResourceModule>();
            researchModule = GetModule<ResearchModule>();

            // Take resources from faction. This can be altered on Start.
            if (Faction != null)
            {
                Faction.ConfigurePlayer(this);
            }
        }

        #endregion

        public override bool ArePlayersAllied(APlayer other)
        {
            return PlayersRelationshipManager.GetOrCreate().AreAllied(this, other);
        }

        #region Units

        /// <inheritdoc/>
        public override void RefundPlayer(IUnit unit)
        {
            // Handle unit production, this is generally empty for non operational units
            if (unit.ActiveProduction != null)
            {
                var resources = unit.ActiveProduction.CancelProduction();
                resourceModule.AddResources(resources);
            }

            var refund = unit.GetRefundResources();
            resourceModule.AddResources(refund);
        }

        /// <inheritdoc/>
        public override long AddResource(ResourceQuantity resourceQuantity)
        {
            if (resourceModule != null)
                return resourceModule.AddResource(resourceQuantity);
            return resourceQuantity.Quantity;
        }

        /// <inheritdoc/>
        public override bool HasRegisteredProducible(ProducibleQuantity producibleQuantity)
        {
            int producibleID = producibleQuantity.Producible.ID;

            switch (producibleQuantity.Producible)
            {
                case ResearchSO research:
                    return researchModule.IsResearchComplete(research);

                case AUnitSO _:
                    // Similar as for entity, but prioritised duo to `isOperational` check.
                    int quantity = 0;

                    foreach (var unit in units)
                        if (unit.IsOperational && unit.Data.ID == producibleID)
                            quantity++;

                    return quantity >= producibleQuantity.Quantity;

                case AEntitySO _:
                    quantity = 0;

                    foreach (var entity in entities)
                        if (entity.Data.ID == producibleID)
                            quantity++;

                    return quantity >= producibleQuantity.Quantity;

                case ResourceSO resource:
                    return resourceModule.HasEnoughResource(
                        new(resource, producibleQuantity.Quantity));

                default: return false;
            }
        }

        #endregion

    }

}
