using UnityEngine;

namespace TRavljen.UnitSystem.Collection
{

    /// <summary>
    /// Capability interface for resource nodes, defining the resource that the node provides.
    /// </summary>
    public interface IResourceNodeCapability : IEntityCapability
    {
        /// <summary>
        /// The resource available in the node, including type and quantity.
        /// </summary>
        ResourceQuantity Resource { get; }

    }

}