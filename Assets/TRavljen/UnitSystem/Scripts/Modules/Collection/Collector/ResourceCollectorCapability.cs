using UnityEngine;

namespace TRavljen.UnitSystem.Collection
{

    /// <summary>
    /// Defines the capability of a resource collector, specifying its attributes
    /// and behavior for collecting resources in the game.
    /// </summary>
    public struct ResourceCollectorCapability : IResourceCollectorCapability
    {

        [Tooltip("Specifies the type of collection behavior the collector uses.")]
        [SerializeField]
        private CollectType collectType;

        [Tooltip("The maximum capacity of resources the collector can hold before depositing or processing.")]
        [SerializeField]
        private int capacity;

        [Tooltip("The interval between collection actions (in seconds). ")]
        [SerializeField, Range(0.05f, 30f)]
        private float collectInterval;

        [Tooltip("The amount of resource collected per action.")]
        [SerializeField]
        private int collectAmount;

        [Tooltip("The minimum range required for interacting with a resource node.")]
        [SerializeField, Range(0.1f, 100f)]
        private float minRange;

        [Tooltip("The maximum range within which the collector can interact with a resource node.")]
        [SerializeField, Range(0.1f, 100f)]
        private float range;

        [Tooltip("The resources this collector can collect. If empty, the collector can collect all resource types.")]
        [SerializeField]
        private ResourceSO[] supportedResources;

        /// <inheritdoc />
        readonly public CollectType CollectType => collectType;

        /// <inheritdoc />
        readonly public int Capacity => capacity;

        /// <inheritdoc />
        readonly public ResourceSO[] SupportedResources => supportedResources;

        /// <inheritdoc />
        readonly public float MinRange => minRange;

        /// <inheritdoc />
        readonly public float Range => range;

        /// <inheritdoc />
        readonly public float CollectInterval => collectInterval;

        /// <inheritdoc />
        readonly public int CollectAmount => collectAmount;

        #region IEntityCapability

        /// <inheritdoc />
        readonly void IEntityCapability.ConfigureEntity(IEntity entity)
        {
            entity.gameObject.AddComponentIfNotPresent<ResourceCollector>();
        }

        /// <inheritdoc />
        void IEntityCapability.SetDefaultValues()
        {
            collectInterval = .1f;
            collectType = CollectType.GatherAndDeposit;
            capacity = 50;
            collectAmount = 10;
            minRange = 0;
            range = 1;

            // Supports all resources by default
            supportedResources = new ResourceSO[0];
        }

        #endregion

    }
}