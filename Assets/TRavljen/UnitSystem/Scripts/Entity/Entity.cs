using UnityEngine;

namespace TRavljen.UnitSystem
{

    using Combat;
    using TRavljen.UnitSystem.Build;
    using TRavljen.UnitSystem.Garrison;
    using UnityEngine.Events;

    /// <summary>
    /// Represents a core entity in the framework, providing foundational functionality 
    /// for managing entity data, ownership, components, capabilities, and lifecycle events.
    /// </summary>
    public class Entity : MonoBehaviour, IEntity
    {

        #region Properties

        [Tooltip("The data associated with this entity, defined as a AEntitySO.")]
        [SerializeField]
        protected AEntitySO data;

        [Tooltip("The player who owns this entity.")]
        [SerializeField]
        protected APlayer owner;

        [Tooltip("Indicates whether the entity is operational and ready to be used. " +
            "This should only be false if entity requires construction or startup animation before it is ready for use.")]
        [SerializeField]
        private bool isOperational = true;

        [Tooltip("Reference the game object on which the IUnitSpawn implementation is attached to. " +
            "By default, this would be the UnitSpawnPoint component.")]
        [SerializeField]
        [RequiresType(typeof(IUnitSpawn))]
        private GameObject spawnPointObject;

        [Tooltip("Event invoked when unit is destroyed through DestroyEntity(float) method.")]
        public UnityEvent<Entity> OnDestroy = new();

        [SerializeField]
        [Tooltip("Event invoked when the entity becomes operational.")] 
        private UnityEvent<Entity> _onOperationsActive = new();

        /// <summary>
        /// Gets the player owner of this entity.
        /// </summary>
        public APlayer Owner => owner;

        /// <summary>
        /// Gets the AEntitySO data associated with this entity.
        /// </summary>
        public AEntitySO Data => data;

        public bool IsOperational => isOperational;
        
        public UnityEvent<Entity> OnOperationsActive =>_onOperationsActive;

        #endregion

        #region Entity Components

        /// <summary>
        /// Gets the health component of this entity, if one is present.
        /// </summary>
        public IHealth Health { get; private set; }
      
        /// <summary>
        /// Gets the garrison component if present.
        /// </summary>
        public IGarrisonEntity Garrison { private set; get; }
        
        /// <summary>
        /// Gets the active production component if present.
        /// </summary>
        public IActiveProduction ActiveProduction { private set; get; }
        
        /// <summary>
        /// Spawn point for units in case they are garrisoned, produced or otherwise
        /// spawned out of this entity.
        /// </summary>
        public IUnitSpawn UnitSpawn { private set; get; }

        /// <summary>
        /// Primary control for the entity. If entity supports movement and spawn point,
        /// this will be returning spawn point control. Otherwise, it will be returning
        /// the same value as for movement controls.
        /// </summary>
        public IControlEntity EntityControl { private set; get; }
        
        /// <summary>
        /// Gets the entity component if buildable.
        /// </summary>
        public IBuildableEntity Buildable { private set; get; }

        #endregion

        #region Lifecycle

        // Keeps private, OnInitialize should be used.
        private void Awake() => OnInitialize();

        /// <summary>
        /// Unity's Awake lifecycle method. Ensures initialization of the entity.
        /// </summary>
        public virtual void OnInitialize()
        {
            Garrison = GetComponent<IGarrisonEntity>();
            ActiveProduction = GetComponent<IActiveProduction>();
            Buildable = GetComponent<IBuildableEntity>();
            EntityControl = GetComponent<IControlEntity>();
            Health = GetComponent<IHealth>();

            UnitSpawn = spawnPointObject.IsNotNull() ? spawnPointObject.GetComponent<IUnitSpawn>() : GetComponent<IUnitSpawn>();

            if (UnitSpawn.IsNotNull())
                EntityControl = UnitSpawn;
        }

        #endregion
        
        #region Operational

        public virtual void SetOperational(bool operational)
        {
            if (isOperational == operational) return;

            isOperational = operational;

            if (IsOperational)
            {
                OnOperationsActive.Invoke(this);
            }
        }
        
        #endregion

        #region Components & Capabilities

        /// <inheritdoc/>
        public bool TryGetCapability<TCapability>(out TCapability capability) where TCapability : IEntityCapability
            => data.TryGetCapability(out capability);

        /// <inheritdoc/>
        public bool TryGetEntityComponent<TEntityComponent>(out TEntityComponent component) where TEntityComponent : IEntityComponent
            => TryGetComponent(out component);

        #endregion

        #region Owner Management

        /// <inheritdoc/>
        public void AssignOwner(APlayer player)
        {
            if (owner != player)
            {
                owner = player;
                EntityEvents.Instance.OnOwnerChanged.Invoke(this);
            }
        }

        /// <inheritdoc/>
        public virtual void RemoveEntityFromOwner()
        {
            if (owner == null) return;

            owner.RemoveEntity(this, true);
            AssignOwner(null);
        }

        #endregion

        #region Destruction

        /// <inheritdoc/>
        public virtual void DestroyEntity(float delay = 0f)
        {
            RemoveEntityFromOwner();

            OnDestroy.Invoke(this);

            EntityEvents.Instance.OnEntityDestroy.Invoke(this);

            // Destroy the GameObject.
            Destroy(gameObject, delay);
        }

        #endregion

        #region Internal Management

        /// <summary>
        /// Internally sets the entity's data. 
        /// This is typically used during prefab creation or runtime entity configuration.
        /// </summary>
        /// <param name="data">The ScriptableObject data to assign.</param>
        internal void SetData(AEntitySO data) => this.data = data;

        #endregion

    }

}