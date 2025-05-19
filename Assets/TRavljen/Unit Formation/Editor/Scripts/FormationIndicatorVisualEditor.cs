using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TRavljen.UnitFormation.Editor
{
    using UnityEditor;
    using Placement;

    [CustomEditor(typeof(FormationIndicatorVisual))]
    public class FormationIndicatorVisualEditor : Editor
    {
        SerializedProperty unitIndicatorPrefab;
        SerializedProperty hideWithDelay;
        SerializedProperty hideDelay;

        private void OnEnable()
        {
            unitIndicatorPrefab = serializedObject.FindProperty("unitIndicatorPrefab");
            hideWithDelay = serializedObject.FindProperty("hideWithDelay");
            hideDelay = serializedObject.FindProperty("hideDelay");
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            EditorGUILayout.PropertyField(unitIndicatorPrefab);

            EditorGUILayout.PropertyField(hideWithDelay);

            if (hideWithDelay.boolValue)
            {
                EditorGUILayout.PropertyField(hideDelay);
            }

            serializedObject.ApplyModifiedProperties();
        }
    }
}