using UnityEngine;

namespace TRavljen.UnitSystem
{

    /// <summary>
    /// Defines a contract for components that can be attached to entities within the framework. 
    /// Provides access to the entity, its transform, and its activity status.
    /// </summary>
    public interface IEntityComponent
    {
        /// <summary>
        /// Gets the associated entity to which this component belongs.
        /// </summary>
        IEntity Entity { get; }

        /// <summary>
        /// Gets the transform of the GameObject this component is attached to.
        /// </summary>
        Transform transform { get; }

        /// <summary>
        /// Indicates whether this component is currently active and operational.
        /// </summary>
        /// <remarks>
        /// Activity typically depends on the component being enabled and 
        /// may include additional conditions in concrete implementations.
        /// </remarks>
        bool IsActive { get; }
    }

}