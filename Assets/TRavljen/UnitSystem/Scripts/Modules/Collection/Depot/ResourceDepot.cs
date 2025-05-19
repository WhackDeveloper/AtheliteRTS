using UnityEngine;

namespace TRavljen.UnitSystem.Collection
{

    using Interactions;

    /// <summary>
    /// Component representing a resource depot, which allows resource collection units 
    /// to deposit gathered resources. Supports filtering by resource type and integrates 
    /// with the player's resource system.
    /// </summary>
    [DisallowMultipleComponent]
    public class ResourceDepot : AEntityComponent, IResourceDepot, IUnitInteractingPosition
    {

        [SerializeField, Tooltip("Defines the drop-off area relative to the depot.")]
        private Bounds dropPoint = new(Vector3.zero, Vector3.one);

#if UNITY_EDITOR
        [SerializeField, Tooltip("Color of the gizmo drawn for the drop-off area in the editor.")]
        private Color dropPointGizmoColor = Color.yellow;
#endif

        private ResourceSO[] supportedResources = new ResourceSO[0];

        public override bool IsActive => Entity.IsOperational;

        /// <summary>
        /// Position of the depot in the world.
        /// </summary>
        public Vector3 Position => transform.position;

        /// <inheritdoc/>
        public ResourceSO[] SupportedResources => supportedResources;

        /// <inheritdoc/>
        protected override void OnInitialize()
        {
            base.OnInitialize();

            if (Entity.TryGetCapability(out IResourceDepotCapability depot))
            {
                supportedResources = depot.SupportedResources;
            }
            else
            {
                Debug.LogError("Entity is missing IResourceDepotCapability: " + name);
            }
        }

        #region IResourceDepot

        /// <inheritdoc/>
        public bool CanDepositResource(ResourceSO resource)
        {
            if (Entity.IsOperational == false) return false;

            if (supportedResources.Length == 0) return true;

            foreach (AProducibleSO supportedResource in supportedResources)
            {
                if (supportedResource.ID == resource.ID) return true;
            }

            return false;
        }

        /// <inheritdoc/>
        public long DepositResources(ResourceQuantity resourceAmount)
        {
            long remainder = Entity.Owner.AddResource(resourceAmount);
            resourceAmount.Quantity = remainder;

            return remainder;
        }

        #endregion

        #region IUnitInteractingPosition

        /// <summary>
        /// Returns closests position around the object based on <see cref="dropPoint"/>
        /// configuration.
        /// </summary>
        public virtual Vector3 GetAvailableInteractionPosition(IUnitInteractorComponent interactor, bool reserve)
        {
            var (position, _) = InteractorPositionHelper.CalculateInteractionPositionAndDirection(interactor, this, GetDropPointBounds());
            return position;
        }

        public virtual bool ReleaseInteractionPosition(IUnitInteractorComponent interactor) => true;

        /// <summary>
        /// Calculates the world-space bounds of the drop-off area.
        /// </summary>
        /// <returns>A <see cref="Bounds"/> object representing the world-space drop-off area.</returns>
        private Bounds GetDropPointBounds()
        {
            Transform transform = this.transform;
            Vector3 center = transform.TransformPoint(dropPoint.center);
            Vector3 size = Vector3.Scale(transform.lossyScale, dropPoint.size);
            return new Bounds(center, size);
        }

        #endregion

#if UNITY_EDITOR
        /// <summary>
        /// Draws the drop-off area as a gizmo in the Unity Editor.
        /// </summary>
        private void OnDrawGizmosSelected()
        {
            Bounds bounds = GetDropPointBounds();
            Gizmos.color = dropPointGizmoColor;
            Gizmos.DrawWireCube(bounds.center, bounds.size);
        }
#endif
    }

}