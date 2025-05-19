using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TRavljen.UnitSystem
{

    /// <summary>
    /// Represents a unit type that can be assigned to one or more <see cref="AUnitSO"/>.
    /// Unit types are used to define classifications and rules for units, such as permissions 
    /// for specific features or interactions.
    /// <para>
    /// For example, unit types can restrict which units can garrison in a <see cref="Garrison.GarrisonEntity"/> 
    /// or define unit behavior based on their type.
    /// </para>
    /// </summary>
    /// <example>
    /// Examples of unit types include:
    /// <list type="bullet">
    /// <item><description>"Non-Military" for civilian units like workers or resource collectors.</description></item>
    /// <item><description>"Structure" for buildings that cannot participate in combat.</description></item>
    /// </list>
    /// </example>
    [CreateAssetMenu(fileName = "New Unit Type", menuName = "Unit System/Unit Type")]
    public class UnitTypeSO : ManagedSO
    {

        [SerializeField, Tooltip("The name of the unit type.")]
        private string typeName;

        [SerializeField, TextArea(1, 3), Tooltip("A short description of the unit type.")]
        private string description;

        /// <summary>
        /// Gets the name of the unit type.
        /// </summary>
        public string TypeName => typeName;

        /// <summary>
        /// Gets a brief description of the unit type.
        /// </summary>
        public string Description => description;
        
        public static bool DoesMatchAnyType(UnitTypeSO[] typesA, UnitTypeSO[] typesB)
        {
            foreach (var typeA in typesA)
            {
                foreach (var typeB in typesB)
                {
                    if (typeA == typeB)
                        return true;
                }
            }

            return false;
        }

    }

}