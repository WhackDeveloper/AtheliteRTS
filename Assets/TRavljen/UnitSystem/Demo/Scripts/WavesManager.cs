using System.Collections.Generic;
using UnityEngine;

namespace IntegrationDemo
{

    using TRavljen.EditorUtility;
    using TRavljen.UnitSystem;
    using TRavljen.UnitSystem.Combat;
    using TRavljen.UnitSystem.Task;

    /// <summary>
    /// Represents a structure that holds an array of unit quantities for a wave.
    /// </summary>
    [System.Serializable]
    public struct WavePrefabs
    {
        /// <summary>
        /// The array of unit quantities to spawn in this wave.
        /// </summary>
        public UnitQuantity[] units;
    }

    /// <summary>
    /// Manages the spawning and behavior of waves of units in the game.
    /// This class controls the timing between waves, the spawning of units,
    /// and their actions during battle.
    /// </summary>
    [RequireComponent(typeof(WaveUnitSpawner))]
    public class WavesManager : MonoBehaviour
    {

        [Tooltip("The player controlled by the human or main player.")]
        [SerializeField]
        private APlayer targetPlayer;

        [Tooltip("The player controlled by the AI.")]
        [SerializeField]
        private APlayer botPlayer;

        [Tooltip("Indicates whether to reset the spawn timer after all wave units are cleared (killed). " +
            "If disabled, timer will reset right after spawning wave units.")]
        [SerializeField]
        private bool resetTimerAfterWavesIsCleared = true;

        [Tooltip("The delay in seconds before the first wave spawns.")]
        [SerializeField]
        private float delayBeforeFirstWave = 60;

        [Tooltip("The time in seconds between subsequent waves.")]
        [SerializeField]
        private float timeBetweenWaves = 40;

        [Tooltip("The unit prefabs and their quantities to spawn for the wave.")]
        [SerializeField]
        private WavePrefabs prefabs = new();

        [Tooltip("The spawner responsible for creating units in the wave.")]
        [SerializeField]
        private WaveUnitSpawner waveSpawner;

        [Tooltip("The timer tracking the time until the next wave should spawn. " +
            "This field is disabled in the inspector to prevent manual edits.")]
        [SerializeField, DisableInInspector]
        private float spawnTimer = 0;

        [Tooltip("Indicates whether a wave is currently active. This field is " +
            "disabled in the inspector to prevent manual edits.")]
        [SerializeField, DisableInInspector]
        private bool isWaveActive = false;

        [Tooltip("A list of units that are currently in the active wave. " +
            "This field is disabled in the inspector to prevent manual edits.")]
        [SerializeField, DisableInInspector]
        protected List<Unit> activeWaveUnits = new();

        /// <summary>
        /// The task to execute for attacking units; contains the logic for 
        /// moving and attacking using the specified unit.
        /// </summary>
        private readonly ITask attackTask = new MoveAndAttackTask();

        private void Awake()
        {
            spawnTimer = delayBeforeFirstWave;

            if (waveSpawner == null)
                waveSpawner = GetComponent<WaveUnitSpawner>();
        }

        private void OnValidate()
        {
            if (waveSpawner == null)
                waveSpawner = GetComponent<WaveUnitSpawner>();

            waveSpawner.Player = botPlayer;
        }

        private void Update()
        {
            UpdateTimer();

            if (spawnTimer < 0)
            {
                ResetTimer();
            }

            UpdateActiveUnits();
        }

        /// <summary>
        /// Updates the spawn timer, decreasing it each frame.
        /// If resetTimerAfterWavesIsCleared is true, it only updates when no wave is active.
        /// </summary>
        protected virtual void UpdateTimer()
        {
            if (resetTimerAfterWavesIsCleared)
            {
                if (!isWaveActive)
                    spawnTimer -= Time.deltaTime;
            }
            else
            {
                spawnTimer -= Time.deltaTime;
            }
        }

        /// <summary>
        /// Resets the spawn timer for the next wave and spawns a new wave of units.
        /// </summary>
        protected virtual void ResetTimer()
        {
            spawnTimer = timeBetweenWaves;

            SpawnWave();
        }

        /// <summary>
        /// Updates the active units, assigning a target and sending them to attack.
        /// </summary>
        protected virtual void UpdateActiveUnits()
        {
            bool findTarget = true;
            Vector3 targetPosition = Vector3.zero;

            foreach (var activeUnit in activeWaveUnits)
            {
                if (activeUnit.ActiveTask != null) continue;

                if (findTarget)
                {
                    targetPosition = GetTargetPosition();
                    findTarget = false;
                }

                SendUnitToAttack(activeUnit, targetPosition);
            }
        }

        /// <summary>
        /// Gets the position of a random unit owned by the target player.
        /// If there are no operational units, returns Vector3.zero.
        /// </summary>
        /// <returns>A Vector3 representing the target position, or Vector3.zero if no units are available.</returns>
        private Vector3 GetTargetPosition()
        {
            List<Vector3> positions = new();

            foreach (var unit in targetPlayer.GetUnits())
            {
                if (!unit.IsOperational) continue;

                positions.Add(unit.transform.position);
            }

            if (positions.Count == 0)
            {
                Debug.Log("No more player units!");
                return Vector3.zero;
            }

            return positions[Random.Range(0, positions.Count)];
        }

        /// <summary>
        /// Handles the logic when a unit's health is depleted.
        /// Removes the unit from the active wave and checks if the wave is still active.
        /// </summary>
        /// <param name="health">The health component of the depleted unit.</param>
        /// <param name="attacker">The unit that attacked and caused the health depletion.</param>
        private void HealthDepleted(IHealth health, AUnitComponent attacker)
        {
            if (health.Entity is not Unit unit) return;

            activeWaveUnits.Remove(unit);

            if (activeWaveUnits.Count == 0)
            {
                isWaveActive = false;
            }
        }

        /// <summary>
        /// Spawns a new wave of units based on the configured prefabs.
        /// Sets the wave to active and assigns tasks to the spawned units.
        /// </summary>
        private void SpawnWave()
        {
            // Set active
            isWaveActive = true;

            waveSpawner.SpawnWave(prefabs, spawnedUnits =>
            {
                activeWaveUnits = new(spawnedUnits);

                // Assign them attack tasks for target positions
                Vector3 targetPosition = GetTargetPosition();

                foreach (var newUnit in activeWaveUnits)
                {
                    // Observe when unit is killed, remove it from the list
                    if (newUnit.TryGetComponent(out Health health))
                    {
                        health.OnHealthDepleted.AddListener(HealthDepleted);
                    }

                    SendUnitToAttack(newUnit, targetPosition);
                }
            });
        }

        /// <summary>
        /// Sends a specified unit to attack at the given target position.
        /// Utilizes the Move and Attack strategy via the unit's task system.
        /// </summary>
        /// <param name="unit">The unit to send to attack.</param>
        /// <param name="targetPosition">The position to attack.</param>
        private void SendUnitToAttack(Unit unit, Vector3 targetPosition)
        {
            // Uses Move to Attack if present. For this a Task with Position Context
            // for attacking must be present on the unit.
            if (unit.TryGetComponent(out UnitAttack attack))
            {
                attack.SetStance(AttackStance.Aggressive);

                unit.ScheduleTask(
                    new PositionContext(targetPosition),
                    new AttackTaskInput(attack),
                    attackTask);
            }
        }

    }

}