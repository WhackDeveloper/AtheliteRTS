using UnityEngine;

namespace TRavljen.UnitSystem.Editor
{
    using UnityEditor;

    /// <summary>
    /// A custom property drawer for scriptable value properties. 
    /// This drawer renders two fields side-by-side: one for a ScriptableObject reference 
    /// and another for its associated value.
    /// </summary>
    public class ScriptableValuePropertyDrawer : PropertyDrawer
    {

        /// <summary>
        /// The SerializedProperty representing the ScriptableObject reference.
        /// </summary>
        protected SerializedProperty scriptableProperty;

        /// <summary>
        /// The SerializedProperty representing the associated value of the ScriptableObject.
        /// </summary>
        protected SerializedProperty valueProperty;

        #region Main GUI Rendering

        /// <summary>
        /// Overrides the OnGUI method to define how the property is rendered in the Inspector.
        /// </summary>
        /// <param name="position">The position rect for the property GUI.</param>
        /// <param name="property">The SerializedProperty being drawn.</param>
        /// <param name="label">The label of the property.</param>
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            // Start a property field
            EditorGUI.BeginProperty(position, label, property);

            position = EditorGUI.PrefixLabel(position, GUIUtility.GetControlID(FocusType.Passive), label);

            var indent = EditorGUI.indentLevel;
            EditorGUI.indentLevel = 0;

            // Calculate rects for each field
            float scriptableWidth = position.width * (2 / 4f);
            float valueWidth = position.width * (2 / 4f);
            Rect scriptableRect = new(position.x, position.y, scriptableWidth, position.height);
            Rect valueRect = new(position.x + scriptableWidth + 5, position.y, valueWidth - 5, position.height);

            EditorGUI.PropertyField(scriptableRect, scriptableProperty, GUIContent.none);

            if (scriptableProperty.objectReferenceValue != null)
            {
                float halfWidth = valueWidth / 2;
                valueRect.width = halfWidth - 5;
                GUIStyle style = new GUIStyle(EditorStyles.label);
                style.alignment = TextAnchor.MiddleRight;
                EditorGUI.LabelField(valueRect, valueProperty.displayName, style);

                valueRect.x += halfWidth;
                EditorGUI.PropertyField(valueRect, valueProperty, GUIContent.none);
            }
            else
            {
                ResetValue(valueProperty);

                OnGUIValueWithoutReference(valueRect);
            }

            // Restore indent level and end property
            EditorGUI.indentLevel = indent;
            EditorGUI.EndProperty();
        }

        #endregion

        #region Main GUI Rendering

        /// <summary>
        /// Overrides the OnGUI method to define how the property is rendered in the Inspector.
        /// </summary>
        /// <param name="position">The position rect for the property GUI.</param>
        /// <param name="property">The SerializedProperty being drawn.</param>
        /// <param name="label">The label of the property.</param>
        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
            => EditorGUIUtility.singleLineHeight;

        #endregion

        #region Reset and Fallback Behavior

        /// <summary>
        /// Renders a fallback message when the ScriptableObject reference is not set.
        /// </summary>
        /// <param name="position">The position rect for the fallback message.</param>
        public virtual void OnGUIValueWithoutReference(Rect position)
        {
            EditorGUI.LabelField(position, new GUIContent("Not set"));
        }

        /// <summary>
        /// Resets the value of the associated SerializedProperty to its default based on its type.
        /// </summary>
        /// <param name="valueProperty">The SerializedProperty to reset.</param>
        public virtual void ResetValue(SerializedProperty valueProperty)
        {
            switch (valueProperty.propertyType)
            {
                case SerializedPropertyType.Integer:
                    valueProperty.intValue = 0;
                    break;

                case SerializedPropertyType.Float:
                    valueProperty.floatValue = 0f;
                    break;

                case SerializedPropertyType.Generic: // If it could hold long or custom types
                    valueProperty.longValue = 0;
                    break;

                default:
                    Debug.LogWarning($"Unhandled SerializedPropertyType: {valueProperty.propertyType}");
                    break;
            }
        }

        #endregion
    }

}