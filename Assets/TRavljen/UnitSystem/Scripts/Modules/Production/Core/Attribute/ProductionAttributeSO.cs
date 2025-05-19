using UnityEngine;

namespace TRavljen.UnitSystem
{

    /// <summary>
    /// Defines a base production attribute, such as "Population" or "Unit Limit."
    /// </summary>
    /// <remarks>
    /// Attributes are primarily used in production systems to define rules, 
    /// parameters, and gameplay logic.
    /// <para>
    /// Attributes can represent various gameplay mechanics, including:
    /// - Population capacity for a player.
    /// - Production speed modifiers.
    /// - Resource limits.
    /// </para>
    /// </remarks>
    [CreateAssetMenu(fileName = "New attribute", menuName = "Unit System/Attribute")]
    public class ProductionAttributeSO : ManagedSO
    {
        /// <summary>
        /// The name of the attribute (e.g., "Population" or "Unit Limit").
        /// </summary>
        public string Name;

        /// <summary>
        /// A description of the attribute, generally visible to the player.
        /// </summary>
        [TextArea(2, 5)]
        public string Description;
    }

}
