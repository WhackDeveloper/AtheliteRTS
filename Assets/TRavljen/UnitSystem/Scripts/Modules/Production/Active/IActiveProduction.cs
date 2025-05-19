namespace TRavljen.UnitSystem
{

    /// <summary>
    /// Defines the behavior for unit production in a system.  
    /// Extends <see cref="IProduce"/> to provide methods and properties for managing,  
    /// tracking, and delegating production processes.
    /// </summary>
    public interface IActiveProduction : IEntityComponent, IProduce
    {
        /// <summary>
        /// Gets or sets the delegate responsible for managing additional constraints  
        /// or conditions during production.
        /// </summary>
        IActiveProductionDelegate Delegate { get; set; }

        /// <summary>
        /// Gets the progress of the current production as a percentage,  
        /// ranging from 0.0 (not started) to 1.0 (completed).
        /// </summary>
        float CurrentProductionProgress { get; }

        /// <summary>
        /// Checks if the specified producible item is currently being produced.
        /// </summary>
        /// <param name="producible">The producible item to check.</param>
        /// <returns>True if the item is being produced; otherwise, false.</returns>
        bool IsProducing(AProducibleSO producible);

        /// <summary>
        /// Cancels a specific production order for the given producible item.
        /// </summary>
        /// <param name="producible">The producible item to cancel.</param>
        /// <returns>Returns amount of producibles in the cancelled order.</returns>
        long CancelProductionOrder(AProducibleSO producible);

        /// <summary>
        /// Cancels all production orders and refunds any associated resources.
        /// </summary>
        /// <returns>An array of <see cref="ResourceQuantity"/> representing the refunded resources.</returns>
        ResourceQuantity[] CancelProduction();

        /// <summary>
        /// Starts the production process based on a provided productionQuantity.
        /// </summary>
        /// <param name="productionQuantity">The production quantity to execute.</param>
        /// <param name="queueMultipleOrders">If true, adds multiple orders to the production queue.</param>
        void StartProduction(ProducibleQuantity productionQuantity, bool queueMultipleOrders = false);

        /// <summary>
        /// Starts the production process for a specific producible item and quantity.
        /// </summary>
        /// <param name="producible">The producible item to produce.</param>
        /// <param name="quantity">The quantity of the item to produce.</param>
        /// <param name="queueMultipleOrders">If true, adds multiple orders to the production queue.</param>
        void StartProduction(AProducibleSO producible, long quantity, bool queueMultipleOrders = false);

        /// <summary>
        /// Retrieves the current production queue, including the producible items and their quantities.
        /// </summary>
        /// <returns>An array of <see cref="ProducibleQuantity"/> representing the production queue.</returns>
        ProducibleQuantity[] GetProductionQueue();
    }

}