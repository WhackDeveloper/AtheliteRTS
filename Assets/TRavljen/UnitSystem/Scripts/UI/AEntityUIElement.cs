using UnityEngine;

namespace TRavljen.UnitSystem
{

    /// <summary>
    /// Represents a base class for all UI elements related to an entity.
    /// </summary>
    public abstract class AEntityUIElement : MonoBehaviour
    {
        /// <summary>
        /// Configures the UI element with the provided entity data.
        /// </summary>
        /// <param name="entity">The entity whose information will be displayed.</param>
        public abstract void Configure(IEntity entity);
    }
}