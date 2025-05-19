using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace TRavljen.UnitSelection
{

    /// <summary>
    /// Custom selection bounds by using specific collider. This can be useful
    /// when unit is constructed from multiple renderers and collider might
    /// be a better fit. And there is always <see cref="CustomSelectionBounds"/> option.
    /// </summary>
    public class ColliderSelectionBounds : MonoBehaviour, ISelectionBounds
    {
        [SerializeField]
        [Tooltip("Assign a collider for custom bounds.")]
        private Collider selectionCollider;

        public Bounds SelectionBounds => selectionCollider.bounds;
    }


}