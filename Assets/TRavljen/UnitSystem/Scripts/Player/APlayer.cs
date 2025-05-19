using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using System;
using UnityEditor;

namespace TRavljen.UnitSystem
{

    /// <summary>
    /// Defines the main player component, representing a player in the game. 
    /// This component manages the player's units, entities, modules, and producibles.
    /// </summary>
    public abstract class APlayer : MonoBehaviour
    {

        #region Properties

        [SerializeField]
        private AFactionSO faction;

        /// <summary>
        /// List of player modules. Gives support to various features, custom or built-in.
        /// </summary>
        [SerializeField]
        private APlayerModule[] modules = new APlayerModule[0];

        /// <summary>
        /// Collection of all player units. Units are a subset of entities that 
        /// represent controllable and commandable game objects.
        /// </summary>
        protected readonly List<IUnit> units = new();

        /// <summary>
        /// Collection of all player entities. Entities include both units and other 
        /// types of objects (e.g., static buildings, destructible items).
        /// </summary>
        protected readonly List<IEntity> entities = new();

        #endregion

        #region Events

        /// <summary>
        /// Event invoked when a unit is added to the player's collection.
        /// </summary>
        public UnityEvent<IUnit> OnUnitAdded = new();

        /// <summary>
        /// Event invoked when a unit is removed from the player's collection.
        /// </summary>
        public UnityEvent<IUnit> OnUnitRemoved = new();

        /// <summary>
        /// Event invoked when a entity is added to the player's collection.
        /// </summary>
        public UnityEvent<IEntity> OnEntityAdded = new();

        /// <summary>
        /// Event invoked when a entity is removed from the player's collection.
        /// </summary>
        public UnityEvent<IEntity> OnEntityRemoved = new();

        /// <summary>
        /// Event invoked when a producible is added to the player.
        /// </summary>
        public UnityEvent<AProducibleSO, long> OnRegisterProducible = new();

        /// <summary>
        /// Event invoked when a producible is added to the player.
        /// </summary>
        public UnityEvent<AProducibleSO> OnUnregisterProducible = new();

        #endregion

        #region Getters

        /// <summary>
        /// Gets all modules attached to the player.
        /// </summary>
        public APlayerModule[] Modules => modules;

        /// <summary>
        /// Gets or sets the player's faction.
        /// </summary>
        public AFactionSO Faction
        {
            get => faction;
            set => faction = value;
        }

        /// <summary>
        /// Retrieves all units currently owned by the player.
        /// </summary>
        public IUnit[] GetUnits() => units.ToArray();

        /// <summary>
        /// Retrieves all entities currently owned by the player.
        /// </summary>
        public IEntity[] GetEntities() => entities.ToArray();

        #endregion

        #region Lifecycle

        protected virtual void Awake()
        {
            // Adds player to the manager
            ActivePlayersManager.GetOrCreate().AddPlayer(this);

            foreach (var unit in units)
            {
                unit.AssignOwner(this);
            }
        }

        protected virtual void OnDestroy()
        {
            // Removes player from the manager. Defeated player should not be destroyed.
            ActivePlayersManager manager = ActivePlayersManager.GetOrCreate();
            if (manager != null)
                manager.RemovePlayer(this);
        }

        protected virtual void OnValidate()
        {
            // Update references.
            modules = GetComponentsInChildren<APlayerModule>();
        }

        #endregion

        #region Player Relations

        /// <summary>
        /// Checks if the players are allies.
        /// </summary>
        /// <param name="other">Player to check.</param>
        /// <returns>Returns true if players are allies; otherwise false.</returns>
        public abstract bool ArePlayersAllied(APlayer other);

        #endregion

        #region Module management

        /// <summary>
        /// Adds a module to the player.
        /// </summary>
        /// <param name="module">The module to be added.</param>
        public void AddModule(APlayerModule module)
        {
            modules = new List<APlayerModule>(modules) { module }.ToArray();
        }

        /// <summary>
        /// Creates a new module and adds it to the player.
        /// </summary>
        /// <typeparam name="Module">Type of the module to create.</typeparam>
        /// <returns>Returns created module.</returns>
        public Module CreateModule<Module>() where Module : APlayerModule
        {
            return CreateModule(typeof(Module)) as Module;
        }

