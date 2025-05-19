using System;
using UnityEngine;

namespace TRavljen.UnitSystem
{
    /// <summary>
    /// Defines a spawning point for units, responsible for spawning and respawning 
    /// units at a designated location. Spawn points may also specify a target location 
    /// for spawned units to move towards.
    /// </summary>
    public interface IUnitSpawn: IControlEntity
    {
        
        /// <summary>
        /// Gets the point where units are spawned from this control.
        /// </summary>
        Vector3 SpawnPoint { get; }

        /// <summary>
        /// Spawns a single unit using its associated data and invokes a callback upon completion.
        /// </summary>
        /// <param name="unitData">
        /// The <see cref="AUnitSO"/> that defines the data and prefab for the unit to spawn.
        /// </param>
        /// <param name="moveToTarget">
        /// If true, and the spawn point has a defined target position, the unit will 
        /// automatically move towards the target after spawning.
        /// </param>
        /// <param name="unitSpawned">
        /// Callback invoked when the unit is successfully spawned. Provides the spawned 
        /// <see cref="Unit"/> as a parameter.
        /// </param>
        void SpawnUnit(AUnitSO unitData, bool moveToTarget, Action<Unit> unitSpawned);

        /// <summary>
        /// Spawns a unit from the specified prefab at the spawn point.
        /// </summary>
        /// <param name="prefab">The prefab to use for spawning the new unit.</param>
        /// <param name="moveToTarget">
        /// If true, and the spawn point has a defined target position, the unit will 
        /// automatically move towards the target after spawning.
        /// </param>
        /// <returns>The newly instantiated unit.</returns>
        Unit SpawnUnit(Unit prefab, bool moveToTarget);

        /// <summary>
        /// Respawns an existing unit at the spawn point, reactivating it if necessary.
        /// </summary>
        /// <param name="sceneUnit">The unit instance to respawn.</param>
        /// <param name="moveToTarget">
        /// If true, and the spawn point has a defined target position, the unit will 
        /// automatically move towards the target after respawning.
        /// </param>
        void RespawnUnit(Unit sceneUnit, bool moveToTarget);
    }

}