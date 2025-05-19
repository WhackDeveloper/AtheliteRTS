
namespace TRavljen.UnitSystem
{
    using System;
    using UnityEngine;

    /// <summary>
    /// Attribute to specify that a serialized field requires a component 
    /// implementing a specific interface or inheriting a specific type.
    /// </summary>
    /// <remarks>
    /// This attribute can be used to ensure that the assigned field in the 
    /// Unity Inspector adheres to a specific type constraint, typically for 
    /// validating MonoBehaviour or ScriptableObject references.
    /// </remarks>
    [AttributeUsage(AttributeTargets.Field)]
    public class RequiresTypeAttribute : PropertyAttribute
    {
        /// <summary>
        /// Gets the required type or interface that the assigned field must implement.
        /// </summary>
        public Type InterfaceType;

        /// <summary>
        /// Initializes a new instance of the <see cref="RequiresTypeAttribute"/> class.
        /// </summary>
        /// <param name="expectedInterfaceType">
        /// The <see cref="Type"/> that the field's assigned value must implement or inherit from.
        /// </param>
        public RequiresTypeAttribute(Type expectedInterfaceType)
        {
            InterfaceType = expectedInterfaceType;
        }
    }


}