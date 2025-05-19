using System.Collections.Generic;
using TRavljen.EditorUtility;
using UnityEngine;
using UnityEngine.Events;

namespace TRavljen.UnitSystem
{

    /// <summary>
    /// Manages the active players in the game, ensuring a centralized reference 
    /// for adding, removing, and tracking players during gameplay.
    /// </summary>
    /// <remarks>
    /// This class functions as a singleton to maintain consistency across the game. 
    /// It dynamically updates the list of active players and invokes events when the list changes.
    /// </remarks>
    public class ActivePlayersManager : MonoBehaviour
    {
        [SerializeField, DisableInInspector]
        private List<APlayer> activePlayers = new();

        /// <summary>
        /// Invoked whenever the list of active players changes.
        /// </summary>
        public UnityEvent<APlayer[]> OnPlayersChange = new();

        /// <summary>
        /// The singleton instance of the ActivePlayersManager.
        /// </summary>
        private static ActivePlayersManager instance;

        /// <summary>
        /// Gets the array of active players. Do not modify this, it will create
        /// unexpected behaviour.
        /// </summary>
        public APlayer[] GetActivePlayers() => activePlayers.ToArray();

        /// <summary>
        /// Gets the number of active players.
        /// </summary>
        public int ActivePlayerCount => activePlayers.Count;

        /// <summary>
        /// Retrieves the singleton instance of <see cref="ActivePlayersManager"/>, 
        /// creating a new one if it does not already exist.
        /// </summary>
        /// <returns>
        /// The singleton instance of <see cref="ActivePlayersManager"/>.
        /// </returns>
        public static ActivePlayersManager GetOrCreate()
        {
            if (instance.IsNotNull())
                return instance;

            instance = SingletonHandler.Create<ActivePlayersManager>("Active Players Manager");
            return instance;
        }

        /// <summary>
        /// Adds a player to the active players list.
        /// </summary>
        /// <param name="newPlayer">The player to add.</param>
        /// <remarks>
        /// If the player is already in the list, no action is taken. 
        /// Triggers the <see cref="OnPlayersChange"/> event if the list is modified.
        /// </remarks>
        public void AddPlayer(APlayer newPlayer)
        {
            if (!activePlayers.Contains(newPlayer))
            {
                activePlayers.Add(newPlayer);
                OnPlayersChange.Invoke(activePlayers.ToArray());
            }
        }

        /// <summary>
        /// Removes a player from the active players list.
        /// </summary>
        /// <param name="player">The player to remove.</param>
        /// <remarks>
        /// This method may also trigger additional logic for handling defeated players, 
        /// such as disabling production or maintaining a reference to defeated players elsewhere.
        /// Triggers the <see cref="OnPlayersChange"/> event if the list is modified.
        /// </remarks>
        public void RemovePlayer(APlayer player)
        {
            if (activePlayers.Remove(player))
            {
                OnPlayersChange.Invoke(activePlayers.ToArray());
            }
        }

        /// <summary>
        /// Ensures only one instance of the ActivePlayersManager exists.
        /// </summary>
        private void Awake()
        {
            if (instance == null)
            {
                instance = this;
            }
            else
            {
                Debug.Log("Singleton already exists, new instance will be destroyed");
                Destroy(this);
            }
        }

    }

}