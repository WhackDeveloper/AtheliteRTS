using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace TRavljen.UnitSystem.Demo
{

#if UNITY_EDITOR
    using UnityEditor;
    [CustomEditor(typeof(TurnBaseProductionManager))]
    internal class TurnBaseProductionManagerEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            EditorGUILayout.HelpBox("Disables real time production on players & manages production manually.", MessageType.None);
            base.OnInspectorGUI();
        }
    }
#endif
    
    /// <summary>
    /// Manages turn-based production updates in a strategy game. 
    /// Each turn, this component increments production time for all players' 
    /// production modules that support turn-based updates.
    /// </summary>
    /// <remarks>
    /// Currently, only the production system fully supports turn-based updates. 
    /// Other systems like unit movement and resource collection are not yet 
    /// compatible with this feature.
    /// </remarks>
    public class TurnBaseProductionManager : MonoBehaviour
    {

        /// <summary>
        /// Index of the current player.
        /// </summary>
        private int currentPlayerIndex = 0;

        /// <summary>
        /// Amount of production time incremented for each turn.
        /// </summary>
        [SerializeField]
        private int turnIncrement = 1;

        /// <summary>
        /// Event invoked when all players have completed their turns.
        /// </summary>
        public UnityEvent AllTurnsEnded = new();

        private ActivePlayersManager playersManager;
        private readonly List<ProductionModule> modules = new();

        /// <summary>
        /// Initializes the manager and sets up active players and their production modules.
        /// </summary>
        private void Awake()
        {
            playersManager = ActivePlayersManager.GetOrCreate();

            // Initial set up
            PlayersChange(playersManager.GetActivePlayers());
        }

        /// <summary>
        /// Subscribes to player-related events on enable.
        /// </summary>
        private void OnEnable()
        {
            playersManager.OnPlayersChange.AddListener(PlayersChange);
        }

        /// <summary>
        /// Unsubscribes from player-related events on disable.
        /// </summary>
        private void OnDisable()
        {
            playersManager.OnPlayersChange.RemoveListener(PlayersChange);
        }

        /// <summary>
        /// Handles changes in the active players list and updates the production modules.
        /// </summary>
        /// <param name="players">The list of currently active players.</param>
        private void PlayersChange(APlayer[] players)
        {
            modules.Clear();

            foreach (APlayer player in players)
            {
                if (player.TryGetModule(out ProductionModule module))
                {
                    module.ProduceManually = true;
                    modules.Add(module);
                }
            }
        }

        /// <summary>
        /// Ends the current player's turn and advances to the next player. 
        /// If all players have completed their turns, invokes global turn-end logic 
        /// and processes production updates.
        /// </summary>
        public void EndTurnForCurrentPlayer()
        {
            currentPlayerIndex += 1;

            // Check if last player ended his turn.
            // If it did, handle production.
            if (currentPlayerIndex == playersManager.ActivePlayerCount)
            {
                currentPlayerIndex = 0;
                AllTurnsEnded?.Invoke();

                // Produce for one increment.
                foreach (var module in modules)
                {
                    module.Produce(turnIncrement);
                }
            }
        }

    }

}