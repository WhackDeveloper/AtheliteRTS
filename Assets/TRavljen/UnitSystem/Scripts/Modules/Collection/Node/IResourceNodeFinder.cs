using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TRavljen.UnitSystem.Collection
{
    
    public interface IResourceNodeFinder
    {
        /// <summary>
        /// Finds nearby resource nodes within the specified range.
        /// </summary>
        /// <param name="position">The position from which to search for resource nodes.</param>
        /// <param name="resource">The specific resource type to filter by, or null to include all types.</param>
        /// <param name="range">The search radius in units.</param>
        /// <returns>
        /// An array of <see cref="ResourceNode"/> objects within the specified range, sorted by distance.
        /// </returns>
        public IResourceNode[] FindNearby(Vector3 position, ResourceSO resource, float range);
       
        /// <summary>
        /// Finds the closest resource node within the specified range.
        /// </summary>
        /// <param name="position">The position from which to search for the closest resource node.</param>
        /// <param name="resource">The specific resource type to filter by, or null to include all types.</param>
        /// <param name="range">The search radius in units.</param>
        /// <param name="node">
        /// The closest <see cref="ResourceNode"/> found, or null if no matching resource nodes are found.
        /// </param>
        /// <returns>True if a matching resource node is found, otherwise false.</returns>
        public bool FindClosest(Vector3 position, ResourceSO resource, float range, out IResourceNode node);

    }
    
}