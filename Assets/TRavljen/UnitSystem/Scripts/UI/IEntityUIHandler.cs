using UnityEngine;

namespace TRavljen.UnitSystem
{

    /// <summary>
    /// Data for the entity UI layout handler.
    /// </summary>
    public struct EntityUILayoutData
    {
        /// <summary>
        /// Section of the container.
        /// </summary>
        public int SectionIndex;
        /// <summary>
        /// Reference to the layout root element / container.
        /// </summary>
        public Transform Container;
    }

    /// <summary>
    /// Defines an interface for handling UI elements associated with an entity.
    /// </summary>
    public interface IEntityUIHandler
    {
        /// <summary>
        /// Builds UI elements associated with the given container.
        /// </summary>
        /// <param name="layoutData">The data of container where elements can be instantiated.</param>
        /// <returns>Returns true if any elements were added to container; otherwise false.</returns>
        bool BuildUIElements(EntityUILayoutData layoutData);
    }


}