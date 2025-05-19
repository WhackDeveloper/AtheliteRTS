using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TRavljen.UnitSystem
{

    /// <summary>
    /// Represents a unit type and its associated limit.
    /// </summary>
    [System.Serializable]
    public struct UnitLimit
    {
        [Tooltip("The unit type for which the limit applies.")]
        public AUnitSO unit;

        [Tooltip("The maximum allowable count for the specified unit type.")]
        [PositiveInt]
        public int limit;
    }

}