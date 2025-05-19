namespace TRavljen.UnitSystem.Editor
{

    using UnityEditor;

    /// <summary>
    /// Custom editor for <see cref="ManagedSO"/> and its subclasses.
    /// </summary>
    /// <remarks>
    /// Enhances the Inspector interface for ManagedSO assets by displaying a unique ID section
    /// and rendering all other properties except those explicitly ignored.
    /// </remarks>
    [CustomEditor(typeof(ManagedSO), true)]
    [CanEditMultipleObjects]
    public class ManagedSOEditor : Editor
    {
        /// <summary>
        /// Draws the Inspector GUI for the <see cref="ManagedSO"/> asset.
        /// </summary>
        public override void OnInspectorGUI()
        {
            serializedObject.UpdateIfRequiredOrScript();

            EditorGUILayout.Space();

            // Draw unique ID management section
            ManagedSOEditorUtility.DrawUniqueIDSection(serializedObject);

            // Render all properties except those specified
            EditorHelper.RenderProperties(serializedObject, new() { "uniqueID" });

            if (serializedObject.ApplyModifiedProperties())
            {
                EditorUtility.SetDirty(target);
            }
        }
    }

}