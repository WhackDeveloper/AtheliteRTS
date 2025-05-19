using UnityEngine;
using UnityEngine.Serialization;

namespace TRavljen.UnitSystem
{
    
#if UNITY_EDITOR
    using UnityEditor;
    using EditorUtility;
    internal static class FactionPlayerUnitSpawnerEditorTools
    {
        [MenuItem("GameObject/TRavljen/UnitSystem/Faction Player Spawner")]
        public static void CreateFactionSpawnerInScene()
        {
            if (!EditorTools.CreateObjectFromMenu<FactionPlayerUnitSpawner>("Faction Player Spawner", false, out var spawner))
                return;
            
            spawner.gameObject.AddComponent<PlayerBuildingsSpawn>();
            spawner.gameObject.AddComponent<UnitSpawnPoint>();
            Debug.Log("New faction spawner was created. You can now proceed with setting it up.");
        }
    }
#endif

    /// <summary>
    /// A spawner for player units that belongs to its faction on start.
    /// Responsible for initializing units and managing spawning mechanisms for units and buildings.
    /// </summary>
    public sealed class FactionPlayerUnitSpawner : APlayerUnitSpawner
    {
        /// <summary>
        /// The spawn point used for unit placement.
        /// </summary>
        [SerializeField] private UnitSpawnPoint unitSpawnPoint;

        /// <summary>
        /// The spawner responsible for constructing buildings.
        /// </summary>
        [SerializeField] private ABuildingSpawn buildingSpawn;

        /// <summary>
        /// Initializes the spawner by spawning the faction's starting units.
        /// </summary>
        private void Start()
        {
            SetupComponents();
            
            if (player.Faction != null)
            {
                // Spawn the initial units defined by the faction.
                SpawnUnits(player.Faction.GetStartingUnits());
            }
        }

        private void OnValidate()
        {
            SetupComponents();
        }

        private void SetupComponents()
        {
            if (player.IsNull())
                player = GetComponentInParent<APlayer>();
            
            if (unitSpawnPoint.IsNull())
                unitSpawnPoint = GetComponent<UnitSpawnPoint>();
            
            if (buildingSpawn.IsNull())
                buildingSpawn = GetComponent<ABuildingSpawn>();
        }

        /// <summary>
        /// Gets the unit spawn point used for placing units.
        /// </summary>
        /// <returns>The <see cref="UnitSpawnPoint"/> instance associated with this spawner.</returns>
        public override IUnitSpawn GetUnitSpawn()
        {
            return unitSpawnPoint;
        }

        /// <summary>
        /// Gets the building spawner used for constructing buildings.
        /// </summary>
        /// <returns>The <see cref="ABuildingSpawn"/> instance associated with this spawner.</returns>
        public override ABuildingSpawn GetBuildingSpawn()
        {
            return buildingSpawn;
        }
    }

}