using System;
using System.Collections.Generic;
using UnityEngine;

namespace TRavljen.UnitSystem.Editor
{
    
    using UnityEditor;
    
    [CustomEditor(typeof(APlayer), true)]
    public class APlayerEditor : Editor
    {

        private readonly List<string> events = new() {
            "OnUnitAdded", "OnUnitRemoved", "OnEntityAdded", "OnEntityRemoved", "OnRegisterProducible", "OnUnregisterProducible"
        };

        private SerializedProperty modules;
        private Player player;

        private const string selectedTabKey = "Player.selectedTab";
        protected int SelectedTabIndex
        {
            set => EditorPrefs.SetInt(selectedTabKey, value);
            get => EditorPrefs.GetInt(selectedTabKey);
        }

        protected virtual List<string> GetEvents() => events;

        private void OnEnable()
        {
            player = target as Player;

            modules = serializedObject.FindProperty("modules");
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            var index = GUILayout.Toolbar(SelectedTabIndex, new string[] { "Data", "Modules", "Events" });
            SelectedTabIndex = index;

            EditorGUILayout.Space(12);

            if (index == 0)
            {
                RenderMainGUI();
            }
            else if (index == 1)
            {
                RenderAddModuleGUI();
                RenderModulesGUI();
            }
            else if (index == 2)
            {
                RenderEventsGUI();
            }

            if (serializedObject.ApplyModifiedProperties())
            {
                // Record undo and mark object as dirty
                Undo.RecordObject(target, "Modify Player");
                EditorUtility.SetDirty(target);
            }
        }

        private void RenderMainGUI()
        {
            var filter = GetEvents();
            filter.Add("modules");
            EditorHelper.RenderProperties(serializedObject, filter);
        }

        private void RenderAddModuleGUI()
        {
            SerializedPropertyArrayGUIHelper.ShowAddObjectMenu<APlayerModule>(
                "Add new module", player.Modules, AddModuleOfType, types => {
                foreach (var type in types)
                    AddModuleOfType(type);
                });
        }

        private void RenderModulesGUI()
        {
            EditorGUILayout.Space(12);
            SerializedPropertyArrayGUIHelper.RenderObjects(player.Modules, modules, target);
            EditorGUILayout.Space();
        }

        private void AddModuleOfType(Type type)
        {
            player.CreateModule(type);

            // Set Dirty
            if (serializedObject.ApplyModifiedProperties())
            {
                EditorUtility.SetDirty(target);

                Undo.RecordObject(target, "Player Module" + type.ToString() + " added");
            }
        }

        private void RenderEventsGUI()
        {
            EditorGUILayout.HelpBox("Events for observing changes on the Player.", MessageType.None);
            EditorGUILayout.Space();

            EditorHelper.RenderProperties(serializedObject, GetEvents(), false);
        }

    }
}