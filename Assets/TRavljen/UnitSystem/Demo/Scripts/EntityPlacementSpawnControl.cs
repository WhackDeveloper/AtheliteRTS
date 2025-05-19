using UnityEngine;

namespace TRavljen.UnitSystem.Demo
{

    /// <summary>
    /// Controls the spawning of buildable units by utilizing <see cref="ABuildingSpawn"/> 
    /// and responding to <see cref="ProductionModule.OnStartPlacementRequest"/> 
    /// to initiate the placement of units when a production action is triggered.
    /// </summary>
    public class EntityPlacementSpawnControl: MonoBehaviour
    {

        /// <summary>
        /// Manages placement of the buildable units.
        /// </summary>
        [SerializeField, Tooltip("Manages placement of the buildable units.")]
        private ABuildingSpawn buildingSpawn;

        [SerializeField]
        private APlayer player;

        protected void OnValidate()
        {
            if (player == null)
            {
                player = GetComponentInParent<APlayer>();
            }

            if (player != null && buildingSpawn == null)
            {
                buildingSpawn = player.GetComponentInChildren<ABuildingSpawn>();
            }
        }

        private void OnEnable()
        {
            if (player.TryGetModule(out ProductionModule module))
            {
                module.OnStartPlacementRequest.AddListener(StartPlacement);
            }
        }

        private void OnDisable()
        {
            if (player.TryGetModule(out ProductionModule module))
            {
                module.OnStartPlacementRequest.RemoveListener(StartPlacement);
            }
        }

        /// <summary>
        /// Handles the placement of a entity by taking the next available building plot.
        /// </summary>
        private void StartPlacement(PlacementRequiredInfo info)
        {
            if (info.prefab is not Unit unit)
            {
                Debug.Log("Demo scene supports Unit placement only. Implement your own placement for non Unit entities.");
                return;
            }

            long toSpawn = info.quantity;

            // Support edge case of spawning more than single unit.
            while (toSpawn > 0)
            {
                toSpawn--;

                if (player.TryGetModule(out ResourceModule module) &&
                    !module.RemoveResources(unit.Data.Cost))
                {
                    Debug.Log("Not enough resources. Placement should not have been invoked! Something ain't right.");
                    return;
                }

                bool spawned = buildingSpawn.SpawnBuilding(unit.Data, spawnedUnit =>
                {
                    /*
                     Do something extra after placement here
                     */
                });

                // When spawning fails, stop the loop.
                if (!spawned)
                    break;
            }
        }

    }


}