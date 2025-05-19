namespace TRavljen.UnitSystem.Collection
{

    /// <summary>
    /// Defines the capability of a resource collector, specifying its collection behavior,
    /// range, capacity, and other collection-related attributes.
    /// </summary>
    public interface IResourceCollectorCapability : IEntityCapability
    {
        /// <summary>
        /// The type of collection behavior this collector uses.
        /// </summary>
        CollectType CollectType { get; }

        /// <summary>
        /// The maximum capacity of resources this collector can hold before processing or depositing.
        /// </summary>
        int Capacity { get; }

        /// <summary>
        /// The types of resources that this collector can collect. 
        /// If empty, the collector is not restricted to specific resource types.
        /// </summary>
        ResourceSO[] SupportedResources { get; }

        /// <summary>
        /// The minimum range required to interact with a resource node.
        /// </summary>
        float MinRange { get; }

        /// <summary>
        /// The maximum range within which the collector can interact with resource nodes.
        /// </summary>
        float Range { get; }

        /// <summary>
        /// The interval (in seconds) between collection actions.
        /// </summary>
        float CollectInterval { get; }

        /// <summary>
        /// The amount of resource collected per collection action.
        /// </summary>
        int CollectAmount { get; }
    }

}