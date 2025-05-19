using UnityEngine;

namespace TRavljen.UnitSystem.Editor
{
    using UnityEditor;

    /// <summary>
    /// Utility class for rendering additional sections in the Inspector for <see cref="ManagedSO"/> assets.
    /// </summary>
    /// <remarks>
    /// Handles the display and management of unique IDs for ManagedSO objects, including registration in the
    /// <see cref="SystemDatabase"/>.
    /// </remarks>
    public static class ManagedSOEditorUtility
    {
        /// <summary>
        /// Draws the unique ID section in the Inspector for a <see cref="ManagedSO"/> asset.
        /// </summary>
        /// <param name="serializedObject">
        /// The serialized object representing the <see cref="ManagedSO"/> being edited.
        /// </param>
        public static void DrawUniqueIDSection(SerializedObject serializedObject)
        {
            SerializedProperty idProperty = serializedObject.FindProperty("uniqueID");
            
            if (serializedObject.targetObject is not ManagedSO managed) return; 

            if (!SystemDatabase.GetInstance().IsRegistered(managed))
            {
                // Warning message for unregistered assets
                EditorGUILayout.HelpBox(
                    "Registering in DB will make the object accessible in the SystemDatabase and assign it a unique ID. " +
                    "This is required for use of built-in UnitSystem modules. " +
                    "If the asset is created within the UnitSystem Manager, this will be handled automatically upon creation.",
                    MessageType.Warning
                );

                // Button to register in the database
                if (GUILayout.Button("Register in SystemDatabase"))
                {
                    SystemDatabase.GetInstance().Register(managed);

                    // Apply modified properties and refresh the serialized object
                    serializedObject.Update();

                    Debug.Log($"Assigned new unique ID: {idProperty.intValue}");

                    EditorUtility.SetDirty(serializedObject.targetObject);
                }
            }
            else
            {
                // Display the unique ID (read-only)
                GUI.enabled = false;
                EditorGUILayout.TextField("Unique ID", idProperty.intValue.ToString());
                GUI.enabled = true;
                
                GUILayout.BeginHorizontal();
                GUILayout.FlexibleSpace();
                
                if (GUILayout.Button("Remove from SystemDatabase"))
                {
                    SystemDatabase.GetInstance().Remove(managed);
                }
                
                GUILayout.EndHorizontal();
            }
        }
    }

}