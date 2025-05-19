using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TRavljen.UnitSelection
{

    /// <summary>
    /// Custom selection bounds by using specific renderer. When unit is constructed
    /// from multiple renderers and you need to use a specific one (for main/large mesh),
    /// then you can use this component and assign the <see cref="selectionRenderer"/>.
    /// </summary>
    public sealed class RendererSelectionBounds : MonoBehaviour, ISelectionBounds
    {

        [SerializeField]
        [Tooltip("Assign a renderer for custom bounds.")]
        private Renderer selectionRenderer;

        public Bounds SelectionBounds => selectionRenderer.bounds;
    
    }

}