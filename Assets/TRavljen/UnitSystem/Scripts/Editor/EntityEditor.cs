namespace TRavljen.UnitSystem.Editor
{
    using UnityEditor;
    using UnityEngine;

    /// <summary>
    /// Custom Editor for the <see cref="Entity"/> class and its derivatives.
    /// </summary>
    /// <remarks>
    /// This editor enhances the Unity Inspector for objects derived from <see cref="Entity"/>.
    /// It adds buttons and tools for managing entity components directly in the Inspector, 
    /// including an option to open the UnitSystem Manager for scriptable object-based management.
    ///
    /// While entity components can be configured and managed via scriptable objects, 
    /// this editor provides a fallback mechanism to add or manage components manually.
    /// </remarks>
    [CustomEditor(typeof(Entity), true)]
    [CanEditMultipleObjects]
    internal class EntityEditor : Editor
    {
        /// <summary>
        /// The currently selected <see cref="Entity"/> being edited.
        /// </summary>
        private Entity entity;

        /// <summary>
        /// Called when the editor is enabled. Initializes the <see cref="Entity"/> reference.
        /// </summary>
        private void OnEnable()
        {
            entity = target as Entity;
        }

        /// <summary>
        /// Renders the custom Inspector GUI for the Entity.
        /// Includes default property rendering, component management, and a button to open the UnitSystem Manager.
        /// </summary>
        public override void OnInspectorGUI()
        {
            // Render the default Inspector fields
            base.OnInspectorGUI();

            // Add custom component management GUI
            RenderAddComponentGUI();
        }

        /// <summary>
        /// Renders the GUI for managing <see cref="IEntityComponent"/> types associated with the Entity.
        /// </summary>
        private void RenderAddComponentGUI()
        {
            // Get all existing IEntityComponent instances on the entity
            var components = entity.GetComponentsInChildren<IEntityComponent>();

            EditorGUILayout.LabelField("Entity Components", EditorStyles.boldLabel);

            // Inform users about scriptable object-based management
            EditorGUILayout.HelpBox(
                "Components can be configured and managed for prefabs through the UnitSystem Manager and its scriptable objects.",
                MessageType.Info);

            // Provide a button to open the UnitSystem Manager
            if (GUILayout.Button("Open Manager Window"))
            {
                UnitSystemManagerWindow.ShowWindow();
            }

            // Add options for manually adding new IEntityComponent types
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.Space();
            EditorGUILayout.BeginVertical(GUILayout.Width(250));

            SerializedPropertyArrayGUIHelper.ShowAddObjectMenu<IEntityComponent>(
                "Add Entity Component",
                components,
                type =>
                {
                    // Add the new component via Undo system for editor tracking
                    var component = Undo.AddComponent(entity.gameObject, type);

                    if (component != null)
                    {
                        // Mark the entity GameObject and Entity object dirty for Unity to save changes
                        EditorUtility.SetDirty(entity.gameObject);
                        EditorUtility.SetDirty(entity);
                    }

                    serializedObject.ApplyModifiedProperties();
                },
                null);

            EditorGUILayout.EndVertical();
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.Space();
        }
    }

}