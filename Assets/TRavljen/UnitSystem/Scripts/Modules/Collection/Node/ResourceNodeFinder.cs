using TRavljen.UnitSystem.Interactions;
using UnityEngine;
using UnityEngine.Serialization;

namespace TRavljen.UnitSystem.Collection
{
    
    using Utility;
    
    /// <summary>
    /// Utility class for scanning and finding nearby resource nodes within a specified range.
    /// Provides methods to search for all resource nodes or the closest one based on specified criteria.
    /// </summary>
    [System.Serializable]
    public class ResourceNodeFinder: IResourceNodeFinder
    {

        [SerializeField]
        private LayerMask layerMask = ~0;
        
        [SerializeReference]
        private NearbyColliderFinder nearbyColliderFinder;

        public ResourceNodeFinder(LayerMask layerMask, bool useNonAllocating = false, int allocationSize = 100)
        {
            this.layerMask = layerMask;
            nearbyColliderFinder = new NearbyColliderFinder(layerMask, useNonAllocating, allocationSize);
        }

        public IResourceNode[] FindNearby(Vector3 position, ResourceSO resource, float range)
        {
            return nearbyColliderFinder.FindNearby<IResourceNode>(position, range, (node) => ValidateNode(resource, node));
        }

        public bool FindClosest(Vector3 position, ResourceSO resource, float range, out IResourceNode node)
        {
            return nearbyColliderFinder.FindClosest(position, range, out node, (node) => ValidateNode(resource, node));
        }

        private static bool ValidateNode(ResourceSO resource, IResourceNode node)
        {
            return (resource == null || node.ResourceAmount.Resource.ID == resource.ID) &&
                   !node.IsDepleted &&
                   // If node is limiting interactions, make sure limit is not reached.
                   (node is not ILimitedUnitInteractionTarget target || !target.HasReachedLimit());
        }
    }

}