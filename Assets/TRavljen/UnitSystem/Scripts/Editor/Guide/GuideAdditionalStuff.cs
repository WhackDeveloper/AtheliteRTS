using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace TRavljen.UnitSystem.Editor
{
    [System.Serializable]
    internal sealed class GuideAdditionalStuff : IGuideSection
    {

        private GameObject damageVisualsPrefab;
        private GameObject damageVisuals;

        private GameObject entitySpawnerIndicatorPrefab;
        private GameObject entitySpawnerIndicator;

        private GameObject waveManagerPrefab;
        private GameObject waveManager;

        private GameObject turnBasedPrefab;
        private GameObject turnBased;
        
        [SerializeField]
        private List<string> expanded = new();

        public string Title => "Additional Demo Prefabs";
        public string Id => "_setup_window_additional_stuff";

        public void OnEnable()
        {
            damageVisualsPrefab = GuideHelper.LoadPrefab<GameObject>("UnitSystem/Demo/Prefabs/UI/Visual Damage System.prefab");
            entitySpawnerIndicatorPrefab = GuideHelper.LoadPrefab<GameObject>("UnitSystem/Demo/Prefabs/Misc/Selected Entity Spawn Indicator.prefab");
            waveManagerPrefab = GuideHelper.LoadPrefab<GameObject>("UnitSystem/Demo/Prefabs/Misc/Wave Manager.prefab");
            turnBasedPrefab = GuideHelper.LoadPrefab<GameObject>("UnitSystem/Demo/Prefabs/Misc/Turn Base Production.prefab");
            
            if (damageVisualsPrefab.IsNotNull())
                damageVisuals = GameObject.Find(damageVisualsPrefab.name);
            
            if (entitySpawnerIndicatorPrefab.IsNotNull())
                entitySpawnerIndicator = GameObject.Find(entitySpawnerIndicatorPrefab.name);
            
            if (waveManagerPrefab.IsNotNull())
                waveManager = GameObject.Find(waveManagerPrefab.name);
            
            if (turnBasedPrefab.IsNotNull())
                turnBased = GameObject.Find(turnBasedPrefab.name);
        }

        public void OnGUI(IGuideLayout layout)
        {
            if (damageVisualsPrefab.IsNull() && entitySpawnerIndicatorPrefab.IsNull() && waveManagerPrefab.IsNull() && turnBasedPrefab.IsNull())
            {
                EditorGUILayout.HelpBox("Prefabs seem to be missing. Nothing to set up here.", MessageType.Warning);
                return;
            }

            if (damageVisualsPrefab.IsNotNull())
                OnDamageVisualsGUI();

            if (entitySpawnerIndicatorPrefab.IsNotNull())
                OnSpawnerIndicatorGUI();

            if (waveManagerPrefab.IsNotNull())
                OnWaveManagerGUI();
            
            if (turnBasedPrefab.IsNotNull())
                OnTurnBaseGUI();
        }

        private void OnDamageVisualsGUI()
        {
            const string info = "Add prefab for displaying floating text for damage. " + 
                                "\nPrefab allows selection between world or screen space text positioning.";
            OnPrefabGUI("Damage Visuals", info, "Add damage visuals",
                damageVisualsPrefab, ref damageVisuals);
        }

        private void OnSpawnerIndicatorGUI()
        {
            const string info = "Add prefab for displaying target position of spawn point present on selected entity. " +
                                "\nPrefab allows player to see where spawn point will sent its spawned units.";
            OnPrefabGUI("Spawner Target Indicator", info, "Add target indicator", 
                entitySpawnerIndicatorPrefab, ref entitySpawnerIndicator);
        }
        
        private void OnWaveManagerGUI()
        {
            const string info =
                "Add manager for spawning incoming enemy units and set their target player for attacking.";
            OnPrefabGUI("Enemy Waves", info, "Add wave manager",
                waveManagerPrefab, ref waveManager);
        }
        
        private void OnTurnBaseGUI()
        {
            const string info =
                "Add simple UI and controller for managing production systems in a turn based fashion.";
            OnPrefabGUI("Turn base", info, "Add prefab",
                turnBasedPrefab, ref turnBased);
        }

        private bool PrefabFoldout(string title)
        {
            var old = expanded.Contains(title);
            var isExpanded = EditorGUILayout.Foldout(old, title);

            if (isExpanded == old) return isExpanded;
            
            if (isExpanded)
                expanded.Add(title);
            else
                expanded.Remove(title);

            return isExpanded;
        }
 
        private void OnPrefabGUI(string title, string info, string addActionText, GameObject prefab, ref GameObject instance)
        {
            var isExpanded = PrefabFoldout(title);
            if (!isExpanded) return;
            
            EditorGUI.indentLevel++;
            
            if (instance.IsNotNull())
            {
                EditorGUILayout.HelpBox("All done!", MessageType.Info);
            }
            else
            {
                EditorGUILayout.HelpBox(info, MessageType.Info);
                if (GuideHelper.AlignedButton(addActionText))
                {
                    instance = GuideHelper.EditorCreateObject(prefab, null);
                }
            }

            EditorGUI.indentLevel--;
        }
    }
}