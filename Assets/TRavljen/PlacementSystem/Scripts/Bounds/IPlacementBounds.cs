using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TRavljen.PlacementSystem
{

    /// <summary>
    /// Interface for providing placement bounds for a placing object.
    /// </summary>
    public interface IPlacementBounds
    {
        /// <summary>
        /// Root game object of the placing object.
        /// </summary>
        public GameObject gameObject { get; }

        /// <summary>
        /// Bounds relative to local position of the root <see cref="gameObject"/>.
        /// </summary>
        public Bounds PlacementBounds { get; }

        /// <summary>
        /// Absolute rotation of the placing object.
        /// </summary>
        public Quaternion PlacementRotation { get; }
    }

}