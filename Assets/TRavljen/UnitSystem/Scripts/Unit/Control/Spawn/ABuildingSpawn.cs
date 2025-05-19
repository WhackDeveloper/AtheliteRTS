using UnityEngine;

namespace TRavljen.UnitSystem
{

    /// <summary>
    /// Abstract base class defining the contract for spawning buildings.
    /// </summary>
    public abstract class ABuildingSpawn : MonoBehaviour
    {
        /// <summary>
        /// Spawns a building using the specified unit data.
        /// </summary>
        /// <param name="unit">The unit data representing the building to spawn.</param>
        /// <param name="unitSpawned">Callback invoked after the building is successfully spawned.</param>
        /// <returns>Returns true if building was spawned, otherwise false.</returns>
        public abstract bool SpawnBuilding(AUnitSO unit, System.Action<Unit> unitSpawned);
    }

}