        /// <summary>
        /// Creates a new module and adds it to the player.
        /// </summary>
        /// <param name="type">Type of the module to create. This type must be a subclass of <see cref="APlayerModule"/></param>
        /// <returns>Returns created module.</returns>
        public APlayerModule CreateModule(Type type)
        {
            // Check for invalid types
            if (!type.IsSubclassOf(typeof(APlayerModule)))
            {
                Debug.Log($"Type {type} NOT subclass of APlayerModule!");
                return null;
            }

            APlayerModule module = GetComponentInChildren(type, true) as APlayerModule;

            if (module == null)
            {
                Transform transform = this.transform.Find("Modules");

                if (transform == null)
                    transform = gameObject.AddChildGameObject("Modules").transform;

#if UNITY_EDITOR
                module = Undo.AddComponent(transform.gameObject, type) as APlayerModule;
#else
                module = transform.gameObject.AddComponent(type) as APlayerModule;
#endif
            }

            AddModule(module);

            return module;
        }

        /// <summary>
        /// Removes a specific module from the player.
        /// </summary>
        /// <param name="module">The module to be removed.</param>
        public void RemoveModule(APlayerModule module)
        {
            List<APlayerModule> temp = new(modules);
            temp.Remove(module);
            modules = temp.ToArray();
        }

        /// <summary>
        /// Removes all modules of the specified type from the player.
        /// </summary>
        /// <typeparam name="Module">The type of the module to remove.</typeparam>
        public void DestroyModule<Module>() where Module : APlayerModule
        {
            List<APlayerModule> temp = new(modules);
            for (int index = temp.Count - 1; index >= 0; index--)
            {
                if (temp[index].GetType() == typeof(Module))
                {
                    Destroy(temp[index]);
                    temp.RemoveAt(index);
                }
            }

            modules = temp.ToArray();
        }

        /// <summary>
        /// Retrieves a module of the specified type from the player.
        /// </summary>
        /// <typeparam name="Module">The type of the module to retrieve.</typeparam>
        /// <returns>The first module of the specified type, or <c>null</c> if no such module exists.</returns>
        public Module GetModule<Module>() where Module : APlayerModule
        {
            foreach (APlayerModule module in modules)
                if (module is Module searchedModule)
                    return searchedModule;

            return null;
        }

        /// <summary>
        /// Attempts to retrieve a module of the specified type from the player.
        /// </summary>
        /// <typeparam name="Module">The type of the module to retrieve.</typeparam>
        /// <param name="behaviour">The retrieved module, or <c>null</c> if no such module exists.</param>
        /// <returns><c>true</c> if a module of the specified type is found; otherwise, <c>false</c>.</returns>
        public bool TryGetModule<Module>(out Module behaviour) where Module : APlayerModule
        {
            foreach (APlayerModule module in modules)
            {
                if (module is Module searchedModule)
                {
                    behaviour = searchedModule;
                    return true;
                }
            }

            behaviour = null;
            return false;
        }

        #endregion

        #region Unit Management

        /// <summary>
        /// Adds an entity to the player by assigning ownership and managing 
        /// related attributes or producibles based on the entity type.
        /// </summary>
        /// <param name="entity">The entity to be added.</param>
        /// <param name="register">Specifies whether to register producible for this entity.</param>
        public virtual void AddEntity(IEntity entity, bool register)
        {
            entities.Add(entity);
            entity.AssignOwner(this);

            if (register)
            {
                RegisterProducible(entity.Data);
            }

            OnEntityAdded.Invoke(entity);

            if (entity is not IUnit unit) return;
            
            units.Add(unit);
            OnUnitAdded.Invoke(unit);
        }

        /// <summary>
        /// Removes an entity from the player by revoking ownership and managing 
        /// related attributes or producibles based on the entity type.
        /// </summary>
        /// <param name="entity">The entity to be removed.</param>
        /// <param name="unregister">Specifies whether to unregister producible for this entity.</param>
        public virtual void RemoveEntity(IEntity entity, bool unregister)
        {
            if (!entities.Remove(entity)) return;

            entity.AssignOwner(null);

            if (unregister)
            {
                UnregisterProducible(entity.Data);
            }

            OnEntityRemoved.Invoke(entity);

            if (entity is IUnit unit && units.Remove(unit))
            {
                OnUnitRemoved?.Invoke(unit);
            }
        }

        /// <summary>
        /// Adds unit to the player by assigning ownership and storing
        /// the reference into <see cref="Units"/> collection. This
        /// allows quick access and matching for players units.
        /// </summary>
        /// <param name="unit">
        /// New unit that will to be added to the player.
        /// </param>
        /// <param name="registersProducible">
        /// Determines if the unit should also be registered (invoking
        /// register event) for modules to respond to.
        /// </param>
        public virtual void AddUnit(IUnit unit, bool registersProducible)
        {
            AddEntity(unit, registersProducible);
        }

