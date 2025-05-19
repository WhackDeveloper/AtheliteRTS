using System;

namespace TRavljen.UnitSystem
{

    /// <summary>
    /// Contains the necessary information for placing an entity in the game.
    /// </summary>
    public struct PlacementRequiredInfo
    {
        /// <summary>
        /// The player who owns the entity being placed.
        /// </summary>
        public APlayer owner;

        /// <summary>
        /// The entity prefab to be placed.
        /// </summary>
        public IEntity prefab;

        /// <summary>
        /// The quantity of the entity to be placed.
        /// </summary>
        public long quantity;

        /// <summary>
        /// Initializes a new instance of the <see cref="PlacementRequiredInfo"/> struct.
        /// </summary>
        /// <param name="owner">The owner of the entity.</param>
        /// <param name="prefab">The prefab of the entity.</param>
        /// <param name="quantity">The number of entities to be placed.</param>
        public PlacementRequiredInfo(APlayer owner, IEntity prefab, long quantity)
        {
            this.owner = owner;
            this.prefab = prefab;
            this.quantity = quantity;
        }
    }

    /// <summary>
    /// Defines a contract for handling production requests within the production system.
    /// </summary>
    public interface IProductionRequestHandler
    {
        /// <summary>
        /// Handles the production request by verifying limits, checking and consuming
        /// resources, initiating production or placement, etc.
        /// </summary>
        /// <param name="productionQuantity">The production quantity.</param>
        /// <param name="entity">The entity requesting production.</param>
        /// <param name="placementRequired">Action callback for placement when required.</param>
        public void HandleProductionRequest(ProducibleQuantity productionQuantity, IEntity entity, Action<PlacementRequiredInfo> placementRequired);
    }

}