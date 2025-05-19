using UnityEngine;

namespace TRavljen.UnitSystem
{

    public struct PassiveResourceProductionCapability : IPassiveResourceProductionCapability
    {

        [Tooltip("Resources this unit produces and their quantity per second")]
        [SerializeField] private ResourceQuantity[] producesResource;

        [Tooltip("Specifies the resource amount at which it should be deposited to " +
            "the player's resources storage.")]
        [SerializeField] private long depositResourceQuantity;

        /// <summary>
        /// Specifies the resources this unit produces per game defined period.
        /// Multiple resources are supported.
        /// </summary>
        public readonly ResourceQuantity[] ProducesResource => producesResource;

        /// <summary>
        /// Specifies the resource amount at which it should be deposited to
        /// the player's resources storage.
        /// </summary>
        public readonly long DepositResourceQuantity => depositResourceQuantity;

        #region IEntityCapability

        readonly void IEntityCapability.ConfigureEntity(IEntity entity)
        {
            entity.gameObject.AddComponentIfNotPresent<PassiveResourceProduction>();
        }

        void IEntityCapability.SetDefaultValues()
        {
            producesResource = null;
            depositResourceQuantity = 10;
        }

        #endregion
    }

}