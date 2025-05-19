using UnityEngine;

namespace TRavljen.PlacementSystem
{

    /// <summary>
    /// Abstract <see cref="MonoBehaviour"/> for implementing <see cref="IPlacementBounds"/>.
    /// Subclass this component to achieve custom bounds behaviour.
    /// </summary>
    public abstract class APlacementBounds : MonoBehaviour, IPlacementBounds
    {

        [SerializeField]
        private bool drawGizmo = false;

        public abstract Bounds PlacementBounds { get; }
        public abstract Quaternion PlacementRotation { get; }

        private void OnDrawGizmos()
        {
            if (drawGizmo)
            {
                Gizmos.color = Color.blue;
                Bounds bounds = PlacementBounds;
                Gizmos.matrix = Matrix4x4.TRS(bounds.center, PlacementRotation, bounds.size);
                Gizmos.DrawWireCube(Vector3.zero, Vector3.one);
            }
        }

    }

}