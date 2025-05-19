using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TRavljen.UnitSystem
{

    /// <summary>
    /// A ScriptableObject representing a unit limit attribute. 
    /// This associates a specific unit type with its production or operational limits.
    /// </summary>
    [CreateAssetMenu(fileName = "New unit limit attribute", menuName = "Unit System/Unit Limit Attribute")]
    public class UnitLimitAttributeSO : ProductionAttributeSO
    {
        /// <summary>
        /// The unit type associated with this limit attribute.
        /// </summary>
        public AUnitSO unit;
    }

}