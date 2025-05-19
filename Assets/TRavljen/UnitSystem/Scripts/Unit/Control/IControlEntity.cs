using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TRavljen.UnitSystem
{

    /// <summary>
    /// Interface for unit's main control which sets the target position of an entity.
    /// Designed to accommodate both movable and non-movable entities, 
    /// such as stationary units with target positions for spawn points or moving units
    /// to a new position controlled by the player.
    /// Control contract is intended to be invoked by player's interactions and
    /// not designed to be used for regular unit movement controls.
    /// </summary>
    public interface IControlEntity
    {
        /// <summary>
        /// Sets the new control position for the unit.
        /// <para>
        /// This could be a moving unit or spawn point that will send their spawned
        /// units to specified position.
        /// </para>
        /// </summary>
        /// <param name="position">The world position to assign as the target.</param>
        void SetControlPosition(Vector3 position);
        
        /// <summary>
        /// Gets the control point of the unit.
        /// </summary>
        Vector3 GetControlPosition();

    }

}