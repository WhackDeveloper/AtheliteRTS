using TRavljen.EditorUtility;
using UnityEngine;

namespace TRavljen.UnitSelection.Editor
{
    using UnityEditor;

    [CustomEditor(typeof(ManageUnitObject))]
    [CanEditMultipleObjects]
    internal class ManageUnitObjectEditor : HiddenScriptPropertyEditor
    {
        public override void OnInspectorGUI()
        {
            RenderInspectorGUI("Adds support for Drag Selection and few other Multi-Selection features.");
        }

        public void RenderInspectorGUI(string mainMessage)
        {
            var managedUnit = target as ManageUnitObject;

            bool containsSelectableComponent = managedUnit.TryGetComponent(out ISelectable _);

            if (containsSelectableComponent)
            {
                EditorGUILayout.HelpBox(mainMessage, MessageType.Info);
            }
            else
            {
                EditorGUILayout.HelpBox(
                    "This component does nothing if there is no component on this game object which implements ISelectable interface, like SelectableUnit.",
                    MessageType.Error);

                if (GUILayout.Button("Fix error"))
                {
                    managedUnit.gameObject.AddComponent<SelectableUnit>();
                }

                if (GUILayout.Button("Remove component"))
                {
                    DestroyImmediate(managedUnit);
                    // Should not continue with the code if component is destroyed.
                    return;
                }
            }

            EditorGUILayout.Space();

            DrawDefaultInspector();
        }
    }
}
