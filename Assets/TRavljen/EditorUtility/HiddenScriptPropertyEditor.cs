#if UNITY_EDITOR
namespace TRavljen.EditorUtility
{

    using UnityEditor;

    /// <summary>
    /// Simple editor script that hides the reference to the original script
    /// file for the component that uses this Editor.
    /// </summary>
    public class HiddenScriptPropertyEditor : Editor
    {

        public override void OnInspectorGUI()
        {
            // Get the serialized properties
            serializedObject.Update();
            SerializedProperty property = serializedObject.GetIterator();
            property.NextVisible(true);

            // Iterate through all properties and draw them except the script reference
            while (property.NextVisible(false))
            {
                // Show all but m_Script properties.
                if (property.name != "m_Script")
                {
                    EditorGUILayout.PropertyField(property, true);
                }
            }

            // Apply changes to the serialized object
            serializedObject.ApplyModifiedProperties();
        }
    }
    
}
#endif
