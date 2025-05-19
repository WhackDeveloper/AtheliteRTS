using System;
using UnityEngine;

#if UNITY_EDITOR
namespace TRavljen.UnitSystem
{
    using UnityEditor;
    
    /// <summary>
    /// Configuration ScriptableObject for the Unit System. This configuration file defines paths for the database
    /// and asset creation within the Unit System. It is essential for managing the Unit System's behavior and ensuring
    /// that assets and database files are stored in appropriate locations.
    /// </summary>
    public class UnitSystemConfig : ScriptableObject
    {

        // Constants
        private static readonly string configurationPath = "Assets/TRavljen/UnitSystem/UnitSystemConfig.asset";
        private static readonly string databaseDefaultPath = "Assets/Resources/UnitSystem/";
        private static readonly string newAssetsDefaultPath = "Assets/Resources/UnitSystem/Data";
        private static readonly string newPrefabsDefaultPath = "Assets/Resources/UnitSystem/Prefabs";
        private static readonly string databaseName = "System Database.asset";

        [SerializeField, HideInInspector]
        [Tooltip("Defines the path of the UnitSystem database.")]
        private string databaseCustomPath = null;

        [SerializeField, HideInInspector]
        [Tooltip("Path which defines where the Scriptable Objects of type ManagedSO " +
                 "will be created by the UnitSystem Manager window.")]
        private string assetsCustomPath = null;
        
        [SerializeField, HideInInspector]
        [Tooltip("Path which defines where the Prefabs for ManagedSO " +
                 "will be created by the UnitSystem Manager window.")]
        private string prefabsCustomPath = null;

        /// <summary>
        /// Gets the default name of the database file.
        /// </summary>
        public string DatabaseName => databaseName;

        /// <summary>
        /// Retrieves or creates the configuration instance. If no configuration exists, a new one is created
        /// at the default path. Use this method to ensure the configuration is available.
        /// </summary>
        /// <returns>The UnitSystemConfig instance.</returns>
        public static UnitSystemConfig GetOrCreate()
        {
            UnitSystemConfig config = AssetDatabase.LoadAssetAtPath<UnitSystemConfig>(configurationPath);

            if (config == null)
            {
                // Create new one
                config = CreateInstance<UnitSystemConfig>();

                config.ResetPaths();
                AssetDatabase.CreateAsset(config, configurationPath);
                AssetDatabase.SaveAssetIfDirty(config);
                AssetDatabase.Refresh();
            }

            return config;
        }

        private void Reset()
        {
            ResetPaths();
        }

        private void ResetPaths()
        {
            SetCustomDatabasePath(databaseDefaultPath);
            SetCustomAssetsPath(newAssetsDefaultPath);
        }

        #region Internal Set

        public void SetCustomDatabasePath(string path)
        {
            databaseCustomPath = path;
        }

        public void SetCustomAssetsPath(string path)
        {
            assetsCustomPath = path;
        }

        public void SetCustomPrefabsPath(string path)
        {
            prefabsCustomPath = path;
        }

        #endregion

        #region Public Get

        /// <summary>
        /// Gets the folder path for creating new assets. If no custom path is set, it uses the default path.
        /// </summary>
        public string GetAssetsFolderPath()
        {
            return string.IsNullOrEmpty(assetsCustomPath) ? newAssetsDefaultPath : assetsCustomPath;
        }

        /// <summary>
        /// Gets the folder path for the Unit System database. If no custom path is set, it uses the default path.
        /// </summary>
        public string GetDatabaseFolderPath()
        {
            return string.IsNullOrEmpty(databaseCustomPath) ? databaseDefaultPath : databaseCustomPath;
        }
        
        /// <summary>
        /// Gets the folder path for creating new prefabs. If no custom path is set, it uses the default path.
        /// </summary>
        public string GetsPrefabsFolderPath()
        {
            return string.IsNullOrEmpty(prefabsCustomPath) ? newPrefabsDefaultPath : prefabsCustomPath;
        }

        /// <summary>
        /// Constructs the full path for the database file based on the current database folder path.
        /// </summary>
        /// <returns>The full database path.</returns>
        public string CreateDatabasePath()
        {
            return CreateDatabasePath(GetDatabaseFolderPath());
        }

        /// <summary>
        /// Constructs the full path for the database file based on a specified folder.
        /// </summary>
        /// <param name="folder">The folder path to use for constructing the database path.</param>
        /// <returns>The full database path.</returns>
        public string CreateDatabasePath(string folder)
        {
            if (folder.LastIndexOf('/') != folder.Length - 1)
                folder += "/";

            return folder + databaseName;
        }

        #endregion
    }

}
#endif