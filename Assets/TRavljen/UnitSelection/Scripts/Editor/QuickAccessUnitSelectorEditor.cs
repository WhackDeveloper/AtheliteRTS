using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TRavljen.EditorUtility;
namespace TRavljen.UnitSelection.Editor
{

    using UnityEditor;
    [CustomEditor(typeof(QuickAccessUnitSelector))]
    public class QuickAccessUnitSelectorEditor : Editor
    {

        private SerializedProperty enabled;

        private QuickAccessUnitSelector selector;

        private void OnEnable()
        {
            enabled = serializedObject.FindProperty("EnableQuickAccess");
            selector = target as QuickAccessUnitSelector;
        }

        public override void OnInspectorGUI()
        {
            EditorGUILayout.HelpBox("Here you can see debug information in runtime. Lists of saved units for quick selection.", MessageType.Info);
            RenderDebugGUI();
        }

        public void RenderGUI()
        {
            serializedObject.Update();
            EditorGUILayout.PropertyField(enabled, new GUIContent("Enabled"));
            if (serializedObject.ApplyModifiedProperties() && !Application.isPlaying)
            {
                EditorUtility.SetDirty(this);
            }
        }

        private void RenderDebugGUI()
        {
            // Ignore if its not playing
            if (!Application.isPlaying) return;

            var selections = selector.GetSavedSelections();

            if (PersistentFoldout.Foldout("Saved objects"))
            {
                EditorGUI.indentLevel++;
                foreach (var savedSelection in selections)
                {
                    string title = string.Format("Units under action index {0}", savedSelection.Key);
                    EditorGUILayout.LabelField(title, EditorStyles.boldLabel);

                    foreach (var selectable in savedSelection.Value)
                    {
                        EditorGUILayout.ObjectField(
                        selectable.gameObject,
                        typeof(GameObject),
                        true);
                    }
                }
                EditorGUI.indentLevel--;
            }

        }
    }

}