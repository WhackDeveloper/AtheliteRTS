using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace TRavljen.UnitSystem.Editor
{

    /// <summary>
    /// Utility class for creating and saving prefabs in Unity Editor.
    /// </summary>
    public static class PrefabCreator
    {
        /// <summary>
        /// Creates a prefab using the provided <see cref="IEntityPrefabCreatable"/> data 
        /// and saves it to the specified asset path.
        /// </summary>
        /// <param name="prefabData">The data used to create the prefab.</param>
        /// <param name="assetPath">The folder path where the prefab asset will be saved.</param>
        /// <param name="fileName">The name of the prefab file (without extension).</param>
        /// <param name="prefab">The resulting prefab GameObject, if successfully created.</param>
        /// <returns>True if the prefab was successfully created and saved; otherwise, false.</returns>
        public static bool CreateAndSavePrefab(
            IEntityPrefabCreatable prefabData,
            string assetPath,
            string fileName,
            out GameObject prefab)
        {
            if (prefabData == null)
            {
                prefab = null;
                Debug.LogError("Prefab data is null. Cannot create prefab.");
                return false;
            }

            // Create a temporary scene for prefab creation
            var tempScene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Additive);

            try
            {
                Debug.Log($"Creating prefab using data: {prefabData}");

                // Create the prefab GameObject in the temporary scene
                GameObject prefabGO = prefabData.CreatePrefab(fileName);

                // Ensure the directory for the asset path exists
                string directoryPath = System.IO.Path.GetDirectoryName(assetPath);
                if (!System.IO.Directory.Exists(directoryPath))
                {
                    System.IO.Directory.CreateDirectory(directoryPath);
                }

                // Save the GameObject as a prefab asset to the specified path
                string fullPath = System.IO.Path.Combine(assetPath, fileName + ".prefab");
                PrefabUtility.SaveAsPrefabAsset(prefabGO, fullPath);

                // Clean up by deleting the temporary GameObject
                Object.DestroyImmediate(prefabGO);

                // Load the prefab from disk to confirm successful creation
                prefab = AssetDatabase.LoadAssetAtPath<GameObject>(fullPath);

                Debug.Log($"Prefab saved successfully at {fullPath}");
            }
            catch (System.Exception exception)
            {
                prefab = null;
                Debug.LogError($"Failed to create prefab: {exception}");
            }

            // Unload the temporary scene to clean up resources
            EditorSceneManager.CloseScene(tempScene, true);

            return prefab != null;
        }
    }

}
