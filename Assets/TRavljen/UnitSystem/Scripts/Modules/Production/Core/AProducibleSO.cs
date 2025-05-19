using UnityEngine;

namespace TRavljen.UnitSystem
{

    /// <summary>
    /// Base producible object, primarily used to define producibles for
    /// the project. Any gameObject/unit should use this class or derive from
    /// this class to utilize <see cref="UnitSystem"/> components.
    /// <para>
    /// All producibles should originate from this scriptable object and should
    /// primarily differentiate between others using the ID.
    /// You may either subclass this to define new types (e.g., structures) or
    /// add a new field like `Type` with an enum to differentiate
    /// between such units (e.g., workers vs. structures).
    /// </para>
    /// </summary>
    public abstract class AProducibleSO : ManagedSO
    {

        /// <summary>
        /// Specifies the name of the producible.
        /// </summary>
        [Header("General")]
        public string Name;

        /// <summary>
        /// Specifies the description of the producible.
        /// </summary>
        [TextArea(1, 5)]
        public string Description;

        /// <summary>
        /// Specifies the sprite of the producible.
        /// </summary>
        public Sprite Sprite;

        /// <summary>
        /// Specifies production attributes of the producible.
        /// </summary>
        public ProductionAttributeValue[] ProductionAttributes = new ProductionAttributeValue[0];

        /// <summary>
        /// Specifies the producible's requirements, other than cost.
        /// These should not be consumed before production starts.
        /// </summary>
        public ProducibleQuantity[] Requirements = new ProducibleQuantity[0];

        /// <summary>
        /// Specifies the resource requirements to produce this producible. These will be
        /// consumed before the production starts.
        /// </summary>
        public ResourceQuantity[] Cost = new ResourceQuantity[0];

        /// <summary>
        /// Specifies the time required to produce this producible (examples:
        /// training or construction). Values allowed should be positive
        /// numbers. The maximum in the inspector range is 99,999, but this can be
        /// increased here if needed.
        /// </summary>
        [PositiveFloat]
        public float ProductionDuration;

        /// <summary>
        /// Searches for a valid attribute with a value on this producible.
        /// </summary>
        /// <param name="attributeID">ID of the attribute.</param>
        /// <param name="value">Value returned if found.</param>
        /// <returns>
        /// Returns <c>true</c> if the attribute was found; otherwise, <c>false</c>.
        /// </returns>
        public bool TryGetAttribute(int attributeID, out ProductionAttributeValue value)
        {
            foreach (ProductionAttributeValue attribute in ProductionAttributes)
            {
                if (attribute.Attribute.ID == attributeID)
                {
                    value = attribute;
                    return true;
                }
            }

            value = default;
            return false;
        }

    }

}