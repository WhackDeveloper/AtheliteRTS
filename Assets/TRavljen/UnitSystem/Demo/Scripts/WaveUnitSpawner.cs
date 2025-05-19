using IntegrationDemo;
using UnityEngine;
using System;

namespace TRavljen.UnitSystem
{

    /// <summary>
    /// A specialized unit spawner for spawning units in waves. 
    /// It cycles through a list of predefined spawn points and supports integration with building spawners.
    /// </summary>
    public class WaveUnitSpawner : APlayerUnitSpawner
    {

        [Tooltip("An array of unit spawn points used to distribute spawning locations for waves.")]
        [SerializeField]
        private UnitSpawnPoint[] spawnPoints = new UnitSpawnPoint[1];

        private int spawnPointIndex = 0;

        public APlayer Player
        {
            set => this.player = value;
            get => player;
        }

        /// <summary>
        /// Spawns a wave of units based on the provided wave data and executes a callback upon completion.
        /// </summary>
        /// <param name="wave">The wave data containing unit types and quantities to spawn.</param>
        /// <param name="spawnedWaveUnits">
        /// A callback invoked when all units in the wave are spawned. 
        /// Receives an array of spawned units.
        /// </param>
        public void SpawnWave(WavePrefabs wave, Action<Unit[]> spawnedWaveUnits)
        {
            // Cycle through spawn points for each wave
            spawnPointIndex = (spawnPointIndex + 1) % spawnPoints.Length;

            // Spawn the units in the wave
            SpawnUnits(wave.units, spawnedWaveUnits);
        }

        /// <summary>
        /// Retrieves the building spawner used for constructing units that require a building process.
        /// </summary>
        /// <returns>The <see cref="ABuildingSpawn"/> instance associated with this spawner.</returns>
        public override ABuildingSpawn GetBuildingSpawn()
        {
            return null;
        }

        /// <summary>
        /// Retrieves the current unit spawn point, cycling through available points for each wave.
        /// </summary>
        /// <returns>The <see cref="UnitSpawnPoint"/> selected for the current wave.</returns>
        public override IUnitSpawn GetUnitSpawn()
        {
            return spawnPoints[spawnPointIndex];
        }

    }
}