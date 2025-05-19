namespace TRavljen.UnitSystem
{

    /// <summary>
    /// Interface for components associated with a unit.
    /// </summary>
    /// <remarks>
    /// Extends <see cref="IEntityComponent"/> to add a reference to the owning <see cref="Unit"/> entity.
    /// Serves as a marker for components that are explicitly tied to unit-specific functionality.
    /// </remarks>
    public interface IUnitComponent : IEntityComponent
    {
        /// <summary>
        /// Gets the unit associated with this component.
        /// </summary>
        Unit Unit { get; }
    }

}