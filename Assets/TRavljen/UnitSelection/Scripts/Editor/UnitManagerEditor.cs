using UnityEngine;

namespace TRavljen.UnitSelection.Editor
{

    using UnityEditor;

    [CustomEditor(typeof(UnitManager))]
    class UnitManagerEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            var manager = target as UnitManager;

            EditorGUILayout.HelpBox("Units are managed internally, but for debugging purposes they can be viewed here.", MessageType.Info);

            EditorGUILayout.Space();
            EditorGUILayout.Space();

            EditorGUILayout.LabelField("List of managed units");
            EditorGUILayout.Space();

            var units = manager.SelectableUnits;
            int index = 0;
            foreach (ISelectable unit in units)
            {
                var gameObject = unit.gameObject;
                EditorGUILayout.ObjectField($"Unit ({index++})", gameObject, typeof(GameObject), true);
            }
        }
    }

}
