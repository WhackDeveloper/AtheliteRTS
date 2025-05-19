using UnityEngine;
using UnityEditor;

namespace TRavljen.UnitSystem.Editor
{

    [CustomPropertyDrawer(typeof(ProducibleQuantity))]
    public class ProducibleQuantityDrawer : ScriptableValuePropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            scriptableProperty = property.FindPropertyRelative("Producible");
            valueProperty = property.FindPropertyRelative("Quantity");

            base.OnGUI(position, property, label);
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
            => EditorGUIUtility.singleLineHeight;

    }

}