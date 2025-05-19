using System;

namespace TRavljen.UnitSystem
{

    /// <summary>
    /// Represents an attribute and its associated value.
    /// </summary>
    /// <remarks>
    /// This structure is used to define production-related attributes such as "Population," 
    /// "Unit Limit," or similar, along with their respective values.
    /// <para>
    /// Example use cases:
    /// - Increasing population capacity by 2.
    /// - Defining a unit's attack bonus against specific targets.
    /// - Assigning a fixed value like the health of a unit.
    /// </para>
    /// </remarks>
    [Serializable]
    public struct ProductionAttributeValue
    {
        /// <summary>
        /// Specifies the attribute associated with this value.
        /// </summary>
        public ProductionAttributeSO Attribute;

        /// <summary>
        /// The numeric value of the attribute.
        /// </summary>
        public float Value;
    }


}
