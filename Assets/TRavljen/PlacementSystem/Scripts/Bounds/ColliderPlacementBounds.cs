using UnityEngine;

namespace TRavljen.PlacementSystem
{
    using Utility;

    /// <summary>
    /// Defines placement bounds with use of <see cref="BoxCollider"/>. When placing
    /// an object should consider bounds of a box collider, use this component.
    /// It is using manual calculation of the collider bounds because when collider
    /// is disabled (for any reason) those bounds (collder.bounds) will be invalid
    /// and cannot be used for placement bounds.
    /// </summary>
    public class ColliderPlacementBounds : APlacementBounds
    {

        [Tooltip("Specifies the collider for defining object's placement bounds.")]
        [SerializeField]
        private BoxCollider placementCollider;

        [SerializeField, HideInInspector]
        private Bounds bounds;

        public override Bounds PlacementBounds => new(placementCollider.transform.TransformPoint(bounds.center), bounds.size);
        public override Quaternion PlacementRotation => placementCollider.transform.rotation;

        private void OnValidate()
        {
            UpdateBounds();
        }

        private void UpdateBounds()
        {
            if (placementCollider)
                bounds = ColliderBoundsHelper.CalculateBounds(placementCollider);
        }

        public void SetCollider(BoxCollider collider)
        {
            placementCollider = collider;
            UpdateBounds();
        }

    }
}