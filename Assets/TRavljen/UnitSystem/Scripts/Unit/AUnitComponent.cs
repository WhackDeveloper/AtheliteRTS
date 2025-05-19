using UnityEngine;

namespace TRavljen.UnitSystem
{

    /// <summary>
    /// Abstract base class for components associated with units.
    /// </summary>
    /// <remarks>
    /// This class provides common functionality and a reference to the <see cref="Unit"/> entity 
    /// associated with the component. It extends <see cref="AEntityComponent"/> and implements 
    /// <see cref="IUnitComponent"/>.
    /// </remarks>
    [System.Serializable]
    public abstract class AUnitComponent : AEntityComponent, IUnitComponent
    {

        /// <summary>
        /// Reference to the unit associated with this component.
        /// </summary>
        protected Unit unit;

        /// <inheritdoc/>
        public Unit Unit => unit;

        /// <summary>
        /// Initializes the component and sets up the unit reference.
        /// </summary>
        /// <remarks>
        /// Ensures that the entity is of type <see cref="Unit"/>. If not, 
        /// logs an error indicating an invalid usage of this component.
        /// </remarks>
        protected override void OnInitialize()
        {
            base.OnInitialize();

            if (Entity is Unit unit)
            {
                this.unit = unit;
            }
            else
            {
                Debug.LogError("AUnitComponent was used on a game object that does not have a Unit entity!");
            }
        }
    }

}