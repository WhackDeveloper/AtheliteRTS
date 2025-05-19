namespace TRavljen.UnitSystem
{

    /// <summary>
    /// Represents a specific quantity of a resource.
    /// </summary>
    [System.Serializable]
    public struct ResourceQuantity
    {

        /// <summary>
        /// A reusable, immutable empty array of resource quantities. 
        /// Use this to avoid allocating new empty arrays in scenarios where 
        /// an empty array must be returned by design.
        /// </summary>
        public static readonly ResourceQuantity[] EmptyArray = new ResourceQuantity[0];

        /// <summary>
        /// The type of resource associated with this quantity.
        /// </summary>
        public ResourceSO Resource;

        /// <summary>
        /// The quantity of the specified resource.
        /// </summary>
        [PositiveLong]
        public long Quantity;

        /// <summary>
        /// Initializes a new instance of the <see cref="ResourceQuantity"/> struct.
        /// </summary>
        /// <param name="resource">The type of resource.</param>
        /// <param name="quantity">The quantity of the resource.</param>
        public ResourceQuantity(ResourceSO resource, long quantity)
        {
            Resource = resource;
            Quantity = quantity;
        }

        /// <summary>
        /// Computes the full cost of the resource based on its base cost and the current quantity.
        /// </summary>
        /// <returns>
        /// An array of <see cref="ResourceQuantity"/> representing the total cost for the current quantity.
        /// </returns>
        public readonly ResourceQuantity[] GetFullCost()
        {
            return GetFullCost(Resource.Cost, Quantity);
        }

        /// <summary>
        /// Computes the full cost of the resource based on its base cost and a specified quantity.
        /// </summary>
        /// <param name="quantity">The specified quantity to calculate the cost for.</param>
        /// <returns>
        /// An array of <see cref="ResourceQuantity"/> representing the total cost for the specified quantity.
        /// </returns>
        public readonly ResourceQuantity[] GetFullCost(long quantity)
        {
            return GetFullCost(Resource.Cost, quantity);
        }

        /// <summary>
        /// Computes the full cost of a set of resources based on a specified quantity.
        /// </summary>
        /// <param name="cost">The base cost as an array of <see cref="ResourceQuantity"/>.</param>
        /// <param name="quantity">The specified quantity to calculate the cost for.</param>
        /// <returns>
        /// An array of <see cref="ResourceQuantity"/> representing the total cost for the specified quantity.
        /// </returns>
        public static ResourceQuantity[] GetFullCost(ResourceQuantity[] cost, long quantity)
        {
            ResourceQuantity[] fullCost = new ResourceQuantity[cost.Length];

            for (int index = 0; index < cost.Length; index++)
            {
                var resource = cost[index];
                resource.Quantity *= quantity;
                fullCost[index] = resource;
            }

            return fullCost;
        }
    }

}