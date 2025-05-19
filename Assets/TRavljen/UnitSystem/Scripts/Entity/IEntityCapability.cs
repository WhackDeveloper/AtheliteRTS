namespace TRavljen.UnitSystem
{

    /// <summary>
    /// Represents a capability that can be added to or configured for an entity.
    /// </summary>
    /// <remarks>
    /// Capabilities define additional behaviors or features that entities can optionally support, 
    /// allowing for flexible extension without modifying the core entity interface.
    /// </remarks>
    public interface IEntityCapability
    {
        /// <summary>
        /// Configures the necessary components or behaviors on the specified entity 
        /// to support this capability.
        /// </summary>
        /// <param name="entity">
        /// The entity to which this capability will be applied or configured.
        /// </param>
        /// <remarks>
        /// This method may add components to the entity's game object or adjust existing ones.
        /// It is typically called during entity initialization or when dynamically enabling a capability.
        /// </remarks>
        void ConfigureEntity(IEntity entity);

        /// <summary>
        /// Resets all properties to default values.
        /// </summary>
        void SetDefaultValues();
    }

}