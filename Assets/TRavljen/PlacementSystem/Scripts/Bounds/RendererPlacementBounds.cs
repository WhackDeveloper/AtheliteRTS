using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TRavljen.PlacementSystem
{

    /// <summary>
    /// Defines placement bounds with use of <see cref="Renderer"/>. When placing
    /// an object should consider bounds of a renderer, use this component.
    /// </summary>
    public class RendererPlacementBounds : APlacementBounds
    {
        [Tooltip("Specifies the renderer used to extract bounds from")]
        [SerializeField]
        private Renderer _renderer;

        public override Bounds PlacementBounds => _renderer.bounds;
        public override Quaternion PlacementRotation => _renderer.transform.rotation;

        /// <summary>
        /// Sets new renderer for placement bounds. Null should not be passed,
        /// instead disable or destroy this component.
        /// </summary>
        /// <param name="renderer">New renderer</param>
        public void SetRenderer(Renderer renderer)
        {
            _renderer = renderer;
        }
   }

}