using System.Collections;
using System.Collections.Generic;
using UnityEngine.Events;

namespace TRavljen.UnitSystem
{

    /// <summary>
    /// Manages events related to entity.
    /// Provides a centralized event system for the entity mechanics in the game.
    /// </summary>
    public sealed class EntityEvents
    {
        public static readonly EntityEvents Instance = new();

        /// <summary>
        /// Event invoked when an will be destroyed.
        /// </summary>
        public UnityEvent<Entity> OnEntityDestroy = new();

        /// <summary>
        /// Event invoked when unit changes owners.
        /// </summary>
        public UnityEvent<Entity> OnOwnerChanged = new();

    }
}