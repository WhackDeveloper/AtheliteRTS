using System;
using System.Collections;
using System.Collections.Generic;
using TRavljen.UnitSystem.Build;
using UnityEngine;

namespace TRavljen.UnitSystem
{

    /// <summary>
    /// Base class for handling unit spawning logic for a player. 
    /// Supports both direct unit spawning and construction-based spawning.
    /// </summary>
    public abstract class APlayerUnitSpawner : MonoBehaviour
    {

        [Tooltip("Determines if constructed units should finish construction instantly after spawning.")]
        [SerializeField]
        protected bool instantConstruction = true;

        [Tooltip("The player associated with this spawner. Units will belong to this player.")]
        [SerializeField]
        protected APlayer player;

        [Tooltip("Determines if units should move to a random position after spawning.")]
        [SerializeField]
        protected bool moveAfterSpawn = false;

        /// <summary>
        /// Retrieves the building spawn responsible for spawning buildings in world space.
        /// </summary>
        /// <returns>The <see cref="ABuildingSpawn"/> instance associated with the player.</returns>
        public abstract ABuildingSpawn GetBuildingSpawn();

        /// <summary>
        /// Retrieves the spawn point used for direct unit spawning.
        /// </summary>
        /// <returns>The <see cref="UnitSpawnPoint"/> instance used for unit placement.</returns>
        public abstract IUnitSpawn GetUnitSpawn();

        /// <summary>
        /// Spawns units based on the specified quantities and calls a callback once all units are spawned.
        /// </summary>
        /// <param name="unitQuantity">An array defining the units and their respective counts to spawn.</param>
        /// <param name="unitsSpawned">Optional callback invoked when all units are spawned.</param>
        public void SpawnUnits(UnitQuantity[] unitQuantity, Action<Unit[]> unitsSpawned = null)
        {
            List<Unit> spawnedUnits = new();
            int spawningGroups = unitQuantity.Length;

            // Suggestion: Improve the grouping logic for clarity and reusability.
            // Current approach is simple but tightly coupled to the spawning loop.
            void EndGroupSpawn()
            {
                spawningGroups--;

                if (spawningGroups == 0)
                    unitsSpawned?.Invoke(spawnedUnits.ToArray());
            }

            foreach (var entry in unitQuantity)
            {
                if (entry.unit == null)
                {
                    EndGroupSpawn();
                    continue;
                }

                // If unit requires building, use building spawner
                if (entry.unit.TryGetCapability(out BuildableCapability _))
                {
                    ABuildingSpawn spawn = GetBuildingSpawn();

                    if (spawn == null)
                    {
                        throw new NullReferenceException("Building spawner is not set. Cannot spawn buildings without it.");
                    }

                    spawningGroups += entry.count;
                    for (int index = 0; index < entry.count; index++)
                    {
                        spawn.SpawnBuilding(entry.unit, spawnedUnit =>
                        {
                            // Assign the unit to the player
                            player.AddUnit(spawnedUnit, true);

                            // Finish construction if applicable
                            if (instantConstruction && spawnedUnit.Buildable != null)
                            {
                                spawnedUnit.Buildable.FinishBuilding();
                            }

                            spawnedUnits.Add(spawnedUnit); 
                            EndGroupSpawn();
                        });
                    }

                    EndGroupSpawn();
                }
                else
                {
                    spawningGroups += entry.count;
                    for (var index = 0; index < entry.count; index++)
                    {
                        GetUnitSpawn().SpawnUnit(entry.unit, moveAfterSpawn, unit =>
                        {
                            // Assign the unit to the player
                            player.AddUnit(unit, true);

                            spawnedUnits.Add(unit);
                            EndGroupSpawn();
                        });
                    }

                    EndGroupSpawn();
                }
            }
        }

    }

}