using UnityEngine;

namespace TRavljen.UnitSystem.Interactions
{

    /// <summary>
    /// Defines an object that can be interacted with by an <see cref="IUnitInteractorComponent"/>.
    /// </summary>
    public interface IUnitInteracteeComponent: IEntityComponent
    {
        /// <summary>
        /// Current position of the interactee.
        /// </summary>
        public Vector3 Position { get; }
    }

}
