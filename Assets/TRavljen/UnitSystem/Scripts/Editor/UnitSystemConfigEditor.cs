using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TRavljen.UnitSystem.Editor
{

    using UnityEditor;
    
    [CustomEditor(typeof(UnitSystemConfig))]
    class UnitSystemConfigEditor : Editor
    {

        private UnitSystemConfig config;
        
        private void OnEnable()
        {
            config = (UnitSystemConfig)target;
        }

        public override void OnInspectorGUI()
        {
            EditorGUILayout.HelpBox("You can update paths in Player Settings", MessageType.Info);
            
            EditorGUILayout.LabelField("Database path", config.GetDatabaseFolderPath());
            EditorGUILayout.LabelField("Assets path", config.GetAssetsFolderPath());
            EditorGUILayout.LabelField("Prefabs path", config.GetsPrefabsFolderPath());
            
            if (GUILayout.Button("Open player settings"))
            {
                SettingsService.OpenProjectSettings("Project/Unit System");
            }
        }
    }

}