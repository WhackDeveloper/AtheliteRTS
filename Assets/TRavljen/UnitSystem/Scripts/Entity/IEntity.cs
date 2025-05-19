using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace TRavljen.UnitSystem
{
    using Combat;
    using Build;
    using Garrison;

    /// <summary>
    /// Root interface for all entities in the game, representing any interactive game object, 
    /// including units, resource nodes, towers, faction flags, etc.
    /// </summary>
    public interface IEntity
    {

        /// <summary>
        /// Gets the root transform of the entity.
        /// </summary>
        public Transform transform { get; }

        /// <summary>
        /// Gets the root game object of the entity.
        /// </summary>
        public GameObject gameObject { get; }

        /// <summary>
        /// Gets the owner of the entity.
        /// </summary>
        public APlayer Owner { get; }

        /// <summary>
        /// Indicates whether the entity is operational and ready to be used.
        /// This should only be false if entity requires construction or startup animation before it is ready for use.
        /// </summary>
        public bool IsOperational { get; }
        
        /// <summary>
        /// Gets the data associated with the entity, this would be a scriptable object defining its configuration.
        /// </summary>
        public AEntitySO Data { get; }

        #region Components

        /// <summary>
        /// Gets the health system associated with the entity, if present.
        /// </summary>
        /// <remarks>
        /// Returns <c>null</c> if the entity does not have a health system.
        /// </remarks>
        public IHealth Health { get; }

        /// <summary>
        /// Contract for entity garrison which enables other units to enter this one.
        /// </summary>
        public IGarrisonEntity Garrison { get; }

        /// <summary>
        /// Contract for entity production component.
        /// </summary>
        public IActiveProduction ActiveProduction { get; }

        /// <summary>
        /// Contract for entity spawn point component.
        /// </summary>
        public IUnitSpawn UnitSpawn { get; }

        /// <summary>
        /// Contract for entity control for direct command. This is used for
        /// non-automated behaviour, like initial movement on spawn or
        /// player commands.
        /// </summary>
        public IControlEntity EntityControl { get; }

        /// <summary>
        /// Contract for entity which requires building.
        /// </summary>
        public IBuildableEntity Buildable { get; }

        #endregion
        
        #region Events
        
        /// <summary>
        /// Event invoked when entity becomes operational. This is relevant only for entities
        /// where some form of construction or delay is present after placement or spawning.
        /// </summary>
        public UnityEvent<Entity> OnOperationsActive { get; }
        
        #endregion

        #region Methods

        /// <summary>
        /// Assigns ownership of this entity to a specified player.
        /// </summary>
        /// <param name="player">The player to whom ownership will be assigned.</param>
        public void AssignOwner(APlayer newOwner);

        /// <summary>
        /// Removes this entity from its current owner's control.
        /// This method should be used to manually manage ownership before destruction.
        /// </summary>
        public void RemoveEntityFromOwner();

        /// <summary>
        /// Destroys the entity and removes it from its owner's control.
        /// </summary>
        /// <param name="delay">The delay, in seconds, before the GameObject is destroyed. Defaults to 0.</param>
        public void DestroyEntity(float delay = 0f);

        /// <summary>
        /// Sets the operational state of the entity. By default, all entities are operational; this can be useful
        /// for cases like building state of an entity.
        /// </summary>
        /// <remarks>
        /// When the entity becomes operational, it is added to the owner's producible list
        /// and relevant events are triggered.
        /// </remarks>
        /// <param name="operational">True if the entity is becoming operational, false otherwise.</param>
        public void SetOperational(bool operational);

        /// <summary>
        /// Attempts to retrieve a capability of the specified type from the entity's data.
        /// </summary>
        /// <typeparam name="Capability">The type of capability to retrieve.</typeparam>
        /// <param name="capability">The retrieved capability, if available.</param>
        /// <returns>True if the capability exists; otherwise, false.</returns>
        public bool TryGetCapability<Capability>(out Capability capability) where Capability : IEntityCapability;

        /// <summary>
        /// Attempts to retrieve a component of the specified type attached to the entity.
        /// </summary>
        /// <typeparam name="EntityComponent">The type of component to retrieve.</typeparam>
        /// <param name="component">The retrieved component, if available.</param>
        /// <returns>True if the component exists; otherwise, false.</returns>
        public bool TryGetEntityComponent<EntityComponent>(out EntityComponent component) where EntityComponent : IEntityComponent;

        #endregion

    }

}