        /// <summary>
        /// Removes unit from the player by removing ownership and
        /// removing the unit from <see cref="Units"/> collection.
        /// </summary>
        /// <param name="unit">
        /// Unit to be removed, which will no longer be under ownership of
        /// it's current player.
        /// </param>
        /// <param name="unregistersProducible">
        /// Determines if the unit should also be unregistered (invoking
        /// unregister event) for modules to respond to.
        /// </param>
        public virtual void RemoveUnit(IUnit unit, bool unregistersProducible)
        {
            RemoveEntity(unit, true);
        }

        /// <summary>
        /// Refunds the player for the destruction of the unit. This is generally
        /// used for when player destroys the unit manually.
        /// </summary>
        /// <param name="unit">Unit for which to refund cost.</param>
        public abstract void RefundPlayer(IUnit unit);

        #endregion

        #region Producible Management

        /// <summary>
        /// Adds a producible of resource type with the specified quantity to the player.
        /// If the player has a resource module, the resource will be added; otherwise, 
        /// the full quantity remains unadded.
        /// </summary>
        /// <param name="resourceQuantity">The resource and quantity to add.</param>
        /// <returns>
        /// Returns the remaining quantity if not all resources could be added/deposited; 
        /// otherwise, returns 0.
        /// </returns>
        public virtual long AddResource(ResourceQuantity resourceQuantity)
        {
            if (TryGetModule(out ResourceModule module))
            {
                return module.AddResource(resourceQuantity);
            }

            return resourceQuantity.Quantity;
        }

        /// <summary>
        /// Registers a producible to the player with a default quantity of 1.
        /// </summary>
        /// <remarks>
        /// This is a convenience method that delegates to 
        /// <see cref="RegisterProducible(AProducibleSO, long)"/>.
        /// </remarks>
        /// <param name="producible">The producible to add.</param>
        public virtual void RegisterProducible(AProducibleSO producible)
            => RegisterProducible(producible, 1);

        /// <summary>
        /// Registers a producible to the player with the specified quantity.
        /// </summary>
        /// <remarks>
        /// This method primarily triggers an event to notify other components of the 
        /// added producible. You can override this method if needed to customize behavior.
        /// </remarks>
        /// <param name="producible">The producible to add.</param>
        /// <param name="quantity">The quantity of the producible to add.</param>
        public virtual void RegisterProducible(AProducibleSO producible, long quantity)
        {
            OnRegisterProducible.Invoke(producible, quantity);
        }

        /// <summary>
        /// Removes a registered producible from the player.
        /// </summary>
        /// <remarks>
        /// This method primarily triggers an event to notify other components about the 
        /// removal of the producible. You can override this method to customize behavior if needed.
        /// </remarks>
        /// <param name="producible">The producible to remove.</param>
        public virtual void UnregisterProducible(AProducibleSO producible)
        {
            OnUnregisterProducible?.Invoke(producible);
        }

        /// <summary>
        /// Checks if the player has registered the specified producible in the required quantity.
        /// </summary>
        /// <remarks>
        /// This method must be implemented to define how producibles are tracked and verified 
        /// against the player's inventory or attributes.
        /// </remarks>
        /// <param name="producibleQuantity">
        /// The producible and quantity to check for.
        /// </param>
        /// <returns>
        /// Returns true if the player has registered the producible in at least the specified quantity; 
        /// otherwise, false.
        /// </returns>
        public abstract bool HasRegisteredProducible(ProducibleQuantity producibleQuantity);

        /// <summary>
        /// Checks if the requirements for a producible are fullfilled, by
        /// checking if the player contains all the requirements defined
        /// on producible with the help of existing method <see cref="HasRegisteredProducible(ProducibleQuantity)"/>.
        /// To customise this override the method.
        /// </summary>
        /// <param name="producible">Producible with requirements</param>
        /// <param name="quantity">
        /// Quantity of the producible, this will be multipled requirements quantity.
        /// </param>
        /// <returns>If there are no requirements, or they are fullfilled TRUE
        /// is returned, otherwise FALSE.</returns>
        public virtual bool FulfillsRequirements(AProducibleSO producible, long quantity)
        {
            // Check if any requirements are set
            if (producible.Requirements.Length == 0) return true;

            var requirements = producible.Requirements;

            // Goes through requirements and checks if any are missing.
            // If any does miss, the loop will return early 'false'.
            foreach (ProducibleQuantity requirement in requirements)
            {
                var fullRequirement = new ProducibleQuantity(
                    requirement.Producible,
                    requirement.Quantity * quantity);
                if (!HasRegisteredProducible(fullRequirement))
                {
                    return false;
                }
            }

            return true;
        }

        #endregion

    }

}