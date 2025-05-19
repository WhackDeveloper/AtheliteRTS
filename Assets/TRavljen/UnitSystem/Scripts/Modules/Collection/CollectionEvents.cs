namespace TRavljen.UnitSystem.Collection
{

    using UnityEngine.Events;

    /// <summary>
    /// Manages events related to resource collection, including resource deposits, 
    /// depletion of resource nodes, and missing resource depots.
    /// Provides a centralized event system for the collection mechanics in the game.
    /// </summary>
    public class CollectionEvents
    {

        /// <summary>
        /// Singleton instance of the <see cref="CollectionEvents"/> class.
        /// Ensures centralized event management.
        /// </summary>
        public static CollectionEvents Instance { get; } = new();

        /// <summary>
        /// Event invoked when a resource collector deposits resources into a depot.
        /// </summary>
        /// <remarks>
        /// Parameters:
        /// - <see cref="IResourceCollector"/>: The collector depositing the resources.
        /// - <see cref="ProducibleQuantity"/>: The quantity of resources deposited.
        /// </remarks>
        public UnityEvent<IResourceCollector, ProducibleQuantity> OnResourceDeposited = new();

        /// <summary>
        /// Event invoked when a resource node is depleted.
        /// </summary>
        /// <remarks>
        /// Parameter:
        /// - <see cref="IResourceNode"/>: The resource node that is depleted.
        /// </remarks>
        public UnityEvent<IResourceNode> OnNodeDepleted = new();

        /// <summary>
        /// Event invoked when collector wants to deposit his resources, but no
        /// depot is found.
        /// </summary>
        /// <remarks>
        /// Parameter:
        /// - <see cref="ResourceCollector"/>: The collector that attempted to deposit resources.
        /// </remarks>
        public UnityEvent<ResourceCollector> OnMissingDepot = new();

    }


}