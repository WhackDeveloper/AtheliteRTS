namespace TRavljen.UnitSystem
{

    /// <summary>
    /// Defines passive resource production capability, used for constant
    /// and passive resource production without manually queuing them.
    /// </summary>
    public interface IPassiveResourceProductionCapability : IEntityCapability
    {
        /// <summary>
        /// Resources this unit produces and their quantity per second.
        /// </summary>
        public ResourceQuantity[] ProducesResource { get; }

        /// <summary>
        /// Resource quantity at which the produced resource will be deposited
        /// to the players storage.
        /// </summary>
        public long DepositResourceQuantity { get; }
    }

}