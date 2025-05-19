using UnityEngine;

namespace TRavljen.UnitSystem.Collection
{
    /// <summary>
    /// Implementation of the resource depot capability, specifying supported resources.
    /// </summary>
    public struct ResourceDepotCapability : IResourceDepotCapability
    {
        [SerializeField]
        private ResourceSO[] supportedResources;

        /// <inheritdoc/>
        /// <remarks>
        /// If this array is empty, the depot accepts deposits of any resource type.
        /// </remarks>
        readonly public ResourceSO[] SupportedResources => supportedResources;

        #region IEntityCapability

        readonly void IEntityCapability.ConfigureEntity(IEntity entity)
        {
            // Adds the ResourceDepot component to the entity, enabling depot behavior in the game world.
            entity.gameObject.AddComponentIfNotPresent<ResourceDepot>();
        }

        void IEntityCapability.SetDefaultValues()
        {
            supportedResources = null;
        }

        #endregion

    }
}