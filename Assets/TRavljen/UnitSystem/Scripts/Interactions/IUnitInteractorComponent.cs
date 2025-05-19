using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TRavljen.UnitSystem.Interactions
{

    /// <summary>
    /// Defines a unit capable of initiating interactions with an <see cref="IUnitInteracteeComponent"/>.
    /// </summary>
    /// <remarks>
    /// Interactors are typically units in a game, such as characters or entities,
    /// that can interact with objects, structures, or other units.
    /// </remarks>
    public interface IUnitInteractorComponent: IUnitComponent
    {
        /// <summary>
        /// Gets the minimum range at which the unit can interact with an <see cref="IUnitInteracteeComponent"/>.
        /// </summary>
        float MinInteractionRange { get; }

        /// <summary>
        /// Gets the maximum range within which the unit can interact with an <see cref="IUnitInteracteeComponent"/>.
        /// </summary>
        float MaxInteractionRange { get; }

        /// <summary>
        /// Gets the current position of the unit in world space.
        /// </summary>
        /// <remarks>
        /// This property is typically used to calculate distances to interactable objects
        /// and validate whether the unit can initiate an interaction based on its range.
        /// </remarks>
        Vector3 Position { get; }
    }

}
