using TRavljen.UnitSelection;

namespace TRavljen.UnitSelection.Editor
{
    using UnityEditor;
    [CustomEditor(typeof(PlayerUnitListFilter))]
    internal class PlayerUnitListFilterEditor : Editor
    {

        SerializedProperty list;

        private void OnEnable()
        {
            list = serializedObject.FindProperty("playerUnits");
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            EditorGUILayout.HelpBox(
                "Filter managing reference of player units with a simple list.",
                MessageType.Info);

            EditorGUILayout.LabelField("Behaviour", EditorStyles.boldLabel);

            EditorGUILayout.HelpBox(
                "Performs custom filtering by taking out enemy units when selection contains both enemy and friendly units." +
                "\nDisable this component or game object to disable this filter." +
                "\n\nWith this component you can specify player units list in editor and then " +
                "continue managing the unit list during game loop, in case new " +
                "unit is spawned by the player.",
                MessageType.None);

            EditorGUILayout.Space();

            EditorGUILayout.PropertyField(list);

            serializedObject.ApplyModifiedProperties();
        }
    }

}