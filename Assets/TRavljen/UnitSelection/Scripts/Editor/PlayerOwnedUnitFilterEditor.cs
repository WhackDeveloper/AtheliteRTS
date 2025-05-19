namespace TRavljen.UnitSelection.Editor
{

    using UnityEditor;

    [CustomEditor(typeof(PlayerOwnedUnitFilter))]
    internal class PlayerOwnedUnitFilterEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            EditorGUILayout.HelpBox(
                "Filter managing player units with a UnitOwnership component.",
                MessageType.Info);

            EditorGUILayout.LabelField("Behaviour", EditorStyles.boldLabel);

            EditorGUILayout.HelpBox(
                "Performs custom filtering by taking non player units when selection contains both player and non player units." +
                "\nDisable component, game object, or remove it to disable this filter.",
                MessageType.None);

            EditorGUILayout.Space();
        }
    }

}