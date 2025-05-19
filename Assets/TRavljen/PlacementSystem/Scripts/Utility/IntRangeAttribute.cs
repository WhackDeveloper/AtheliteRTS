using UnityEngine;

namespace TRavljen.Utility
{

    public class IntRangeAttribute : PropertyAttribute
    {
        public int Min { get; }
        public int Max { get; }

        public IntRangeAttribute(int min, int max)
        {
            Min = min;
            Max = max;
        }
    }

}

#if UNITY_EDITOR
namespace TRavljen.Utility
{
    using UnityEditor;

    #region CustomPropertyDrawer

    [CustomPropertyDrawer(typeof(IntRangeAttribute))]
    public class IntRangeAttributeDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            IntRangeAttribute range = attribute as IntRangeAttribute;

            // Find the properties of the struct
            SerializedProperty minProp = property.FindPropertyRelative("min");
            SerializedProperty maxProp = property.FindPropertyRelative("max");

            EditorGUI.BeginProperty(position, label, property);
            position = EditorGUI.PrefixLabel(position, GUIUtility.GetControlID(FocusType.Passive), label);

            // Start checking for changes
            EditorGUI.BeginChangeCheck();

            // Set up rectangles
            float padding = 6;
            var minFieldRect = new Rect(position.x, position.y, position.width / 4, position.height);
            var maxFieldRect = new Rect(position.x + position.width * 3 / 4, position.y, position.width / 4, position.height);
            var sliderRect = new Rect(position.x + position.width / 4 + padding, position.y, position.width / 2 - padding * 2, position.height);

            // Draw fields and slider
            EditorGUI.PropertyField(minFieldRect, minProp, GUIContent.none);
            int originalMin = minProp.intValue;
            int originalMax = maxProp.intValue;

            float minValue = originalMin;
            float maxValue = originalMax;

            EditorGUI.MinMaxSlider(sliderRect, ref minValue, ref maxValue, range.Min, range.Max);
            EditorGUI.PropertyField(maxFieldRect, maxProp, GUIContent.none);

            if (EditorGUI.EndChangeCheck())
            {
                // Clamp values from fields.
                int minimalMaxValue = Mathf.Max(range.Min, minProp.intValue) + 1;
                minimalMaxValue = Mathf.Min(minimalMaxValue, maxProp.intValue);
                maxProp.intValue = Mathf.Clamp(maxProp.intValue, minimalMaxValue, range.Max);
                minProp.intValue = Mathf.Clamp(minProp.intValue, range.Min, Mathf.Min(range.Max, maxProp.intValue) - 1);
            }

            // Update values from slider only if they changed,
            // otherwise keep the field values if they were changed.
            int newSliderMinValue = (int)minValue;
            int newSliderMaxValue = (int)maxValue;

            if (originalMax != newSliderMaxValue)
                maxProp.intValue = (int)maxValue;
            if (originalMin != newSliderMinValue)
                minProp.intValue = (int)minValue;

            EditorGUI.EndProperty();
        }
    }

    #endregion
}
#endif
