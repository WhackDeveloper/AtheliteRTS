using UnityEngine;

namespace TRavljen.UnitSystem.Editor
{
    using UnityEditor;
    using System;

    public class EnterAssetNamePopup : EditorWindow
    {
        public string assetName = "";
        public Action<string> OnNameEntered;

        public static void ShowPopup(Action<string> onNameEntered, string fileName = "New Asset")
        {
            var window = ScriptableObject.CreateInstance<EnterAssetNamePopup>();
            window.assetName = fileName;
            window.titleContent = new GUIContent("Enter Asset Name");
            window.OnNameEntered = onNameEntered;
            window.minSize = new(400, 100);
            window.ShowUtility();
        }

        private void OnGUI()
        {
            GUILayout.Label("Enter the name for the new asset:", EditorStyles.boldLabel);

            // Input field for the asset name
            assetName = EditorGUILayout.TextField("", assetName);

            bool validName = !assetName.Contains("/");
            if (!validName)
            {
                GUILayout.Label("The input is not a valid name. It should not contain '/' character.");
            }

            // Buttons for confirmation and cancellation
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.Space();
            EditorGUILayout.Space();

            GUI.enabled = validName;
            if (GUILayout.Button("Create"))
            {
                OnNameEntered?.Invoke(assetName);
                Close();
            }
            GUI.enabled = true;

            if (GUILayout.Button("Cancel"))
            {
                Close();
            }
            EditorGUILayout.EndHorizontal();
        }
    }

}