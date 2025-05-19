using TRavljen.EditorUtility;
using UnityEngine;

namespace TRavljen.UnitSystem.Editor
{

    using UnityEditor;

    [CustomEditor(typeof(PlayersRelationshipManager), true)] 
    public class PlayerRelationshipManagerEditor : HiddenScriptPropertyEditor
    {
        private const string tabKey = "player_relationship_manager_tab";
        private int tabIndex;
        
        private PlayersRelationshipManager manager;
        private APlayer[] scenePlayers;

        private int selectedPlayerID
        {
            get => EditorPrefs.GetInt("player_relationship_selected_player_id", -1);
            set => EditorPrefs.SetInt("player_relationship_selected_player_id", value);
        }
        
        private APlayer selectedPlayer;

        private void OnEnable()
        {
            tabIndex = EditorPrefs.GetInt(tabKey, tabIndex);

            manager = target as PlayersRelationshipManager;
            RefreshPlayers();
        }

        private void RefreshPlayers()
        {
            scenePlayers = FindObjectsByType<APlayer>(FindObjectsSortMode.InstanceID);

            var playerID = selectedPlayerID;
            
            foreach (var player in scenePlayers)
            {
                if (player.GetInstanceID() == playerID)
                {
                    selectedPlayer = player;
                }
            }

            if (!selectedPlayer.IsNull() || scenePlayers.Length <= 0) return;
            
            selectedPlayer = scenePlayers[0];
            selectedPlayerID = selectedPlayer.GetInstanceID();
        }

        public override void OnInspectorGUI()
        {
            var index = GUILayout.Toolbar(tabIndex, new[] { "Normal", "Edit" });
            if (index != tabIndex)
                tabIndex = index;
            
            if (tabIndex == 0)
                base.OnInspectorGUI();
            else
                OnEditGUI();
        }

        private void OnEditGUI()
        {
            EditorGUILayout.LabelField("Showing relationships for currently selected player.");

            selectedPlayer = (APlayer)EditorGUILayout.ObjectField("Selected player: ", selectedPlayer, typeof(APlayer), true);

            var newPlayerID = selectedPlayer.IsNotNull() ? selectedPlayer.GetInstanceID() : -1;
            if (newPlayerID != selectedPlayerID)
                selectedPlayerID = newPlayerID;
            
            if (selectedPlayer.IsNull())
            {
                EditorGUILayout.HelpBox("Select a player from the scene to edit", MessageType.Info);
                return;
            }
            
            EditorGUILayout.HelpBox("Only enabled players will appear as options for undefined relationships.", MessageType.None);
            
            var index = 0;
            foreach (var scenePlayer in scenePlayers)
            {
                if (scenePlayer == selectedPlayer) continue;

                EditorGUILayout.BeginVertical(GUI.skin.box);

                EditorGUILayout.LabelField($"Relationship {++index}");
                EditorGUILayout.ObjectField(scenePlayer, typeof(APlayer), true);

                var state = manager.GetRelationship(selectedPlayer, scenePlayer);
                var newState = (RelationshipState)EditorGUILayout.EnumPopup(state);

                if (state != newState)
                {
                    manager.SetRelationship(selectedPlayer, scenePlayer, newState);
                    EditorUtility.SetDirty(manager);
                }

                EditorGUILayout.EndVertical();
            }
            
            if (GUILayout.Button("Refresh players"))
                RefreshPlayers();
        }
    }
}
