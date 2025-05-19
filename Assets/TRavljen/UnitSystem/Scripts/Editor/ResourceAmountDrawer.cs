using UnityEngine;

namespace TRavljen.UnitSystem.Editor
{
    using UnityEditor;

    [CustomPropertyDrawer(typeof(ResourceQuantity))]
    public class ResourceAmountDrawer : ScriptableValuePropertyDrawer
    {

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            scriptableProperty = property.FindPropertyRelative("Resource");
            valueProperty = property.FindPropertyRelative("Quantity");

            base.OnGUI(position, property, label);
        }

    }

}