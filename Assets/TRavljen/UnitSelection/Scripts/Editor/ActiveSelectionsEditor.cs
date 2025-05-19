using UnityEngine;

namespace TRavljen.UnitSelection.Editor
{
    using UnityEditor;
    using EditorUtility;

    [CustomEditor(typeof(ActiveSelections))]
    public class ActiveSelectionsEditor : Editor
    {

        private SerializedProperty applyMaxActiveSelections;
        private SerializedProperty maxActiveSelections;
        private SerializedProperty supportsGroups;

        private ActiveSelections activeSelections;

        private void OnEnable()
        {
            activeSelections = target as ActiveSelections;

            applyMaxActiveSelections = serializedObject.FindProperty("ApplyMaxActiveSelections");
            maxActiveSelections = serializedObject.FindProperty("MaxActiveSelections");
            supportsGroups = serializedObject.FindProperty("supportsGroups");

            if (Application.isPlaying)
                EditorApplication.update += Repaint;
        }

        private void OnDisable()
        {
            if (Application.isPlaying)
                EditorApplication.update -= Repaint;
        }

        public override void OnInspectorGUI()
        {
            EditorGUILayout.HelpBox("Here you can see debug information in runtime. A list of highlighted and selected objects.", MessageType.Info);
            RenderDebugGUI();
        }

        public void RenderGUI()
        {
            if (serializedObject == null) return;

            if (target is not MonoBehaviour mono ||
                !mono.isActiveAndEnabled)
                return;

            serializedObject.Update();
            EditorGUILayout.PropertyField(applyMaxActiveSelections);

            if (applyMaxActiveSelections.boolValue)
                EditorGUILayout.PropertyField(maxActiveSelections);

            EditorGUILayout.Space();
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.HelpBox("New feature that allows group/squads of units to be selected together as one.", MessageType.Info);
            EditorGUILayout.Space();
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.Space();

            EditorGUILayout.PropertyField(supportsGroups);

            if (serializedObject.ApplyModifiedProperties())
            {
                EditorUtility.SetDirty(target);
            }
        }

        private void RenderDebugGUI()
        {
            // Ignore if its not playing
            if (!Application.isPlaying) return;

            if (PersistentFoldout.Foldout("Selected objects",  groupHeader: false))
            {
                EditorGUI.indentLevel++;
                foreach (var selectable in activeSelections.SelectedUnits)
                {
                    EditorGUILayout.ObjectField(
                        selectable.gameObject,
                        typeof(GameObject),
                        true);
                }
                EditorGUI.indentLevel--;
            }

            if (PersistentFoldout.Foldout("Highlighted objects", groupHeader: false))
            {
                EditorGUI.indentLevel++;
                foreach (var selectable in activeSelections.HighlightedUnits)
                {
                    EditorGUILayout.ObjectField(
                        selectable.gameObject,
                        typeof(GameObject),
                        true);
                }
                EditorGUI.indentLevel--;
            }
        }
    }

}