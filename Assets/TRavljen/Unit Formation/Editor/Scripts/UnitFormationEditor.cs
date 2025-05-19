namespace TRavljen.UnitFormation.Editor
{
    using UnityEditor;

    [CustomEditor(typeof(UnitFormation))]
    public class UnitFormationEditor : Editor
    {

        SerializedProperty placeOnGround;
        SerializedProperty maxGroundDistance;
        SerializedProperty noiseEnabled;
        SerializedProperty units;

        private void OnEnable()
        {
            placeOnGround = serializedObject.FindProperty("placeOnGround");
            maxGroundDistance = serializedObject.FindProperty("maxGroundDistance");
            noiseEnabled = serializedObject.FindProperty("noiseEnabled");
            units = serializedObject.FindProperty("units");
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            EditorGUILayout.PropertyField(placeOnGround);
            if (placeOnGround.boolValue)
            {
                EditorGUI.indentLevel++;
                EditorGUILayout.PropertyField(maxGroundDistance);
                EditorGUI.indentLevel--;
            }

            EditorGUILayout.PropertyField(noiseEnabled);

            EditorGUILayout.PropertyField(units);

            serializedObject.ApplyModifiedProperties();
        }
    }
}