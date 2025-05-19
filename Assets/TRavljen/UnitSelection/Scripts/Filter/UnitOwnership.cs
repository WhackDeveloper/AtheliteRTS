using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TRavljen.UnitSelection
{

    /// <summary>
    /// Specifies the ownership state of the unit.
    /// </summary>
    public enum UnitOwnershipState
    {
        Player,
        Ally,
        Neutral,
        Enemy
    }

    /// <summary>
    /// Component that marks a unit ownership state.
    /// Used by selection filters to determine ownership.
    /// </summary>
    public class UnitOwnership : MonoBehaviour
    {
        [Tooltip("Specifies the unit ownership state.\n" +
                 "Used for selection filtering.")]
        public UnitOwnershipState State = UnitOwnershipState.Neutral;
    }

}