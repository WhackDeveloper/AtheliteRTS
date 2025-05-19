using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TRavljen.UnitSelection
{

    /// <summary>
    /// When using custom selection detection types it can be useful to use
    /// something that might not rely on colliders or renderers.
    /// In such cases you can use this component to define specific bounds
    /// by configuring the <see cref="size"/> and <see cref="offset"/>.
    /// </summary>
    public sealed class CustomSelectionBounds : MonoBehaviour, ISelectionBounds
    {
        [SerializeField]
        [Tooltip("Assign custom size for the selection")]
        private Vector3 size = Vector3.one;

        [SerializeField]
        [Tooltip("Assign custom size for the selection")]
        private Vector3 offset;

        [SerializeField]
        private bool drawGizmo = false;

        private Transform _transform;

        public Bounds SelectionBounds => new Bounds(_transform.position + _transform.rotation * offset, size);

        private void Awake() => _transform = transform;

        private void OnDrawGizmos()
        {
            if (drawGizmo)
            {
                // Keep in mind that AABB tests do not include rotation.
                Bounds bounds = new Bounds(transform.position + offset, size);
                Gizmos.matrix = Matrix4x4.TRS(bounds.center, transform.rotation, bounds.size);
                Gizmos.DrawWireCube(Vector3.zero, Vector3.one);
            }
        }
    }

}