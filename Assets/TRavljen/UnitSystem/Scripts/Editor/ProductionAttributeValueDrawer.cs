namespace TRavljen.UnitSystem.Editor
{
    using UnityEditor;
    using UnityEngine;

    [CustomPropertyDrawer(typeof(ProductionAttributeValue))]
    public sealed class ProductionAttributeValueDrawer : ScriptableValuePropertyDrawer
    {

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            scriptableProperty = property.FindPropertyRelative("Attribute");
            valueProperty = property.FindPropertyRelative("Value");

            base.OnGUI(position, property, label);
        }

        public override void OnGUIValueWithoutReference(Rect position)
        {
            EditorGUI.LabelField(position, new GUIContent("No attribute"));
        }

    }

}
