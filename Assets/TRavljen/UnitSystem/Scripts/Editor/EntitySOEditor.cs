using System;
using System.Collections.Generic;
using UnityEngine;

namespace TRavljen.UnitSystem.Editor
{

    using UnityEditor;

    [CustomEditor(typeof(AEntitySO), true)]
    [CanEditMultipleObjects]
    public class EntitySOEditor : Editor
    {
        private readonly List<string> mainFilter = new() { "uniqueID", "capabilities" };
        private AEntitySO scriptable;
        private SerializedProperty capabilities;

        private const string selectedTabKey = "AEntitySO.selectedTab";
        private int SelectedTabIndex
        {
            set => EditorPrefs.SetInt(selectedTabKey, value);
            get => EditorPrefs.GetInt(selectedTabKey);
        }

        private void OnEnable()
        {
            scriptable = target as AEntitySO;

            if (scriptable != null && serializedObject.IsNotNull())
            {
                serializedObject.Update();

                capabilities = serializedObject.FindProperty("capabilities");
            }
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            if (SerializedPropertyArrayGUIHelper.CleanupNulls(capabilities))
                // Don't render when there was a cleanup
                return;

            int index = GUILayout.Toolbar(SelectedTabIndex, new string[] { "Main", "Capabilities" });
            SelectedTabIndex = index;
            EditorGUILayout.Space();

            switch (index)
            {
                case 0:
                    // Render all but capabilities and unique ID, both are handled manually.
                    ManagedSOEditorUtility.DrawUniqueIDSection(serializedObject);
                    EditorHelper.RenderProperties(serializedObject, mainFilter);
                    break;

                case 1:
                    RenderAddComponentGUI();
                    EditorGUILayout.Space();
                    RenderCapabilities();
                    break;
            }

            if (serializedObject.ApplyModifiedProperties())
            {
                EditorUtility.SetDirty(target);
            }
        }

        private void RenderCapabilities()
        {
            var capabilitiesReference = scriptable.Capabilities;
            SerializedPropertyArrayGUIHelper.RenderObjects(capabilitiesReference, capabilities, target);
        }

        private void RenderAddComponentGUI()
        {
            // Potentially separate these, getting lots of them and hard to tell why any.
            var capabilities = scriptable.Capabilities;

            if (target is not AUnitSO)
            {
                SerializedPropertyArrayGUIHelper.ShowAddObjectMenu<IEntityCapability, IUnitCapability>("Add Capability", capabilities, type =>
                {
                    AddNewCapability(type);
                },
                null);
            }
            else
            {
                SerializedPropertyArrayGUIHelper.ShowAddObjectMenu<IEntityCapability>("Add Capability", capabilities, type =>
                {
                    AddNewCapability(type);
                },
                null);
            }

            EditorGUILayout.Space();
        }

        private void AddNewCapability(Type type)
        {
            var newInstance = Activator.CreateInstance(type) as IEntityCapability;
            newInstance.SetDefaultValues();
            scriptable.AddCapability(newInstance);

            // Set Dirty
            if (!serializedObject.ApplyModifiedProperties()) return;
            
            EditorUtility.SetDirty(target);
            Undo.RecordObject(target, "Capability" + type.ToString() + " added");
        }
    }

}