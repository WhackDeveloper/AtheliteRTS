using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TRavljen.UnitSystem.Editor
{
    using UnityEditor;

    [CustomEditor(typeof(SystemDatabase))]
    class SystemDatabaseEditor : Editor
    {

        private SerializedProperty list;

        private void OnEnable()
        {
            list = serializedObject.FindProperty("list");
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            EditorGUILayout.HelpBox("To properly manage objects in this database use manager that can be found in menu under TRavljen/UnitSystem Manager", MessageType.Warning);

            GUILayout.BeginHorizontal();

            EditorGUILayout.Space();

            if (GUILayout.Button("Open Manager Window"))
            {
                UnitSystemManagerWindow.ShowWindow();
            }

            GUILayout.EndHorizontal();

            GUIStyle style = new(EditorStyles.foldoutHeader)
            {
                font = EditorStyles.boldFont
            };
            list.isExpanded = EditorGUILayout.Foldout(list.isExpanded, "Items in Database", true, style);

            if (list.isExpanded)
            {

                for (int index = 0; index < list.arraySize; index++)
                {
                    var element = list.GetArrayElementAtIndex(index);
                    var obj = element.objectReferenceValue as ManagedSO;

                    EditorGUILayout.BeginHorizontal();

                    EditorGUILayout.Space(8, false);


                    if (obj == null)
                    {
                        // Show missing asset
                        EditorGUILayout.LabelField("NULL (Missing asset)");

                        if (GUILayout.Button("Remove entry"))
                        {
                            var database = target as SystemDatabase;
                            database.RemoveMissingAssets();
                            break;
                        }
                    }
                    else
                    {
                        // Expand/foldout item data
                        string header = obj.ID.ToString();

                        // Asset reference
                        EditorGUILayout.PropertyField(element, new GUIContent(header));
                    }

                    EditorGUILayout.EndHorizontal();
                }

            }

            EditorGUILayout.Space();

            RenderButtons();

            serializedObject.ApplyModifiedProperties();
        }

        private bool RenderButtons()
        {
            EditorGUILayout.LabelField("Manage assets", EditorStyles.boldLabel);

            bool pressed = false;

            EditorGUILayout.BeginVertical("box");
            EditorGUILayout.HelpBox("You can register all the assets within the project which subclass <ManagedSO> type here.", MessageType.Info);
            if (GUILayout.Button("Register all within the project"))
            {
                ManagedSOTools.AddAllAssetsToDatabase();
                pressed = true;
            }
            EditorGUILayout.EndVertical();

            EditorGUILayout.BeginVertical("box");
            EditorGUILayout.HelpBox("You can remove all the assets in the database here.", MessageType.Info);
            if (GUILayout.Button("Clear database"))
            {
                (target as SystemDatabase).Reset();
                pressed = true;
            }
            EditorGUILayout.EndVertical();

            return pressed;
        }
    }

}
