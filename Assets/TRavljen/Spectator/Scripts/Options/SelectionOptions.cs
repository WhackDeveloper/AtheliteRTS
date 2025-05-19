using System;
using UnityEngine;

namespace Spectator
{

    [Serializable]
    public struct SelectionOptions
    {

        /// <summary>
        /// Specifies if selection by clicking an object is enabled.
        /// If this is enabled <see cref="LayerMask"/> mask will be
        /// used to detect any clickable objects where the cursor is.
        /// </summary>
        public bool Enabled;

        /// <summary>
        /// Specifies if clicking twice on the same object triggers
        /// object centering.
        /// </summary>
        public bool DoubleClickEnabled;

        /// <summary>
        /// Specifies maximal detection distance for selection click.
        /// </summary>
        public float MaxSelectDistance;

        /// <summary>
        /// Specifies selectable layers for selection click.
        /// </summary>
        public LayerMask LayerMask;

    }

}