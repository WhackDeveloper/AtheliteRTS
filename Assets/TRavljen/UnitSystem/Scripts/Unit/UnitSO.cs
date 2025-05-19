using UnityEngine;
using System;
using System.Threading.Tasks;

namespace TRavljen.UnitSystem
{

    /// <summary>
    /// Represents a unit definition in the game, deriving from <see cref="AUnitSO"/>.
    /// This scriptable object provides a basic prefab-based implementation for spawning units
    /// as game objects within the scene. Customizations can be achieved by subclassing <see cref="AUnitSO"/>.
    /// </summary>
    [CreateAssetMenu(fileName = "New unit", menuName = "Unit System/Units/Unit")]
    public class UnitSO : AUnitSO, IProvidesEntityPrefab
    {

        /// <summary>
        /// Specifies the game object prefab used for spawning the unit in the scene.
        /// </summary>
        [SerializeField, Tooltip("Specifies the game object prefab used for " +
            "spawning the unit within the scene.")]
        private Unit associatedPrefab;

        #region IProvidesPrefab

        public bool IsPrefabSet => associatedPrefab != null;

        public bool IsPrefabLoaded => associatedPrefab != null;

        public Entity GetAssociatedPrefab() => associatedPrefab;

        public Task<Entity> LoadAssociatedPrefabAsync()
        {
            return System.Threading.Tasks.Task.Run(() => associatedPrefab as Entity);
        }

        public void LoadAssociatedPrefab(Action<Entity> prefabLoaded)
        {
            prefabLoaded.Invoke(associatedPrefab);
        }

        public void SetAssociatedPrefab(Entity entity)
        {
            if (entity is Unit unit)
                associatedPrefab = unit;
            else
                associatedPrefab = null;
        }

        #endregion
    }

}
