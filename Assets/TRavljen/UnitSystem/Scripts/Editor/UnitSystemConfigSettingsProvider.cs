#if UNITY_EDITOR
namespace TRavljen.UnitSystem.Editor
{
    
    using UnityEditor;
    using UnityEngine;

    public class UnitSystemConfigSettings
    {
        #region Settings

        private static string editingDatabasePath = "";
        private static string editingAssetsPath = "";
        private static string editingPrefabPath = "";

        [SettingsProvider]
        public static SettingsProvider CreateSettingsProvider()
        {
            return new SettingsProvider("Project/Unit System", SettingsScope.Project)
            {
                activateHandler = (title, elements) =>
                {
                    var config = UnitSystemConfig.GetOrCreate();
                    editingDatabasePath = config.GetDatabaseFolderPath();
                    editingAssetsPath = config.GetAssetsFolderPath();
                    editingPrefabPath = config.GetsPrefabsFolderPath();
                },
                guiHandler = (searchContext) =>
                {
                    OnGUI();
                }
            };
        }

        private static void OnGUI()
        {
            GUILayout.Label("Custom Configuration", EditorStyles.boldLabel);

            EditorGUILayout.Space(16);

            UnitSystemConfig config = UnitSystemConfig.GetOrCreate();

            editingDatabasePath = AddPathSelection("Database path:", editingDatabasePath);
            editingAssetsPath = AddPathSelection("Assets path:", editingAssetsPath);
            editingPrefabPath = AddPathSelection("Prefabs path:", editingPrefabPath);

            var didDatabasePathChange = config.GetDatabaseFolderPath() != editingDatabasePath;
            var didAssetsPathChange = config.GetAssetsFolderPath() != editingAssetsPath;
            var didPrefabsPathChange = config.GetsPrefabsFolderPath() != editingPrefabPath;

            GUI.enabled = didAssetsPathChange || didDatabasePathChange || didPrefabsPathChange;

            GUILayout.BeginHorizontal();

            EditorGUILayout.Space();
            EditorGUILayout.Space();
            
            if (GUILayout.Button("Apply changes", GUILayout.Width(150)))
            {
                if (didDatabasePathChange)
                    UpdateDatabasePath(config, editingDatabasePath);

                if (didAssetsPathChange)
                    UpdateAssetsPath(config, editingAssetsPath);

                if (didPrefabsPathChange)
                    UpdatePrefabsPath(config, editingAssetsPath);
            }

            GUILayout.EndHorizontal();

            GUI.enabled = true;
        }

        private static bool UpdatePrefabsPath(UnitSystemConfig config, string newPath)
        {
            // Update path without moving prefabs
            config.SetCustomPrefabsPath(newPath);

            // Save the updated configuration
            EditorUtility.SetDirty(config);
            AssetDatabase.SaveAssetIfDirty(config);
            Debug.Log($"Prefabs path updated to: {newPath}");

            return true;
        }

        private static bool UpdateDatabasePath(UnitSystemConfig config, string newPath)
        {
            if (ValidateAndCreatePath(newPath))
            {
                string oldPath = config.GetDatabaseFolderPath();

                if (oldPath != newPath && MoveDatabase(config, oldPath, newPath))
                {
                    config.SetCustomDatabasePath(newPath);

                    // Save the updated configuration
                    EditorUtility.SetDirty(config);
                    AssetDatabase.SaveAssetIfDirty(config);
                    Debug.Log($"Database path updated to: {newPath}");
                    return true;
                }
                else
                {
                    Debug.Log("The database path is unchanged.");
                }
            }
            else
            {
                Debug.LogError("Invalid database path. Please correct it and try again.");
            }

            return false;
        }

        private static bool UpdateAssetsPath(UnitSystemConfig config, string newPath)
        {
            if (ValidateAndCreatePath(newPath))
            {
                string oldPath = config.GetAssetsFolderPath();

                if (oldPath != newPath)
                {
                    config.SetCustomAssetsPath(newPath);

                    // Save the updated configuration
                    EditorUtility.SetDirty(config);
                    AssetDatabase.SaveAssetIfDirty(config);
                    Debug.Log($"Assets path updated to: {newPath}");

                    return true;
                }
                else
                {
                    Debug.Log("The assets path is unchanged.");
                }
            }
            else
            {
                Debug.LogError("Invalid assets path. Please correct it and try again.");
            }

            return false;
        }

        private static string AddPathSelection(string label, string path)
        {
            EditorGUILayout.BeginHorizontal();
            var newPath = EditorGUILayout.TextField(label, path);
            if (BrowseNewPath(newPath, out var updatedPath))
                newPath = updatedPath;
            EditorGUILayout.EndHorizontal();
            return newPath;
        }

        private static bool BrowseNewPath(string currentFolder, out string newPath)
        {
            newPath = null;
            
            if (!GUILayout.Button("Browse", GUILayout.Width(100)))
                return false;

            var selected = EditorUtility.OpenFolderPanel("Select folder", currentFolder, "");
            if (string.IsNullOrEmpty(selected)) return false;
            
            newPath = ToRelativePath(selected);
            return true;
        }

        private static bool MoveDatabase(UnitSystemConfig config, string oldPath, string newPath)
        {
            oldPath = config.CreateDatabasePath(oldPath);
            newPath = config.CreateDatabasePath(newPath);

            string error = AssetDatabase.MoveAsset(oldPath, newPath);
            if (error.Length > 0)
            {
                Debug.LogError("Database failed to move duo to error: " + error);
                return false;
            }
            else
            {
                return true;
            }
        }

        private static bool ValidateAndCreatePath(string path)
        {
            if (string.IsNullOrEmpty(path)) return false;

            if (AssetDatabase.IsValidFolder(path))
            {
                return true;
            }

            return AssetDatabaseHelper.CreateFolders(path);
        }
        
        private static string ToRelativePath(string fullPath)
        {
            if (fullPath.StartsWith(Application.dataPath))
                return "Assets" + fullPath.Substring(Application.dataPath.Length);
            return fullPath;
        }

        #endregion
    }

}
#endif