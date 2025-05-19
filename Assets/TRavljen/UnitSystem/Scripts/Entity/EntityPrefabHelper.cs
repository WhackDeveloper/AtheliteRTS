namespace TRavljen.UnitSystem
{
    /// <summary>
    /// Utility class for loading entity prefabs. Provides methods to handle prefab loading for entities and units
    /// while ensuring the expected components are attached to the prefabs.
    /// </summary>
    public static class EntityPrefabHelper
    {

        /// <summary>
        /// Loads the prefab associated with a <see cref="AUnitSO"/> and invokes the provided callback with the loaded unit prefab.
        /// </summary>
        /// <param name="unit">The unit scriptable object from which to load the prefab.</param>
        /// <param name="loadedPrefab">Callback to invoke with the loaded unit prefab.</param>
        /// <exception cref="System.ArgumentNullException">
        /// Thrown if the loaded prefab does not have a <see cref="Unit"/> component attached.
        /// </exception>
        public static void LoadUnitPrefab(this AUnitSO unit, System.Action<Unit> loadedPrefab)
        {
            LoadEntityPrefab(unit, prefab =>
            {
                if (prefab is not Unit unitPrefab)
                {
                    throw new System.ArgumentNullException($"Prefab is null. Ensure the prefab {unit.Name} has attached Unit component.");
                }

                loadedPrefab.Invoke(unitPrefab);
            });
        }

        /// <summary>
        /// Loads the prefab associated with an <see cref="AEntitySO"/> and invokes the provided callback with the loaded entity prefab.
        /// </summary>
        /// <param name="entity">The entity scriptable object from which to load the prefab.</param>
        /// <param name="loadedPrefab">Callback to invoke with the loaded entity prefab.</param>
        /// <exception cref="System.ArgumentException">
        /// Thrown if the <paramref name="entity"/> does not implement <see cref="IProvidesEntityPrefab"/>.
        /// </exception>
        /// <exception cref="System.ArgumentNullException">
        /// Thrown if the loaded prefab is null.
        /// </exception>
        public static void LoadEntityPrefab(this AEntitySO entity, System.Action<IEntity> loadedPrefab)
        {
            if (entity is not IProvidesEntityPrefab providesPrefab)
            {
                throw new System.ArgumentException($"Cannot load prefab {entity.Name} for AEntitySO without implement IProvidesEntityPrefab");
            }

            providesPrefab.LoadAssociatedPrefab(prefab =>
            {
                if (prefab == null)
                {
                    throw new System.ArgumentNullException($"Prefab is null. Ensure the prefab is not null on: {entity.Name}");
                }

                loadedPrefab.Invoke(prefab);
            });
        }
    }

}