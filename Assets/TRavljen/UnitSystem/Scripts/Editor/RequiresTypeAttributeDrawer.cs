namespace TRavljen.UnitSystem.Editor
{
    using System;
    using UnityEditor;
    using UnityEngine;

    /// <summary>
    /// Custom property drawer for <see cref="RequiresTypeAttribute"/>.
    /// </summary>
    /// <remarks>
    /// This drawer enforces that a serialized field in the Unity Inspector 
    /// is assigned a reference to a component or object that implements a specified 
    /// interface or inherits a specific type, as defined by the <see cref="RequiresTypeAttribute"/>.
    /// Additionally, it provides a visual indicator showing the required type in the Inspector.
    /// </remarks>
    [CustomPropertyDrawer(typeof(RequiresTypeAttribute))]
    public class RequiresTypeDrawer : PropertyDrawer
    {
        // Define a maximum width for the PropertyField
        private float MaxPropertyFieldWidth = 150f;

        /// <summary>
        /// Called by Unity to render the property in the Inspector.
        /// </summary>
        /// <param name="position">The rectangle on the screen to use for the property GUI.</param>
        /// <param name="property">The <see cref="SerializedProperty"/> to make the custom GUI for.</param>
        /// <param name="label">The label of the property field.</param>
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            MaxPropertyFieldWidth = position.width * 0.6f;
            EditorGUI.BeginProperty(position, label, property);

            Type interfaceType = ((RequiresTypeAttribute)attribute).InterfaceType;

            // Labels for text and colored text
            GUIContent mainLabel = new GUIContent(label.text);
            GUIContent coloredLabel = new GUIContent($" ({interfaceType.Name})");

            // Rich text style for colored label
            GUIStyle richTextStyle = new GUIStyle(EditorStyles.label) { richText = true, normal = { textColor = Color.red } };

            // Calculate sizes for labels
            Vector2 mainLabelSize = EditorStyles.label.CalcSize(mainLabel);
            Vector2 coloredLabelSize = richTextStyle.CalcSize(coloredLabel);

            // Calculate total width required by both labels
            float labelsWidth = mainLabelSize.x + coloredLabelSize.x;

            // Set the PropertyField's width: take either the max width or remaining space
            float propertyFieldWidth = Mathf.Min(MaxPropertyFieldWidth, position.width - labelsWidth - 5);
            float propertyFieldX = position.x + position.width - propertyFieldWidth;

            // Draw the PropertyField on the right, with a maximum width
            Rect propertyRect = new Rect(propertyFieldX, position.y, propertyFieldWidth, position.height);
            EditorGUI.PropertyField(propertyRect, property, GUIContent.none);

            // Draw the main and colored labels to the left of the PropertyField
            float mainLabelWidth = Mathf.Min(mainLabelSize.x, propertyFieldX - position.x - coloredLabelSize.x);
            float coloredLabelX = position.x + mainLabelWidth;

            EditorGUI.LabelField(new Rect(position.x, position.y, mainLabelWidth, position.height), mainLabel);
            EditorGUI.LabelField(new Rect(coloredLabelX, position.y, coloredLabelSize.x, position.height), coloredLabel, richTextStyle);

            // Enforces the correct property type.
            ValidatePropertyType(property);

            EditorGUI.EndProperty();
        }

        /// <summary>
        /// Validates that the specified property value conforms to the required type or interface.
        /// If the property value is not valid, it will be cleared (set to null).
        /// </summary>
        /// <param name="property">The serialized property to validate.</param>
        /// <remarks>
        /// This method ensures that the object assigned to the property:
        /// - Implements the type or interface specified in the <see cref="RequiresTypeAttribute"/>.
        /// - Is a <see cref="Component"/> directly implementing the required interface or type.
        /// - Is a <see cref="GameObject"/> containing a component that implements the required interface or type.
        /// If the validation fails, the property value is cleared, and a warning is logged in the Unity Editor.
        /// </remarks>
        private void ValidatePropertyType(SerializedProperty property)
        {
            if (property.objectReferenceValue != null)
            {
                // Get the type defined in the attribute
                Type interfaceType = ((RequiresTypeAttribute)attribute).InterfaceType;

                // Check if the reference is a Component
                if (property.objectReferenceValue is Component component)
                {
                    // Validate the Component's type
                    if (!interfaceType.IsAssignableFrom(component.GetType()))
                    {
                        Debug.LogWarning($"Assigned object does not implement {interfaceType.Name}. Clearing the reference.");
                        property.objectReferenceValue = null;
                    }
                }
                // Check if the reference is a GameObject
                else if (property.objectReferenceValue is GameObject gameObject)
                {
                    // Check if any component on the GameObject matches the required type
                    var matchingComponent = gameObject.GetComponent(interfaceType);

                    if (matchingComponent == null)
                    {
                        Debug.LogWarning($"Assigned GameObject does not have a component implementing {interfaceType.Name}. Clearing the reference.");
                        property.objectReferenceValue = null;
                    }
                }
                else
                {
                    // Reference is neither a Component nor a GameObject, clear it
                    Debug.LogWarning($"Assigned object is neither a Component nor a GameObject. Clearing the reference.");
                    property.objectReferenceValue = null;
                }
            }
        }


        /// <summary>
        /// Determines the height of the property in the Inspector.
        /// </summary>
        /// <param name="property">The <see cref="SerializedProperty"/> for which the height is being calculated.</param>
        /// <param name="label">The label of the property field.</param>
        /// <returns>The height of the property field in pixels.</returns>
        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            return EditorGUIUtility.singleLineHeight;
        }
    }

}