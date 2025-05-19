using UnityEngine;

namespace TRavljen.UnitSystem.Editor
{
    using UnityEditor;
    using UnityEngine;
    using System.Collections.Generic;

    /// <summary>
    /// Utility class for managing <see cref="ManagedSO"/> assets within the project.
    /// </summary>
    /// <remarks>
    /// Provides methods for batch-processing all ManagedSO assets, such as registering them in the <see cref="SystemDatabase"/>.
    /// This class is intended for editor use only.
    /// </remarks>
    public class ManagedSOTools : MonoBehaviour
    {
        /// <summary>
        /// Finds all <see cref="ManagedSO"/> assets in the project and registers them in the <see cref="SystemDatabase"/>.
        /// </summary>
        /// <remarks>
        /// This method is useful for initializing the database with all existing ManagedSO assets
        /// when starting a new project or re-importing assets.
        /// </remarks>
        public static void AddAllAssetsToDatabase()
        {
            // Retrieve all ManagedSO assets in the project
            List<ManagedSO> foundObjects = GetAllManagedSO();

            // Register each asset to the DB if its not present yet.
            // Invoking this when its already present will assign it a new ID.
            SystemDatabase database = SystemDatabase.GetInstance();
            foreach (var obj in foundObjects)
            {
                if (!database.IsRegistered(obj))
                {
                    database.Register(obj);
                }
            }

            // Save and refresh the asset database to apply changes
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }

        /// <summary>
        /// Retrieves a list of all <see cref="ManagedSO"/> assets in the project.
        /// </summary>
        /// <returns>A list of all ManagedSO assets found in the project.</returns>
        /// <remarks>
        /// This method searches the project using Unity's AssetDatabase for assets of type ManagedSO
        /// and loads them into memory.
        /// </remarks>
        private static List<ManagedSO> GetAllManagedSO()
        {
            List<ManagedSO> managedObjects = new List<ManagedSO>();

            // Search for all ManagedSO assets in the project
            string[] guids = AssetDatabase.FindAssets("t:ManagedSO");

            foreach (string guid in guids)
            {
                string assetPath = AssetDatabase.GUIDToAssetPath(guid);
                ManagedSO asset = AssetDatabase.LoadAssetAtPath<ManagedSO>(assetPath);

                if (asset)
                {
                    managedObjects.Add(asset);
                }
            }

            return managedObjects;
        }
    }

}