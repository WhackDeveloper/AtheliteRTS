using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TRavljen.UnitSelection
{

    /// <summary>
    /// This can be implemented by any component attached to the GameObject
    /// to specify a custom selection bounds. 
    /// </summary>
    public interface ISelectionBounds
    {
        /// <summary>
        /// Returns custom selection bounds.
        /// </summary>
        public Bounds SelectionBounds { get; }
    }

}