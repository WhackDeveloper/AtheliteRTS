using TRavljen.EditorUtility;
using UnityEngine;

namespace TRavljen.UnitSystem
{
    using System.Collections.Generic;
#if UNITY_EDITOR
    using UnityEditor;
#endif
    
    /// <summary>
    /// Centralized database for managing and tracking <see cref="ManagedSO"/> assets.
    /// This database allows for efficient ID management, grouping, and retrieval of assets,
    /// ensuring that only registered assets are managed while allowing others to remain untouched.
    /// </summary>
    /// <remarks>
    /// - IDs are assigned only to assets registered in the database.  
    /// - The database simplifies asset management by focusing on registered assets and avoiding
    ///   full project-wide scans.  
    /// - This approach supports scenarios such as preserving demo assets while isolating user-specific assets.  
    /// </remarks>
    public class SystemDatabase : ScriptableObject
    {

        private static SystemDatabase database;

        // Serialized list of all registered ManagedSO assets.
        [SerializeField, DisableInInspector]
        private List<ManagedSO> list = new();

        // Tracks the next available unique ID.
        [SerializeField, HideInInspector]
        private int currentIndex = startingIndex;

        private const int startingIndex = 0;

        public static System.Action OnDatabaseChange;

#if UNITY_EDITOR
        // Default path for the SystemDatabase asset.
        private static string AssetPath => UnitSystemConfig.GetOrCreate().CreateDatabasePath();
#endif

        #region Initialization

        /// <summary>
        /// Retrieves the instance of the <see cref="SystemDatabase"/>.  
        /// Creates a new instance if none exists.
        /// </summary>
        /// <returns>The singleton instance of the SystemDatabase.</returns>
        public static SystemDatabase GetInstance()
        {
            if (database) return database;

#if UNITY_EDITOR
            // Try to load the SystemDatabase from Resources
            database = AssetDatabase.LoadAssetAtPath<SystemDatabase>(AssetPath);

            if (database == null)
            {
                // Create new one
                database = CreateInstance<SystemDatabase>();
                SaveToFile();

                Debug.Log("SystemDatabase was not found, so a new one was created.");
            }
#else
            SystemDatabase[] databases = Resources.FindObjectsOfTypeAll<SystemDatabase>();
            if (databases != null && databases.Length > 0)
            {
                database = databases[0];
            }
            else
            {
                Debug.LogError("SystemDatabase was not found in resources, cannot create db in runtime.");
            }
#endif
            return database;
        }

#if UNITY_EDITOR
        /// <summary>
        /// Saves the database to a file in the Resources folder.
        /// </summary>
        private static void SaveToFile()
        {
            // Save the new database to the Resources folder
            string path = UnitSystemConfig.GetOrCreate().GetDatabaseFolderPath();
            AssetDatabaseHelper.CreateFolders(path);

            AssetDatabase.CreateAsset(database, AssetPath);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }
#endif

#endregion

        #region Asset Management

        public ManagedSO[] GetAllAssets() => list.ToArray();

        /// <summary>
        /// Cleanup missing assets (nulls) on validation.
        /// </summary>
        private void OnValidate()
        {
            RemoveMissingAssets();
        }

        /// <summary>
        /// Clears all registered assets and resets the index to the starting value.
        /// </summary>
        public void Reset()
        {
            list.Clear();
            currentIndex = startingIndex;

#if UNITY_EDITOR
            EditorUtility.SetDirty(this);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
#endif
        }

        /// <summary>
        /// Removes null references from the asset list.  
        /// This should be called after assets are deleted to ensure the list remains clean.
        /// </summary>
        public void RemoveMissingAssets()
        {
            int count = list.Count;
            list.RemoveAll(asset => asset == null);

            if (count != list.Count)
                OnDatabaseChange?.Invoke();
        }

        /// <summary>
        /// Retrieves a registered <see cref="ManagedSO"/> asset by its unique ID.
        /// </summary>
        /// <param name="id">The ID of the asset to retrieve.</param>
        /// <returns>The corresponding <see cref="ManagedSO"/> asset, or null if not found.</returns>
        public ManagedSO FindAssetById(int id)
        {
            foreach (ManagedSO asset in list)
                if (asset.ID == id)
                    return asset;

            return null;
        }

