using TRavljen.EditorUtility;
using UnityEngine;
using UnityEngine.Events;

namespace TRavljen.UnitSystem.Collection
{
    using Interactions;

    /// <summary>
    /// Component representing a resource node in the game world, handling resource depletion and interactions.
    /// </summary>
    [DisallowMultipleComponent]
    public class ResourceNode : ALimitedInteractionTarget, IResourceNode, IUnitInteractingPosition
    {

        [SerializeField, Tooltip("Collider representing the physical boundaries of the resource node.")]
        private Collider nodeCollider;

        [SerializeField, Tooltip("The initial resource quantity available in the node. Negative or zero value is considered as unlimited quantity.")]
        [DisableInInspector]
        private ResourceQuantity resource;

        [SerializeField, Tooltip("Event triggered when the resource node is depleted.")]
        private UnityEvent OnNodeDepleted;

        [SerializeField]
        [Tooltip("Specifies if the entity game object is destroyed once depleted. " +
                 "By default, the entity will be destroyed.")]
        private bool destroyOnDeplete = true;

        /// <summary>
        /// Specifies if the resource node has unlimited resources.
        /// This is read from capability on start.
        /// </summary>
        private bool hasUnlimitedResource = false;

        /// <inheritdoc/>
        public ResourceQuantity ResourceAmount => resource;

        /// <inheritdoc/>
        public bool IsDepleted => !hasUnlimitedResource && resource.Quantity <= 0;

        /// <summary>
        /// Position provider reference. This is an optional provider.
        /// </summary>
        public IPredefinedInteractionPositionProvider PredefinedPositionProvider;

        /// <inheritdoc/>
        protected override void OnInitialize()
        {
            base.OnInitialize();

            if (!Entity.TryGetCapability(out IResourceNodeCapability node))
            {
                Debug.LogError("Resource Node requires for it's Entity to have `IResourceNodeCapability`");
                return;
            }

            PredefinedPositionProvider = GetComponent<IPredefinedInteractionPositionProvider>();
            resource = node.Resource;

            // This implementation defines negative or zero value as unlimited.
            hasUnlimitedResource = node.Resource.Quantity <= 0;

            if (hasUnlimitedResource)
                resource.Quantity = 9_999;
        }

        /// <inheritdoc/>
        public void ReduceResource(long amount)
        {
            // Should not deplet resource
            if (hasUnlimitedResource) return;

            resource.Quantity -= amount;

            if (resource.Quantity <= 0)
            {
                CollectionEvents.Instance.OnNodeDepleted.Invoke(this);
                // Invoke component event for easier integration on prefabs.
                OnNodeDepleted.Invoke();

                if (destroyOnDeplete)
                {
                    Entity.DestroyEntity(0);
                }
            }
        }

        #region IUnitInteractingPosition

        public virtual Vector3 GetAvailableInteractionPosition(IUnitInteractorComponent interactor, bool reserve)
        {
            // Check for position provider on unit itself
            if (PredefinedPositionProvider != null)
                return PredefinedPositionProvider.GetAvailableInteractionPosition(interactor, this, reserve);

            // Calculate fit position
            var (position, _) = InteractorPositionHelper.CalculateInteractionPositionAndDirection(interactor, this);
            return position;
        }

        public virtual bool ReleaseInteractionPosition(IUnitInteractorComponent interactor)
        {
            // If position provider is set, remove interaction position
            return PredefinedPositionProvider?.ReleaseInteractionPosition(interactor, this) ?? true;
        }

        #endregion
    }

}