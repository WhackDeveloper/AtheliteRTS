using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TRavljen.UnitSystem.Demo
{
    using Combat;

    /// <summary>
    /// Represents a visual element for displaying damage dealt to a target.
    /// </summary>
    /// <remarks>
    /// This interface is designed to be implemented by visual components, 
    /// such as floating damage text or other effects, which visually represent 
    /// damage events in the game. It also provides a mechanism to determine 
    /// when the visual element should be hidden or returned to a pool.
    /// </remarks>
    public interface IDamageVisualElement
    {

        /// <summary>
        /// Game Object holding the visual element.
        /// </summary>
        public GameObject gameObject { get; }
        
        /// <summary>
        /// Players camera used for element positioning.
        /// </summary>
        public Camera PlayerCamera { set; }

        /// <summary>
        /// Displays the damage value visually, associated with the specified target.
        /// </summary>
        /// <param name="health">The health component of the target receiving damage.</param>
        /// <param name="damage">The amount of damage to display.</param>
        void ShowDamage(IHealth health, int damage);

        /// <summary>
        /// Determines whether the visual element should be removed from display and reused.
        /// </summary>
        /// <returns>
        /// <c>true</c> if the element should be returned to a pool; otherwise, <c>false</c>.
        /// </returns>
        bool IsReadyForPooling();
    }

}