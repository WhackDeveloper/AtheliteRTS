namespace TRavljen.UnitSelection.Editor
{
    using UnityEngine;
    using UnityEditor;

    [CustomEditor(typeof(SelectableUnit), true)]
    [CanEditMultipleObjects]
    internal class SelectableUnitEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            EditorGUILayout.HelpBox("Adds support for Click and Hover features.", MessageType.Info);

            EditorGUILayout.Space();

            DrawDefaultInspector();

            var unit = (SelectableUnit)target;
            var hasManageObject = unit.TryGetComponent(out ManageUnitObject _);

            if (hasManageObject) return;
            
            EditorGUILayout.Space();

            if (GUILayout.Button("Enable drag selection"))
            {
                unit.gameObject.AddComponent<ManageUnitObject>();
                EditorUtility.SetDirty(target);
            }

            EditorGUILayout.Space();

            EditorGUILayout.HelpBox(
                "Add component required for drag selection and few other features to work." +
                "\nUnless you are managing the 'IUnitManager' yourself.", MessageType.Info);
        }
    }

}
