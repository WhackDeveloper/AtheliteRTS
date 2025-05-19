using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TRavljen.UnitSystem.Interactions
{

    /// <summary>
    /// Entity component used to enforce limitations on concurrent interactions.
    /// </summary>
    /// <remarks>
    /// This class provides functionality for managing interaction limits on an entity, such as a resource node or structure.
    /// Units can be assigned or unassigned from interacting with the entity, with enforced limits if configured.
    /// </remarks>
    [System.Serializable]
    public abstract class ALimitedInteractionTarget : AEntityComponent, ILimitedUnitInteractionTarget
    {

        [Tooltip("Determines whether concurrent interaction limits are active for this target.")]
        [SerializeField]
        private bool limitConcurrentInteractions = false;

        [Tooltip("Specifies the maximum number of units that can interact with this target simultaneously.")]
        [SerializeField, Range(0, 100)]
        protected int maxConcurrentInteractions = 0;

        /// <summary>
        /// Tracks the units currently assigned to interact with this target.
        /// </summary>
        public readonly HashSet<IUnit> TargetingEntities = new();

        /// <inheritdoc/>
        public int InteractionLimit => maxConcurrentInteractions;

        /// <inheritdoc/>
        public int ActiveInteractions => TargetingEntities.Count;

        /// <inheritdoc/>
        public int AvailableInteractions => maxConcurrentInteractions - ActiveInteractions;

        /// <summary>
        /// Gets the position of this interaction target in the world.
        /// </summary>
        public Vector3 Position => transform.position;

        /// <inheritdoc/>
        public bool IsLimitActive() => limitConcurrentInteractions;

        /// <inheritdoc/>
        public bool HasReachedLimit()
        {
            if (limitConcurrentInteractions)
                return maxConcurrentInteractions > 0 && TargetingEntities.Count >= maxConcurrentInteractions;
            return false;
        }

        /// <summary>
        /// Resets the target, clearing all active interactions.
        /// </summary>
        public void Reset()
        {
            TargetingEntities.Clear();
        }

        /// <summary>
        /// Assigns a unit to interact with this target, provided the interaction limit has not been reached.
        /// </summary>
        /// <param name="unit">The unit attempting to interact with this target.</param>
        /// <returns>
        /// Returns true if the unit was successfully assigned; otherwise, false, typically due to the interaction limit being reached
        /// or the unit already being assigned.
        /// </returns>
        /// <remarks>
        /// This method adds the unit to the <see cref="TargetingEntities"/> set and enforces the interaction limit if active.
        /// </remarks>
        bool ILimitedUnitInteractionTarget.Assign(IUnit unit)
        {
            if (!limitConcurrentInteractions) return true;

            if (!TargetingEntities.Contains(unit))
            {
                if (HasReachedLimit())
                    // Reached max
                    return false;

                TargetingEntities.Add(unit);
                // All good
                return true;
            }

            // Already on the job, should not try to re-assign
            return false;
        }

        /// <summary>
        /// Unassigns a unit from interacting with this target.
        /// </summary>
        /// <param name="unit">The unit to unassign from interacting with this target.</param>
        /// <returns>
        /// Returns true if the unit was successfully unassigned; otherwise, false, typically if the unit was not assigned.
        /// </returns>
        /// <remarks>
        /// This method removes the unit from the <see cref="TargetingEntities"/> set.
        /// </remarks>
        bool ILimitedUnitInteractionTarget.Unassign(IUnit unit)
        {
            return TargetingEntities.Remove(unit);
        }

    }

}