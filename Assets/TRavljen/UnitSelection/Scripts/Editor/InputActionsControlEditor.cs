using System.Collections;
using System.Collections.Generic;

#if ENABLE_INPUT_SYSTEM
namespace TRavljen.UnitSelection.Editor
{
    using UnityEngine;
    using UnityEditor;

    [CustomEditor(typeof(InputActionsControl))]
    public class InputActionsControlEditor : Editor
    {

        private SerializedProperty actions;

        private void OnEnable()
        {
            actions = serializedObject.FindProperty("actions");
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

            EditorGUILayout.PropertyField(actions);

            EditorGUILayout.Space();
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.Space();

            if (GUILayout.Button("Set default actions"))
            {
                InputActionsControl control = target as InputActionsControl;
                control.SetDefaultActions();
                EditorUtility.SetDirty(target);
            }

            EditorGUILayout.EndHorizontal();

            serializedObject.ApplyModifiedProperties();
        }
    }
}

#endif