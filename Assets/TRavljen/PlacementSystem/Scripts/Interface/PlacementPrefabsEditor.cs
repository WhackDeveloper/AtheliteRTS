#if UNITY_EDITOR
namespace TRavljen.PlacementSystem
{
    using UnityEngine;
    using UnityEditor;

    [CustomEditor(typeof(PlacementPrefabs))]
    class PlacementPrefabsEditor : UnityEditor.Editor
    {
        SerializedProperty prefabs;
        SerializedProperty cyclesRotation;

        private void OnEnable()
        {
            prefabs = serializedObject.FindProperty("prefabs");
            cyclesRotation = serializedObject.FindProperty("cyclesRotation");
        }

        public override void OnInspectorGUI()
        {
            EditorGUILayout.PropertyField(cyclesRotation);

            EditorGUILayout.Space();
            EditorGUILayout.HelpBox("Drag your prefabs below", MessageType.None);
            EditorGUILayout.PropertyField(prefabs);

            if (serializedObject.ApplyModifiedProperties())
            {
                // Record undo and mark object as dirty
                Undo.RecordObject(target, "Modify Object Placement Configuration");

                // Do this only on Editor component when in app is not running,
                // cannot mark runtime instantiated objects as dirty.
                if (!Application.isPlaying)
                {
                    EditorUtility.SetDirty(target);
                }
            }
        }
    }
}
#endif