namespace TRavljen.UnitSystem
{

    /// <summary>
    /// Defines the refund capability when a unit is destroyed.
    /// </summary>
    public interface IDestructionRefundCapability : IEntityCapability
    {
        /// <summary>
        /// Gets the resources refunded when the unit is destroyed.
        /// </summary>
        ResourceQuantity[] DestructionRefund { get; }

        /// <summary>
        /// Determines if the refund applies only when the unit was not operational at the time of destruction.
        /// </summary>
        bool ApplyOnlyIfNonOperational { get; }
    }

}