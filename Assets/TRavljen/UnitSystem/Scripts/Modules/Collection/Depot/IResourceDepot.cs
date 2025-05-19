using TRavljen.UnitSystem.Interactions;

namespace TRavljen.UnitSystem.Collection
{

    /// <summary>
    /// Interface representing a resource depot that interacts with resource-carrying units. 
    /// Allows validation and deposit of resources into the player's resource system.
    /// </summary>
    public interface IResourceDepot : IUnitInteracteeComponent
    {

        /// <summary>
        /// Determines if a specific resource type can be deposited in this depot.
        /// </summary>
        /// <param name="resource">The resource to check.</param>
        /// <returns>
        /// Returns true if the resource can be deposited, false otherwise.
        /// </returns>
        public bool CanDepositResource(ResourceSO resource);

        /// <summary>
        /// Deposits a specified amount of resources into the player's resource system.
        /// </summary>
        /// <param name="resourceAmount">The amount and type of resource to deposit.</param>
        /// <returns>
        /// The remaining resource amount that could not be deposited due to capacity limitations.
        /// </returns>
        public long DepositResources(ResourceQuantity resourceAmount);

        /// <summary>
        /// Array of resources supported for depositing. 
        /// If empty, all resources are accepted.
        /// </summary>
        public ResourceSO[] SupportedResources { get; }
    }

}