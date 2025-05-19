using System;
using UnityEngine;

namespace TRavljen.UnitSystem
{

    /// <summary>
    /// Representation of producible quantities with <see cref="float"/>.
    /// </summary>
    [Serializable]
    public struct ProducibleQuantity
    {

        [Tooltip("Specifies the producible.")]
        /// <summary>
        /// Specifies the producible.
        /// </summary>
        public AProducibleSO Producible;

        [Tooltip("Specifies the quantity of the producible.")]
        [PositiveLong]
        /// <summary>
        /// Specifies quantity of the producible.
        /// </summary>
        public long Quantity;

        public ProducibleQuantity(AProducibleSO producible, long quantity)
        {
            Producible = producible;
            Quantity = quantity;
        }

    }

}