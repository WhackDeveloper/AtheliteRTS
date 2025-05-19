using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace TRavljen.UnitSystem.Garrison
{

    /// <summary>
    /// Manages garrisonable units and garrison structures for a player.
    /// Tracks all available garrisons and units that can enter them,
    /// providing functions to manage garrison interactions.
    /// </summary>
    [DisallowMultipleComponent]
    public class GarrisonModule : APlayerModule
    {

        #region Properties

        /// <summary>
        /// List of all garrison units owned by the player.
        /// </summary>
        private readonly List<IGarrisonEntity> garrisons = new();

        /// <summary>
        /// List of all garrisonable units owned by the player.
        /// </summary>
        private readonly List<IGarrisonableUnit> garrisonables = new();

        /// <summary>
        /// Gets an array of all garrison units currently managed by this module.
        /// </summary>
        public IGarrisonEntity[] GarrisonUnits => garrisons.ToArray();

        /// <summary>
        /// Gets an array of all garrisonable units currently managed by this module.
        /// </summary>
        public IGarrisonableUnit[] GarrisonableUnits => garrisonables.ToArray();

        /// <summary>
        /// Event triggered when the list of garrisons changes (addition/removal).
        /// </summary>
        public UnityEvent OnGarrisonListChanged = new();

        /// <summary>
        /// Event triggered when the list of garrisonable units changes (addition/removal).
        /// </summary>
        public UnityEvent OnGarrisonableListChanged = new();

        #endregion

        #region Lifecycle

        private void OnEnable()
        {
            player.OnUnitAdded.AddListener(HandleUnitAdded);
            player.OnUnitRemoved.AddListener(HandleUnitRemoved);
        }

        public void OnDisable()
        {
            player.OnUnitAdded.RemoveListener(HandleUnitAdded);
            player.OnUnitRemoved.RemoveListener(HandleUnitRemoved);
        }

        #endregion

        #region Events

        private void HandleUnitAdded(IUnit unit)
        {
            if (unit.Garrison != null)
            {
                garrisons.Add(unit.Garrison);
                OnGarrisonListChanged.Invoke();
            }

            if (unit.GarrisonableUnit != null)
            {
                garrisonables.Add(unit.GarrisonableUnit);
                OnGarrisonableListChanged.Invoke();
            }
        }

        private void HandleUnitRemoved(IUnit unit)
        {
            if (unit.Garrison != null)
            {
                garrisons.Remove(unit.Garrison);
                OnGarrisonListChanged.Invoke();
            }

            if (unit.GarrisonableUnit != null)
            {
                garrisonables.Remove(unit.GarrisonableUnit);
                OnGarrisonableListChanged.Invoke();
            }
        }

        #endregion

        #region Public Interface

        /// <summary>
        /// Releases all units currently inside the specified garrison.
        /// </summary>
        /// <param name="garrison">The garrison to be emptied.</param>
        public void ReleaseGarrisonedUnits(IGarrisonEntity garrison)
        {
            garrison.RemoveAllUnits();
        }

        /// <summary>
        /// Empties all garrisons owned by the player, releasing their units.
        /// </summary>
        public void EmptyAllGarrisons()
        {
            foreach (var garrison in garrisons)
            {
                garrison.RemoveAllUnits();
            }
        }

        /// <summary>
        /// Calls all garrisons to bring in nearby garrisonable units within a specified range.
        /// </summary>
        /// <param name="range">The maximum distance from the garrison to consider units.</param>
        /// <param name="cancelActiveTasks">Whether to cancel active tasks before garrisoning units.
        /// Otherwise those with active tasks are ignored.</param>
        public virtual void CallInAllGarrisons(float range, bool cancelActiveTasks)
        {
            foreach (var garrison in garrisons)
            {
                CallInNearbyUnits(garrison, range, cancelActiveTasks);
            }
        }

        /// <summary>
        /// Calls in nearby garrisonable units to enter a specific garrison.
        /// </summary>
        /// <param name="garrison">The target garrison.</param>
        /// <param name="range">The maximum distance from the garrison to consider units.</param>
        /// <param name="cancelActiveTasks">Whether to cancel active tasks before garrisoning units.
        /// Otherwise those with active tasks are ignored.</param>
        public virtual void CallInNearbyUnits(IGarrisonEntity garrison, float range, bool cancelActiveTasks)
        {
            List<(IGarrisonableUnit garrisonable, float distance)> inRange = new();
            foreach (var g in garrisonables)
            {
                float distance = Vector3.Distance(garrison.Position, g.Position);

                if (distance <= range)
                    inRange.Add((g, distance));
            }

            // Sort those in range
            inRange.Sort((a, b) => a.distance.CompareTo(b.distance));

            int toGarrison = garrison.GarrisonCapacity - garrison.GarrisonedUnits.Count;
            for (int index = 0; index < inRange.Count; index++)
            {
                // Check if garrison can take more units
                if (toGarrison == 0) break;

                toGarrison--;

                IGarrisonableUnit g = inRange[index].garrisonable;
                // Check if we need to cancel active tasks or ignore those with one
                if (cancelActiveTasks)
                {
                    g.Unit.RemoveAllTasks();
                }
                else if (g.Unit.HasActiveTask())
                {
                    continue;
                }

                if (garrison.IsEligibleToEnter(g))
                    g.GoEnterGarrison(garrison);
            }
        }

        #endregion
    }

}