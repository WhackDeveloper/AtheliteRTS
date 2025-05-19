using TRavljen.UnitSystem.Interactions;

namespace TRavljen.UnitSystem.Collection
{

    /// <summary>
    /// Interface for resource nodes, defining behaviors and properties for resource interactions.
    /// </summary>
    public interface IResourceNode : IUnitInteracteeComponent
    {

        /// <summary>
        /// The current resource available in the node, including type and quantity.
        /// </summary>
        ResourceQuantity ResourceAmount { get; }

        /// <summary>
        /// Indicates whether the node is depleted (i.e., no more resources available).
        /// </summary>
        bool IsDepleted { get; }

        /// <summary>
        /// Reduces the available resource in the node by the specified quantity.
        /// </summary>
        /// <param name="quantity">The amount of resource to reduce.</param>
        void ReduceResource(long quantity);
    }

}