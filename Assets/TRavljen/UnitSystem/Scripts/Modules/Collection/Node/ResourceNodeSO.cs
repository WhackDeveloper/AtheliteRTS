using System.Collections;
using UnityEngine;
using System;
using System.Threading.Tasks;

namespace TRavljen.UnitSystem.Collection
{

    /// <summary>
    /// Scriptable object defining a resource node in the game world. 
    /// This class is responsible for providing and managing the associated prefab 
    /// for resource nodes and ensuring required capabilities are present.
    /// </summary>
    public class ResourceNodeSO : AEntitySO, IProvidesEntityPrefab
    {

        [SerializeField]
        [Tooltip("The associated prefab for this resource node.")]
        private Entity associatedPrefab;

        private void OnEnable()
        {
            // Ensure the ResourceNodeCapability is present when this scriptable object is enabled.
            if (!TryGetCapability(out IResourceNodeCapability _))
                AddCapability(new ResourceNodeCapability());
        }

        #region IProvidesEntityPrefab

        /// <inheritdoc/>
        public bool IsPrefabSet => associatedPrefab != null;

        /// <inheritdoc/>
        public bool IsPrefabLoaded => associatedPrefab != null;

        /// <inheritdoc/>
        public bool HasPrefab() => associatedPrefab != null;

        /// <inheritdoc/>
        public void SetPrefab(Entity entity) => associatedPrefab = entity;

        /// <inheritdoc/>
        public Entity GetAssociatedPrefab() => associatedPrefab;

        /// <inheritdoc/>
        public Task<Entity> LoadAssociatedPrefabAsync()
        {
            return System.Threading.Tasks.Task.Run(() => associatedPrefab);
        }

        /// <inheritdoc/>
        public void LoadAssociatedPrefab(Action<Entity> prefabLoaded)
        {
            prefabLoaded.Invoke(associatedPrefab);
        }

        /// <inheritdoc/>
        public void SetAssociatedPrefab(Entity entity)
        {
            associatedPrefab = entity;
        }

        #endregion
    }

}