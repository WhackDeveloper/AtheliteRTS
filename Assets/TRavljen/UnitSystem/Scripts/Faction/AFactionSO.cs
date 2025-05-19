using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TRavljen.UnitSystem
{
    /// <summary>
    /// Represents the quantity of a specific unit type that can be spawned or produced.
    /// </summary>
    [System.Serializable]
    public struct UnitQuantity
    {
        [PositiveInt]
        [Tooltip("The number of units of the specified type.")]
        public int count;

        /// <summary>
        /// The unit type associated with this quantity. This can represent any unit
        /// type that a player can spawn or produce.
        /// If a limit is reached, its production requirements will not be fulfilled.
        /// </summary>
        [Tooltip("Specifies limited unit, this can be of any type that player can spawn. " +
            "When limit is reached it's requirements for production will not be fulfilled.")]
        public AUnitSO unit;
    }

    /// <summary>
    /// Represents an abstract base class for faction definitions within the game.
    /// Stores essential faction details such as name, description, and unit limits.
    /// </summary>
    public abstract class AFactionSO : ManagedSO
    {

        #region Base

        [Tooltip("Name of the faction.")]
        [SerializeField]
        protected string factionName;

        [Tooltip("Description of the faction.")]
        [SerializeField]
        protected string description;

        [Tooltip("The maximum allowed limit for specific unit types in the faction.")]
        [SerializeField]
        private UnitLimit[] unitMaxLimit;

        /// <summary>
        /// Gets the description of the faction.
        /// </summary>
        public string Description => description;

        /// <summary>
        /// Gets the name of the faction.
        /// </summary>
        public string Name => factionName;

        /// <summary>
        /// Gets the maximum allowed limits for specific unit types in the faction.
        /// </summary>
        public UnitLimit[] UnitMaxLimit => unitMaxLimit;

        /// <summary>
        /// Sets new faction name.
        /// </summary>
        public void SetFactionName(string name) => factionName = name;

        /// <summary>
        /// Sets new faction description.
        /// </summary>
        public void SetDescription(string description) => this.description = description;

        #endregion

        #region Abstract

        /// <summary>
        /// Configures the faction for a specific player.
        /// This typically involves setting up resource limits, research, and other faction-specific parameters.
        /// </summary>
        /// <param name="player">The player entity to configure.</param>
        public abstract void ConfigurePlayer(APlayer player);

        /// <summary>
        /// Returns an array of unit prefabs and their count. This interface
        /// is generally used for spawning initial units of the player in world.
        /// </summary>
        /// <returns>Array of starting units and their count</returns>
        public abstract UnitQuantity[] GetStartingUnits();

        #endregion

    }
}