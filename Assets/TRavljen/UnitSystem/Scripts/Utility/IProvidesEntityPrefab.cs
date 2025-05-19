namespace TRavljen.UnitSystem
{

    /// <summary>
    /// Provided interfaces for accessing a prefab from <see cref="AEntitySO"/>.
    /// Because loading a prefab can be done in various ways from directly
    /// referencing an asset in project, loading it from path or using Addressables,
    /// this interfaces provides multiple ways of accessing it.
    /// If <see cref="GetAssociatedPrefab"/> is called and null is returned,
    /// then the spawning system will assume that prefab must be loaded
    /// in an async manner.
    /// Manager window uses <see cref="IsPrefabSet"/> to determine if prefab
    /// should be created for the data set.
    /// </summary>
    public interface IProvidesEntityPrefab: IEntityPrefabCreatable
    {

        /// <summary>
        /// Returns false if the prefab is missing and cannot be loaded.
        /// </summary>
        bool IsPrefabSet { get; }

        /// <summary>
        /// Checks if the prefab is already loaded and available.
        /// </summary>
        bool IsPrefabLoaded { get; }

        /// <summary>
        /// Returns the associated prefab if loaded, otherwise returns null.
        /// </summary>
        Entity GetAssociatedPrefab();

        /// <summary>
        /// Loads the associated prefab asynchronously.
        /// </summary>
        System.Threading.Tasks.Task<Entity> LoadAssociatedPrefabAsync();

        /// <summary>
        /// Loads the associated prefab asynchronously with a callback.
        /// </summary>
        void LoadAssociatedPrefab(System.Action<Entity> prefabLoaded);

        /// <summary>
        /// Sets or updates the prefab.
        /// </summary>
        /// <param name="prefab"></param>
        void SetAssociatedPrefab(Entity prefab);

    }

}