using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TRavljen.UnitSystem
{

    /// <summary>
    /// Provides a abstract foundation for entity components in the framework. 
    /// This class serves as a foundation for components attached to entities, 
    /// offering basic lifecycle management and a mechanism to access the associated entity.
    /// </summary>
    public abstract class AEntityComponent : MonoBehaviour, IEntityComponent
    {

        #region Properties

        /// <summary>
        /// Entity to which this component is attached to.
        /// </summary>
        private IEntity entity;

        /// <summary>
        /// Gets the entity associated with this component.
        /// This is typically set automatically during initialization.
        /// </summary>
        public IEntity Entity => entity;

        /// <summary>
        /// Indicates whether the component is active and operational.
        /// This can be overridden by subclasses for custom logic.
        /// </summary>
        /// <remarks>
        /// By default, this property returns <see cref="isActiveAndEnabled"/>.
        /// Subclasses can override this to include additional conditions 
        /// such as operational state or other contextual requirements.
        /// </remarks>
        public virtual bool IsActive => isActiveAndEnabled;

        /// <summary>
        /// Gets the owning player of the associated entity.
        /// </summary>
        public APlayer Owner => entity.Owner;

        #endregion

        #region Lifecycle

        /// <summary>
        /// Unity's Awake lifecycle method.
        /// Initializes the component and its dependencies.
        /// </summary>
        protected virtual void Awake()
        {
            OnInitialize();
        }

        /// <summary>
        /// Performs component initialization. 
        /// Ensures the <see cref="Entity"/> property is assigned to the associated entity.
        /// </summary>
        protected virtual void OnInitialize()
        {
            // Attempt to assign the entity if it has not already been set.
            entity ??= GetComponent<IEntity>();
        }

        #endregion
    }


}