        /// <summary>
        /// Checks if a given <see cref="ManagedSO"/> asset is registered in the database.
        /// </summary>
        /// <param name="asset">The asset to check.</param>
        /// <returns>True if the asset is registered; otherwise, false.</returns>
        public bool IsRegistered(ManagedSO asset)
        {
            return list.Contains(asset);
        }

        /// <summary>
        /// Registers a <see cref="ManagedSO"/> asset in the database and assigns a unique ID if necessary.
        /// </summary>
        /// <param name="managedObject">The asset to register.</param>
        public void Register(ManagedSO managedObject)
        {
            // Either ID is default value, or one with this ID already exists;
            // then assign it a new ID.
            if (managedObject.ID == ManagedSO.invalidId || FindAssetById(managedObject.ID) != null)
            {
                int maxID = GetMaxID();

                // Assign the next unique ID
                currentIndex = maxID + 1;
                managedObject.AssignUniqueID(currentIndex);

                SetDirty(managedObject);
            }

            if (!list.Contains(managedObject))
            {
                list.Add(managedObject);
                SetDirty(this);
                OnDatabaseChange?.Invoke();
            }
        }

        /// <summary>
        /// Removes a <see cref="ManagedSO"/> asset from the database.
        /// </summary>
        /// <param name="asset">The asset to remove.</param>
        /// <returns>True if the asset was removed; otherwise, false.</returns>
        public bool Remove(ManagedSO asset)
        {
            return RemoveAssetWithId(asset.ID);
        }

        /// <summary>
        /// Removes an asset from the database using its unique ID.
        /// </summary>
        /// <param name="id">The ID of the asset to remove.</param>
        /// <returns>True if the asset was removed; otherwise, false.</returns>
        public bool RemoveAssetWithId(int id)
        {
            bool assetRemoved = false;
            for (int index = list.Count - 1; index >= 0; index--)
            {
                if (list[index].ID == id)
                {
                    var asset = list[index];

                    assetRemoved = true;

                    // Invalidate the ID & remove from the list
                    asset.InvalidateID();
                    // Remove from list
                    list.Remove(asset);

                    SetDirty(asset);
                }
            }
            return assetRemoved;
        }

        #endregion

        #region Helpers

        /// <summary>
        /// Marks the given asset as dirty to ensure changes are saved in the Unity Editor.
        /// </summary>
        /// <param name="asset">The asset to mark as dirty.</param>
        private void SetDirty(Object asset)
        {
#if UNITY_EDITOR
            EditorUtility.SetDirty(asset);
#endif
        }

        /// <summary>
        /// Gets the highest assigned ID among all registered assets.
        /// </summary>
        /// <returns>The maximum ID value.</returns>
        private int GetMaxID()
        {
            int maxID = ManagedSO.invalidId;

            foreach (ManagedSO asset in list)
            {
                if (maxID < asset.ID)
                    maxID = asset.ID;
            }

            return maxID;
        }

        #endregion
    }


#if UNITY_EDITOR
    /// <summary>
    /// Database cleanup processor triggered when an asset imported/created,
    /// moved or deleted and it will remove any missing assets from the database
    /// to avoid keeping <c>null</c> references.
    /// </summary>
    public class DatabaseCleanupProcessor : AssetPostprocessor
    {

        // This method is called after an asset is imported/created, moved or deleted
        private static void OnPostprocessAllAssets(
            string[] importedAssets,
            string[] deletedAssets,
            string[] movedAssets,
            string[] movedFromAssetPaths)
        {
            // This will re-create the DB if its missing, until custom path/asset is supported.
            SystemDatabase database = SystemDatabase.GetInstance();

            // We only need to cleanup missing assets, the rest is managed manually.
            if (deletedAssets.Length > 0)
            {
                database.RemoveMissingAssets();
            }

            AssetDatabase.SaveAssets();
        }
    }
#endif

}
