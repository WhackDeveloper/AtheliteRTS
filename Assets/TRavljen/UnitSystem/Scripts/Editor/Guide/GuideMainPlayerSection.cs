using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using Object = UnityEngine.Object;

namespace TRavljen.UnitSystem.Editor
{
    using UnityEditor;
    
    [Serializable]
    sealed class GuideMainPlayerSection : IGuideSection
    {
        public string Title => "Main Player";
        public string Id => "_setup_window_main_player";
        
        [SerializeField] 
        private List<APlayer> expandedPlayerFoldouts = new();
        
        [SerializeField]
        private bool showOtherPlayers = false;
        [SerializeField]
        private bool showRelationships = false;
        
        [SerializeField]
        private APlayer mainPlayer;
        
        private GameObject playersRoot;
        private PlayersRelationshipManager relationshipManager;
        private Editor relationshipManagerEditor;

        private APlayer playerPrefab;
        private APlayerUnitSpawner spawnerPrefab;

        public APlayer GetMainPlayer() => mainPlayer;

        public void OnEnable()
        {
            playerPrefab = GuideHelper.LoadPrefab<APlayer>(
                "UnitSystem/Prefabs/Player.prefab");
            spawnerPrefab = GuideHelper.LoadPrefab<APlayerUnitSpawner>(
                "UnitSystem/Prefabs/Faction Player Spawn Point.prefab");
        }

        public void OnGUI(IGuideLayout layout)
        {
            playerPrefab = (APlayer)EditorGUILayout.ObjectField("Player prefab", playerPrefab, typeof(APlayer), false);
            playersRoot = (GameObject)EditorGUILayout.ObjectField(new GUIContent("Creation Root"), playersRoot, typeof(GameObject), true);

            mainPlayer = (APlayer)EditorGUILayout.ObjectField(new GUIContent("Main Player"), this.mainPlayer, typeof(APlayer), true);
            if (mainPlayer.IsNull())
                mainPlayer = null;
            
            if (SceneManager.GetActiveScene() != mainPlayer?.gameObject.scene)
            {
                mainPlayer = null;
            }
            
            if (mainPlayer.IsNull() && GuideHelper.AlignedButton("Create main player"))
            {
                mainPlayer = CreatePlayer("Main Player");
                EditorPrefs.SetBool("_setup_window_players_scene", true);
                expandedPlayerFoldouts.Add(mainPlayer);
            }
            
            if (!mainPlayer.IsNotNull()) return;
            if (!PlayerValidationGUI(mainPlayer, false)) return;
            
            EditorGUILayout.HelpBox("Main player is set up.", MessageType.None);
            
            // Show other players after main one is OK.
            EditorGUILayout.Space(8);

            showOtherPlayers = EditorGUILayout.Foldout(showOtherPlayers, "Other Players");
            if (showOtherPlayers)
                ScenePlayersGUI();

            EditorGUILayout.Space(8);

            showRelationships = EditorGUILayout.Foldout(showRelationships, "Relationships");

            if (showRelationships)
                PlayerRelationshipsGUI(layout);
        }

        private void PlayerRelationshipsGUI(IGuideLayout layout)
        {
            EditorGUILayout.HelpBox("Here you can configure relations between players. This is " +
                                    "primarily needed only for the combat module.", MessageType.Info);
            relationshipManager = Object.FindFirstObjectByType<PlayersRelationshipManager>();
            EditorGUILayout.ObjectField(
                "Configure",
                relationshipManager,
                typeof(PlayersRelationshipManager),
                true);

            if (!relationshipManager.IsNull()) return;
            
            relationshipManager = null;
                
            if (!GuideHelper.AlignedButton("Create new player relationship manager")) return;
                
            var unitSystemRoot = GameObject.Find("Unit System");
            if (unitSystemRoot.IsNull())
            {
                unitSystemRoot = new GameObject("Unit System");
                unitSystemRoot.transform.SetParent(layout.PackageRoot);
                Undo.RegisterCreatedObjectUndo(unitSystemRoot, "Create Relationship Manager");
            }

            if (!unitSystemRoot.IsNotNull()) return;
                
            if (!unitSystemRoot.TryGetComponent(out relationshipManager))
                relationshipManager = (PlayersRelationshipManager)Undo.AddComponent(
                    unitSystemRoot,
                    typeof(PlayersRelationshipManager));

            Selection.activeObject = relationshipManager.gameObject;
        }
        
