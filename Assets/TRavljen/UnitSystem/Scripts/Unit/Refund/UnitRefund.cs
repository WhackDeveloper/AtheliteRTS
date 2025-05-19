using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TRavljen.UnitSystem
{

    public static class UnitRefund
    {
        /// <summary>
        /// Calculates and returns the resources to be refunded based on the unit's state.
        /// </summary>
        /// <returns>The resources to be refunded.</returns>
        public static ResourceQuantity[] GetRefundResources(this IUnit unit)
        {
            bool isOperational = unit.IsOperational;
            if (unit.Data.TryGetCapability(out IDestructionRefundCapability refund))
            {
                // Check if the refund should apply based on the unit's operational state
                if (refund.ApplyOnlyIfNonOperational && isOperational)
                    return ResourceQuantity.EmptyArray;

                return refund.DestructionRefund;
            }

            // Default refund logic: full cost if not operational, otherwise no refund
            return !isOperational ? unit.Data.Cost : ResourceQuantity.EmptyArray;
        }
    }

}
