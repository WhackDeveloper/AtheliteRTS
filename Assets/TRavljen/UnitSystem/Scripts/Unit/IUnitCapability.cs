namespace TRavljen.UnitSystem
{

    /// <summary>
    /// Represents a capability specific to units, extending the base entity capability.
    /// </summary>
    /// <remarks>
    /// This interface is a marker for defining unit-specific behaviors or features, 
    /// such as movement, combat, or resource gathering. While it currently does not 
    /// specify additional members, it serves as a foundation for extending unit 
    /// capabilities in the future.
    /// </remarks>
    public interface IUnitCapability : IEntityCapability
    {
        // Marker interface for unit-specific capabilities.
        // Extend with unit-specific methods or properties as needed.
    }

}