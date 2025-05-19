using System;
using System.Collections.Generic;
using UnityEngine;

namespace TRavljen.UnitSystem
{
    
    #if UNITY_EDITOR
    using UnityEditor;
    using TRavljen.EditorUtility;
    internal static class PlayersRelationshipManagerEditorTools
    {
        [MenuItem("GameObject/TRavljen/UnitSystem/Player Relationship Manager")]
        public static void CreateRelationshipManagerInScene()
        {
            if (EditorTools.CreateObjectFromMenu<PlayersRelationshipManager>("Player Relationship Manager", true))
            {
                Debug.Log("New player relationship manager created. Set up player relationships.");
            }
        }
    }
    #endif
        
    /// <summary>
    /// Defines possible states between players.
    /// </summary>
    [Serializable]
    public enum RelationshipState
    {
        /// <summary> Default state with no hostility or alliance. </summary>
        Neutral,

        /// <summary> Players are friendly and cooperative. </summary>
        Ally,

        /// <summary> Players are enemies and can attack each other. </summary>
        Hostile,
    }

    /// <summary>
    /// Represents a relationship between two players.
    /// </summary>
    [System.Serializable]
    struct PlayerRelationship
    {
        [SerializeField]
        private APlayer playerA;

        [SerializeField]
        private APlayer playerB;

        [SerializeField]
        private RelationshipState state;

        /// <summary>
        /// Gets the relationship state between the players.
        /// </summary>
        public readonly RelationshipState State => state;

        /// <summary>
        /// Initializes a new instance of the <see cref="PlayerRelationship"/> struct.
        /// </summary>
        /// <param name="playerA">The first player.</param>
        /// <param name="playerB">The second player.</param>
        /// <param name="state">The relationship state between the players.</param>
        public PlayerRelationship(APlayer playerA, APlayer playerB, RelationshipState state)
        {
            this.playerA = playerA;
            this.playerB = playerB;
            this.state = state;
        }

        /// <summary>
        /// Checks if the relationship involves the specified pair of players.
        /// </summary>
        /// <param name="playerA">The first player to check.</param>
        /// <param name="playerB">The second player to check.</param>
        /// <returns>True if the relationship contains both players, regardless of order.</returns>
        public readonly bool Contains(APlayer playerA, APlayer playerB)
        {
            return (this.playerA == playerA && this.playerB == playerB) ||
                (this.playerA == playerB && this.playerB == playerA);
        }
    }
    
    /// <summary>
    /// Manages relationships between players.
    /// </summary>
    [DisallowMultipleComponent]
    public class PlayersRelationshipManager : MonoBehaviour
    {
        
        private static PlayersRelationshipManager _instance;
        
        [Tooltip("Specifies the default relationship state used for undefined relationships.")]
        [SerializeField]
        private RelationshipState defaultState = RelationshipState.Hostile;

        [SerializeField]
        private List<PlayerRelationship> playerRelationships = new();

        /// <summary>
        /// Retrieves the singleton instance of <see cref="PlayersRelationshipManager"/>,
        /// creating a new one if it does not already exist.
        /// </summary>
        /// <returns>The singleton instance of <see cref="PlayersRelationshipManager"/>.</returns>
        public static PlayersRelationshipManager GetOrCreate()
        {
            if (_instance.IsNotNull()) return _instance;

            _instance = UnityEngine.Object.FindFirstObjectByType<PlayersRelationshipManager>();

            if (_instance.IsNotNull()) return _instance;

            _instance = SingletonHandler.Create<PlayersRelationshipManager>("Players Relationship Manager");
            return _instance;
        }

        private void Awake()
        {
            // Set instance if null
            if (_instance == null)
            {
                _instance = this;
            }
            // Destroy script if it's an additional instance.
            else if (_instance != this)
            {
                Destroy(this);
            }
        }

        /// <summary>
        /// Sets the relationship state between two players.
        /// If a relationship already exists, it is updated.
        /// </summary>
        /// <param name="p1">The first player.</param>
        /// <param name="p2">The second player.</param>
        /// <param name="state">The new relationship state.</param>
        public void SetRelationship(APlayer p1, APlayer p2, RelationshipState state)
        {
            for (int i = 0; i < playerRelationships.Count; i++)
            {
                if (playerRelationships[i].Contains(p1, p2))
                {
                    playerRelationships[i] = new PlayerRelationship(p1, p2, state);
                    return;
                }
            }

            // If no existing relationship, add a new one
            playerRelationships.Add(new PlayerRelationship(p1, p2, state));
        }

        /// <summary>
        /// Gets the relationship state between two players.
        /// If no relationship is found, defaults to <see cref="defaultState"/>.
        /// </summary>
        /// <param name="p1">The first player.</param>
        /// <param name="p2">The second player.</param>
        /// <returns>The current relationship state.</returns>
        public RelationshipState GetRelationship(APlayer p1, APlayer p2)
        {
            foreach (var relation in playerRelationships)
            {
                if (relation.Contains(p1, p2))
                    return relation.State;
            }

            return defaultState;
        }

        /// <summary>
        /// Determines whether two players are allies.
        /// </summary>
        /// <param name="p1">The first player.</param>
        /// <param name="p2">The second player.</param>
        /// <returns>True if the players are allied; otherwise, false.</returns>
        public bool AreAllied(APlayer p1, APlayer p2)
        {
            return GetRelationship(p1, p2) == RelationshipState.Ally;
        }

        /// <summary>
        /// Determines whether two players are enemies.
        /// </summary>
        /// <param name="p1">The first player.</param>
        /// <param name="p2">The second player.</param>
        /// <returns>True if the players are enemies; otherwise, false.</returns>
        public bool AreEnemies(APlayer p1, APlayer p2)
        {
            return GetRelationship(p1, p2) == RelationshipState.Hostile;
        }
    }

}