using System;
using System.Collections.Generic;
using UnityEngine;

namespace TRavljen.UnitSystem
{

    /// <summary>
    /// Represents the base ScriptableObject for defining entity data and capabilities.
    /// This class serves as a data layer for entities, allowing configuration of capabilities and 
    /// integration with prefabs. It provides methods for creating and configuring prefabs dynamically.
    /// </summary>
    public abstract class AEntitySO : AProducibleSO, IEntityPrefabCreatable
    {

        #region Fields

        [Header("Entity")]

        [SerializeReference]
        [Tooltip("Collection of capabilities associated with the entity. " +
            "These define the entity's configurable behaviors or attributes.")]
        private IEntityCapability[] capabilities = new IEntityCapability[0];

        #endregion

        #region Properties

        /// <summary>
        /// Gets the collection of capabilities associated with the entity.
        /// </summary>
        public IEntityCapability[] Capabilities => capabilities;

        #endregion

        #region Capability Management

        /// <summary>
        /// Attempts to retrieve a capability of the specified type from the entity's capabilities.
        /// </summary>
        /// <typeparam name="Capability">The type of capability to retrieve.</typeparam>
        /// <param name="capability">
        /// The retrieved capability if it exists; otherwise, the default value for the type.
        /// </param>
        /// <returns>
        /// <c>true</c> if the capability exists; otherwise, <c>false</c>.
        /// </returns>
        public virtual bool TryGetCapability<Capability>(out Capability capability) where Capability : IEntityCapability
        {
            foreach (var entityCapability in capabilities)
            {
                if (entityCapability is Capability _capability)
                {
                    capability = _capability;
                    return true;
                }
            }

            capability = default;
            return false;
        }

        /// <summary>
        /// Adds a new capability to the entity's list of capabilities.
        /// </summary>
        /// <param name="capability">The capability to add.</param>
        public virtual void AddCapability(IEntityCapability capability)
        {
            capabilities = new List<IEntityCapability>(capabilities) { capability }.ToArray();
        }

        /// <summary>
        /// Create and add new capability to the entity's list of capabilities of
        /// the specified type. Initializer with no parameters must be available for this method to work.
        /// </summary>
        /// <typeparam name="CapabilityType">Type of the capability.</typeparam>
        /// <returns>Returns capability if initialisation was successful; otherwise false..</returns>
        public virtual CapabilityType AddCapability<CapabilityType>() where CapabilityType : IEntityCapability
        {
            try
            {
                var capability = (CapabilityType)Activator.CreateInstance(typeof(CapabilityType));
                capability.SetDefaultValues();
                AddCapability(capability);
                return capability;
            }
            catch
            {
                return default;
            }
        }

        #endregion

        #region Prefab Management

        /// <summary>
        /// Creates a prefab GameObject for the entity with the specified name.
        /// </summary>
        /// <param name="name">The name of the GameObject to create.</param>
        /// <returns>A new GameObject configured as the entity's prefab.</returns>
        public virtual GameObject CreatePrefab(string name)
        {
            return CreatePrefab<Entity>(name);
        }

        /// <summary>
        /// Configures a prefab GameObject by applying entity-specific data and capabilities.
        /// </summary>
        /// <param name="prefab">The GameObject to configure.</param>
        /// <exception cref="ArgumentException">
        /// Thrown if the prefab does not contain an <see cref="Entity"/> component.
        /// </exception>
        public virtual void ConfigurePrefab(GameObject prefab)
        {
            if (prefab.TryGetComponent(out Entity entity))
            {
                ConfigureComponents(entity, prefab);
            }
            else
            {
                throw new ArgumentException("Prefab with Entity present is expected");
            }
        }

        /// <summary>
        /// Creates a prefab GameObject for the entity using a specified entity type.
        /// </summary>
        /// <typeparam name="EntityType">The type of the entity component to attach to the prefab.</typeparam>
        /// <param name="name">The name of the GameObject to create.</param>
        /// <returns>A new GameObject configured as the entity's prefab.</returns>
        protected virtual GameObject CreatePrefab<EntityType>(string name) where EntityType : Entity
        {
            // Create base object with Entity Type component.
            GameObject root = new GameObject(name);
            var entity = root.AddComponent<EntityType>();

            // Create model placeholder
            GameObject cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
            cube.transform.SetParent(root.transform);
            cube.transform.localPosition = Vector3.zero;
            cube.transform.localScale = Vector3.one;
            cube.name = "Model";

            if (cube.TryGetComponent(out Collider collider))
            {
                GameObject.DestroyImmediate(collider);
            }

            // Configure components
            ConfigureComponents(entity, root);

            return root;
        }

        /// <summary>
        /// Configures the components of an entity GameObject by applying the entity's capabilities.
        /// </summary>
        /// <param name="entity">The entity component attached to the GameObject.</param>
        /// <param name="root">The root GameObject of the entity.</param>
        protected virtual void ConfigureComponents(Entity entity, GameObject root)
        {
            entity.SetData(this);

            foreach (var capability in capabilities)
            {
                capability.ConfigureEntity(entity);
            }
        }

        #endregion

    }

}