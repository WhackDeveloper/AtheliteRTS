using System;
using System.Collections;
using System.Collections.Generic;
using TRavljen.UnitSystem.Build;
using UnityEngine;

namespace TRavljen.UnitSystem
{

    /// <summary>
    /// Handles the spawning of buildings for a specific player, using predefined plot positions.
    /// </summary>
    public sealed class PlayerBuildingsSpawn : ABuildingSpawn
    {

        /// <summary>
        /// Predefined positions for placing buildings or units.
        /// </summary>
        [Tooltip("Specifies positions where building a unit is possible.")]
        [SerializeField]
        private Vector3[] buildingPlots = new Vector3[0];

        [Tooltip("Reference to the player associated with this spawner.")]
        [SerializeField]
        private APlayer player;

        // Array to track units placed at each predefined plot.
        private Unit[] placedUnits;

        // Reference to the player's BuildingModule for managing building states.
        private BuildingModule module;

        /// <summary>
        /// Initializes the building spawner, setting up the building plots and retrieving the BuildingModule.
        /// </summary>
        private void Awake()
        {
            placedUnits = new Unit[buildingPlots.Length];
            module = player.GetModule<BuildingModule>();
        }

        private void OnValidate()
        {
            if (player.IsNull())
                player = GetComponentInParent<APlayer>();
        }

        /// <inheritdoc />
        public override bool SpawnBuilding(AUnitSO unit, System.Action<Unit> unitSpawned)
        {
            if (module == null) return false;

            int positionIndex = GetPlacementIndex();
            if (positionIndex == -1)
            {
                Debug.Log("No more pre-defined postions to place units on. You have reached the maximum of unit plots!");
                return false;
            }

            var position = buildingPlots[positionIndex];
            unit.LoadUnitPrefab(prefab =>
            {
                var unit = Instantiate(prefab, position, Quaternion.identity, null);
                placedUnits[positionIndex] = unit;

                if (!(module != null && module.StartBuilding(unit)))
                {
                    Debug.Log("Building process failed to start..");
                }

                // Add unit to the player after building started
                // (unit should be disabled if it requires building when this is done)
                player.AddUnit(unit, true);

                unitSpawned.Invoke(unit);
            });

            return true;
        }

        /// <summary>
        /// Retrieves the index of the next available building plot.
        /// </summary>
        /// <returns>
        /// The index of an available plot, or -1 if no plots are available.
        /// </returns>
        private int GetPlacementIndex()
        {
            for (int index = 0; index < placedUnits.Length; index++)
            {
                if (placedUnits[index] == null) return index;
            }

            return -1;
        }

        /// <summary>
        /// Visualizes the building plots in the Editor when the GameObject is selected.
        /// </summary>
        private void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.red;
            Vector3 size = Vector3.one;
            foreach (var plot in buildingPlots)
            {
                Gizmos.DrawCube(plot, size);
            }
        }
    }

}