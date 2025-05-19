using UnityEngine;

namespace TRavljen.UnitSystem
{

    /// <summary>
    /// Defines destruction refund capability for when a unit is destroyed.
    /// </summary>
    [System.Serializable]
    public struct DestructionRefundCapability : IDestructionRefundCapability
    {

        [SerializeField]
        [Tooltip("Resources refunded to the player when this unit is destroyed.")]
        private ResourceQuantity[] destructionRefund;

        [SerializeField]
        [Tooltip("Determines if the refund applies only when the unit was not operational at the time of destruction.")]
        private bool applyOnlyIfNonOperational;

        /// <inheritdoc/>
        public readonly ResourceQuantity[] DestructionRefund => destructionRefund;

        /// <inheritdoc/>
        public readonly bool ApplyOnlyIfNonOperational => applyOnlyIfNonOperational;

        #region IEntityCapability

        /// <summary>
        /// Configures the entity for destruction refund capability. 
        /// No additional components are needed, as this is managed by the default unit implementation.
        /// </summary>
        readonly void IEntityCapability.ConfigureEntity(IEntity entity)
        {
            // Managed by the default implementation of AUnit -> Unit.
        }

        void IEntityCapability.SetDefaultValues()
        {
            destructionRefund = new ResourceQuantity[0];
            applyOnlyIfNonOperational = true;
        }

        #endregion
    }

}