        private void ScenePlayersGUI()
        {
            var players = Object.FindObjectsByType<APlayer>(FindObjectsSortMode.None);
            foreach (var player in players)
            {
                if (player.IsNull() || ReferenceEquals(mainPlayer, player)) continue;

                if (!GuideHelper.ListFoldoutGUI(expandedPlayerFoldouts, player, player.name))
                    continue;
                
                PlayerValidationGUI(player);
            }
            
            EditorGUILayout.Space(8);
            if (GuideHelper.AlignedButton("Create new player"))
            {
                CreatePlayer("New Player");
            }
        }

        private bool PlayerValidationGUI(APlayer player, bool showOK = true)
        {
            EditorGUI.indentLevel++;

            var issues = 0;

            if (PlayerModulesGUI(player))
                issues++;
                
            if (PlayerFactionGUI(player))
                issues++;
                
            if (PlayerSpawnerGUI(player))
                issues++;

            if (issues == 0 && showOK)
            {
                EditorGUILayout.HelpBox("Everything looks OK", MessageType.Info);
            }
            
            EditorGUI.indentLevel--;
            return issues == 0;
        }

        private bool PlayerModulesGUI(APlayer player)
        {
            if (player.Modules.Length != 0) return false;
            
            EditorGUILayout.HelpBox("Player has no modules.", MessageType.Warning);
            return true;
        }

        private bool PlayerFactionGUI(APlayer player)
        {
            if (!player.Faction.IsNull()) return false;
            
            EditorGUILayout.HelpBox("Player has no faction.", MessageType.Warning);

            var faction =
                (AFactionSO)EditorGUILayout.ObjectField("Faction", player.Faction, typeof(AFactionSO), false);
            if (faction.IsNotNull())
            {
                player.Faction = faction;
                EditorUtility.SetDirty(player);
            }

            return true;
        }

        private bool PlayerSpawnerGUI(APlayer player)
        {
            var spawners = player.GetComponentsInChildren<APlayerUnitSpawner>(false);
            if (spawners.Length > 0) return false;
            
            EditorGUILayout.HelpBox("Player has no active spawners of type 'FactionPlayerUnitSpawner' on this player. " +
                                    "\nYou can ignore this warning if you spawn and configure player on Start manually. ", MessageType.Warning);
                    
            var prefab = (GameObject)EditorGUILayout.ObjectField(spawnerPrefab?.gameObject, typeof(GameObject), false);
            if (prefab != spawnerPrefab?.gameObject)
            {
                if (prefab.IsNull())
                    spawnerPrefab = null;
                else if (prefab.TryGetComponent(out APlayerUnitSpawner newPrefab))
                    spawnerPrefab = newPrefab;
            }
                    
            if (!GuideHelper.AlignedButton("Add spawner")) return true;
            
            if (spawnerPrefab.IsNull())
            {
                // Create new Spawner object under player.
                var newSpawner = new GameObject("New Spawner");
                newSpawner.AddComponent<FactionPlayerUnitSpawner>();
                newSpawner.AddComponent<UnitSpawnRadius>();
                newSpawner.AddComponent<PlayerBuildingsSpawn>();
                newSpawner.transform.SetParent(player.transform);
                
                Undo.RegisterCreatedObjectUndo(newSpawner, "Add spawn point for player");
                EditorUtility.SetDirty(newSpawner.gameObject);
                    
                // Select it, it needs further setup
                Selection.activeObject = newSpawner.gameObject;
            }
            else
            {
                GuideHelper.EditorCreateObject(spawnerPrefab, player.transform);
            }

            return true;
        }

        private APlayer CreatePlayer(string name)
        {
            APlayer player;
            
            if (!playerPrefab.IsNull())
            {
                player = GuideHelper.EditorCreateObject(playerPrefab, playersRoot.IsNull() ? null : playersRoot.transform);
                player.name = name;
                return player;
            }
            
            var playerGo = new GameObject(name);
            player = playerGo.AddComponent<Player>();
            
            if (!playersRoot.IsNull())
                player.transform.SetParent(playersRoot.transform);

            Undo.RegisterCreatedObjectUndo(playerGo, $"Create {name}");
            return player;
        }
        
    }
}