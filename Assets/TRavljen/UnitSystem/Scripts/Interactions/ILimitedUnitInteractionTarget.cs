using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TRavljen.UnitSystem.Interactions
{

    /// <summary>
    /// Defines an interaction target that imposes a limit on the number of units
    /// that can simultaneously interact with it.
    /// Extends the <see cref="IUnitInteracteeComponent"/> interface.
    /// </summary>
    /// <remarks>
    /// This interface is useful for interactions where capacity needs to be managed,
    /// such as units interacting with a structure or resource node that can only handle
    /// a limited number of interactions at a time.
    /// </remarks>
    public interface ILimitedUnitInteractionTarget : IUnitInteracteeComponent
    {
        /// <summary>
        /// Gets the maximum number of units that can interact with this target simultaneously.
        /// </summary>
        /// <remarks>
        /// If the interaction limit is not active (see <see cref="IsLimitActive"/>), this value may not apply.
        /// </remarks>
        int InteractionLimit { get; }

        /// <summary>
        /// Gets the current number of active interactions with this target.
        /// </summary>
        /// <remarks>
        /// Tracks how many units are currently interacting with the target. 
        /// This value should not exceed <see cref="InteractionLimit"/> when the limit is active.
        /// </remarks>
        int ActiveInteractions { get; }
        
        /// <summary>
        /// Gets the number of remaining interaction slots available.
        /// </summary>
        int AvailableInteractions { get; }

        /// <summary>
        /// Determines whether the interaction limit has been reached.
        /// </summary>
        /// <returns>
        /// True if the number of <see cref="ActiveInteractions"/> equals or exceeds
        /// the <see cref="InteractionLimit"/>; otherwise, false.
        /// </returns>
        bool HasReachedLimit();

        /// <summary>
        /// Determines whether the interaction limit is active.
        /// </summary>
        /// <returns>
        /// True if the target enforces an interaction limit; otherwise, false.
        /// </returns>
        /// <remarks>
        /// Some targets may not always enforce interaction limits, allowing unlimited interactions
        /// even if <see cref="InteractionLimit"/> is defined.
        /// </remarks>
        bool IsLimitActive();

        /// <summary>
        /// Attempts to assign the specified unit to interact with this target.
        /// </summary>
        /// <param name="unit">The unit attempting to interact with this target.</param>
        /// <returns>
        /// True if the unit was successfully assigned to interact; otherwise, false,
        /// typically due to the interaction limit being reached.
        /// </returns>
        /// <remarks>
        /// This method is used to register a new interaction with the target,
        /// incrementing <see cref="ActiveInteractions"/> if successful.
        /// </remarks>
        bool Assign(IUnit unit);

        /// <summary>
        /// Attempts to unassign the specified unit from interacting with this target.
        /// </summary>
        /// <param name="unit">The unit to unassign from interacting with this target.</param>
        /// <returns>
        /// True if the unit was successfully unassigned; otherwise, false.
        /// </returns>
        /// <remarks>
        /// This method is used to deregister an interaction with the target,
        /// decrementing <see cref="ActiveInteractions"/> if successful.
        /// </remarks>
        bool Unassign(IUnit unit);
    }

}