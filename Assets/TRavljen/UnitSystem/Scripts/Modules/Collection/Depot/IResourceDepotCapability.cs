namespace TRavljen.UnitSystem.Collection
{

    /// <summary>
    /// Capability interface for resource depots, defining the types of resources they support for deposit.
    /// </summary>
    public interface IResourceDepotCapability : IEntityCapability
    {
        /// <summary>
        /// Array of resources supported for depositing. 
        /// If empty, all resource types are accepted.
        /// </summary>
        ResourceSO[] SupportedResources { get; }
    }

}