using System.Collections.Generic;
using UnityEditor;

namespace TRavljen.UnitSystem.Editor
{

    /// <summary>
    /// Helper class for rendering serialized properties in custom editors.
    /// </summary>
    /// <remarks>
    /// Provides utility methods for rendering all properties of a serialized object,
    /// with support for excluding specific properties by name.
    /// </remarks>
    public static class EditorHelper
    {

        /// <summary>
        /// Renders all properties of a serialized object, excluding those specified in the ignore list.
        /// </summary>
        /// <param name="serializedObject">The serialized object to render properties for.</param>
        /// <param name="properties">A list of property names to use for filtering.</param>
        /// <param name="ignoring">Should list of names be used for ignoring or using those properties.</param>
        public static void RenderProperties(SerializedObject serializedObject, List<string> properties, bool ignoring = true)
        {
            SerializedProperty property = serializedObject.GetIterator();
            property.NextVisible(true);

            // Iterate through all properties and draw them except the script reference
            while (property.NextVisible(false))
            {
                if (ignoring && properties.Contains(property.name) == false)
                {
                    EditorGUILayout.PropertyField(property, true);
                }
                else if (!ignoring && properties.Contains(property.name) == true)
                {
                    EditorGUILayout.PropertyField(property, true);
                }
            }
        }

    }

}