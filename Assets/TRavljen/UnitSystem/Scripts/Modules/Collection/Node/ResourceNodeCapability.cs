using UnityEngine;

namespace TRavljen.UnitSystem.Collection
{
    /// <summary>
    /// Implementation of the resource node capability, specifying the resource type and quantity.
    /// </summary>
    public struct ResourceNodeCapability : IResourceNodeCapability
    {
        [SerializeField] private ResourceQuantity resource;

        /// <inheritdoc/>
        public readonly ResourceQuantity Resource => resource;

        #region IEntityCapability

        /// <inheritdoc/>
        readonly void IEntityCapability.ConfigureEntity(IEntity entity)
        {
            // Ensures the entity has a ResourceNode component to represent the capability in the game world.
            entity.gameObject.AddComponentIfNotPresent<ResourceNode>();
        }

        /// <inheritdoc />
        void IEntityCapability.SetDefaultValues()
        {
            resource = new ResourceQuantity();
        }

        #endregion
    }
}