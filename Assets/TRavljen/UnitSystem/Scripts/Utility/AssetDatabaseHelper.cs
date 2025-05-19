using UnityEditor;
using UnityEngine;

#if UNITY_EDITOR
namespace TRavljen.UnitSystem
{

    public static class AssetDatabaseHelper
    {
        public static bool CreateFolders(string fullPath)
        {
            if (string.IsNullOrEmpty(fullPath))
            {
                Debug.LogError("Path cannot be null or empty.");
                return false;
            }

            // Split the path into segments
            string[] paths = fullPath.Split('/');
            if (paths.Length == 0 || paths[0] != "Assets")
            {
                Debug.LogError("Path must start with 'Assets'.");
                return false;
            }

            string combinedPath = paths[0]; // Start with "Assets"
            for (int i = 1; i < paths.Length; i++)
            {
                string folderName = paths[i];
                string currentPath = $"{combinedPath}/{folderName}";

                if (!AssetDatabase.IsValidFolder(currentPath))
                {
                    string guid = AssetDatabase.CreateFolder(combinedPath, folderName);
                    if (string.IsNullOrEmpty(guid))
                    {
                        Debug.LogError($"Failed to create folder: {currentPath}");
                        return false;
                    }
                }

                combinedPath = currentPath; // Update the path for the next iteration
            }

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            return true;
        }

    }

}
#endif