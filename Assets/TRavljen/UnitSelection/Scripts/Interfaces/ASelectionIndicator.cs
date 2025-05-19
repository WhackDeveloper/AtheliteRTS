using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TRavljen.UnitSelection
{

    /// <summary>
    /// Abstraction for selection indicator which can be implemented in various ways.
    /// Here are some already available to you.
    /// <see cref="MeshRendererSelectionIndicator"/>,
    /// <see cref="SpriteRendererSelectionIndicator"/>,
    /// <see cref="GameObjectSelectionIndicator"/>
    /// </summary>
    public abstract class ASelectionIndicator : MonoBehaviour
    {
        /// <summary>
        /// Invoked when unit is selected.
        /// </summary>
        public abstract void Select();

        /// <summary>
        /// Invoked when unit is highlighted.
        /// </summary>
        public abstract void Highlight();

        /// <summary>
        /// Invoked when unit is cleared from either selection or highlight.
        /// </summary>
        public abstract void Clear();
    }
}