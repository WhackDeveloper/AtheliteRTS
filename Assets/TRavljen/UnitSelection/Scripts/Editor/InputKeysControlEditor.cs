using System.Collections;
using System.Collections.Generic;

namespace TRavljen.UnitSelection.Editor
{
    using UnityEngine;
    using UnityEditor;

    [CustomEditor(typeof(InputKeysControl))]
    public class InputKeysControlEditor : Editor
    {

        private SerializedProperty modifyCurrentSelectionKey;
        private SerializedProperty cancelSelectionKey;
        private SerializedProperty selectionKey;
        private SerializedProperty quickSaveKey;
        private SerializedProperty quickSelectionKeys;

        private void OnEnable()
        {
            modifyCurrentSelectionKey = serializedObject.FindProperty("modifyCurrentSelectionKey");
            cancelSelectionKey = serializedObject.FindProperty("cancelSelectionKey");
            selectionKey = serializedObject.FindProperty("selectionKey");
            quickSaveKey = serializedObject.FindProperty("quickSaveKey");
            quickSelectionKeys = serializedObject.FindProperty("quickSelectionKeys");
        }

        public override void OnInspectorGUI()
        {
            if (target == null) return;

            if (target is MonoBehaviour mono &&
                mono.isActiveAndEnabled &&
                mono.TryGetComponent(out UnitSelector _))
            {
                EditorGUILayout.HelpBox("You can no longer edit input component attached to Unit Selector.\n" +
                    "Please go to Unit Selector itself and click on Input tab.", MessageType.Info);
            }
            else
            {
                RenderInspectorGUI();
            }
        }

        public void RenderInspectorGUI()
        {
            if (serializedObject == null) return;

            if (target is not MonoBehaviour mono ||
                !mono.isActiveAndEnabled)
                return;

            serializedObject.Update();

            EditorGUILayout.PropertyField(modifyCurrentSelectionKey);
            EditorGUILayout.PropertyField(cancelSelectionKey);
            EditorGUILayout.PropertyField(selectionKey);
            EditorGUILayout.PropertyField(quickSaveKey);
            EditorGUILayout.PropertyField(quickSelectionKeys);

            EditorGUILayout.Space();
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.Space();

            if (GUILayout.Button("Set default keys"))
            {
                InputKeysControl control = target as InputKeysControl;
                control.SetDefaultKeys();
                EditorUtility.SetDirty(target);
            }

            EditorGUILayout.EndHorizontal();

            serializedObject.ApplyModifiedProperties();
        }
    }
}
