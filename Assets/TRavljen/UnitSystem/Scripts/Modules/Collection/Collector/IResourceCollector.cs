using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TRavljen.UnitSystem.Collection
{

    /// <summary>
    /// Defines the interface for resource collectors, outlining their interaction
    /// with resource nodes and supported resource types.
    /// </summary>
    public interface IResourceCollector : IUnitComponent
    {
        /// <summary>
        /// Initiates the collection process from the specified resource node.
        /// </summary>
        /// <param name="node">The resource node to collect from.</param>
        void CollectResource(IResourceNode node);

        /// <summary>
        /// Determines whether the collector can collect a specific type of resource.
        /// </summary>
        /// <param name="resource">The resource type to check.</param>
        /// <returns>Returns true if the collector can collect the resource, otherwise false.</returns>
        bool CanCollect(ResourceSO resource);
    }

}