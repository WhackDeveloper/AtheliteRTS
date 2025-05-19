using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TRavljen.UnitSystem.Utility;
using System;
using UnityEngine.Events;

namespace TRavljen.UnitSystem
{

    /// <summary>
    /// Resource management component responsible for tracking current resources
    /// and their capacities for a player. Provides methods for modifying resources.
    /// </summary>
    [DisallowMultipleComponent]
    public class ResourceModule : APlayerModule
    {

        #region Properties

        [SerializeField, Tooltip("Specifies the default resource capacity. For all resources specified in resourceCapacities, this value is ignored.")]
        private long commonResourceCapacity = 20_000;

        /// <summary>
        /// Specifies the default resource capacity. For all resources specified in
        /// <see cref="resourceCapacities"/>, this value is ignored.
        /// </summary>
        public long CommonResourceCapacity => commonResourceCapacity;

        /// <summary>
        /// If true, resource capacity is ignored, allowing "unlimited" storage up to <see cref="long.MaxValue"/>.
        /// </summary>
        [Tooltip("Specifies if the resource capacity is ignored.")]
        public bool UnlimitedResources = false;

        [SerializeField, Tooltip("Resources defined in this list will have unlimited capacity.")]
        private List<ResourceSO> unlimitedResources = new();

        [SerializeField, HideInInspector, Tooltip("Resource IDs defined in this list will have unlimited capacity.")]
        private List<int> unlimitedResourceIDs = new();

        /// <summary>
        /// When false, all resources share a single common capacity defined by <see cref="CommonResourceCapacity"/>.
        /// When true, each resource can have its unique capacity.
        /// </summary>
        public bool UseUniqueResourceCapacities = true;

        [SerializeField, Tooltip("List defining the capacity for specific resources.")]
        protected List<ResourceQuantity> resourceCapacities = new();

        [SerializeField, Tooltip("List of current resources and their quantities.")]
        protected List<ResourceQuantity> resources = new();

        [Header("Events")]

        /// <summary>
        /// Event invoked when a resource reaches its capacity.
        /// </summary>
        public UnityEvent<ResourceSO> OnResourceFull = new();

        /// <summary>
        /// Event invoked when a resource's quantity changes.
        /// </summary>
        public UnityEvent<ResourceSO> OnResourceUpdate = new();

        /// <summary>
        /// Event invoked when a resource's capacity changes.
        /// </summary>
        public UnityEvent<ResourceSO> OnResourceCapacityUpdate = new();

        #endregion

        #region Lifecycle

        protected override void OnValidate()
        {
            base.OnValidate();

            unlimitedResourceIDs = unlimitedResources.ConvertAll(res => res.ID);
        }

        private void OnEnable()
        {
            player.OnRegisterProducible.AddListener(HandleProducibleRegister);
        }

        private void OnDisable()
        {
            player.OnRegisterProducible.RemoveListener(HandleProducibleRegister);
        }

        private void HandleProducibleRegister(AProducibleSO producible, long quantity)
        {
            if (producible is not ResourceSO resource)
                return;

            AddResource(new(resource, quantity));
        }

        #endregion

        #region Modify Resources

        /// <summary>
        /// Modifies existing resource quantity or adds a new one if none exists yet.
        /// </summary>
        public ResourceQuantity[] AddResources(ResourceQuantity[] resources)
        {
            List<ResourceQuantity> remainingResources = new();
            foreach (var resource in resources)
            {
                var remainder = AddResource(resource);

                if (remainder > 0)
                {
                    // Add remainder
                    remainingResources.Add(new()
                    {
                        Resource = resource.Resource,
                        Quantity = remainder
                    });
                }
            }

            return remainingResources.ToArray();
        }

        /// <summary>
        /// Adds a specific quantity of a resource. If capacity is reached, returns the remainder.
        /// </summary>
        public long AddResource(ResourceQuantity resourceQuantity)
        {
            if (resourceQuantity.Quantity == 0) return 0;

            int resourceID = resourceQuantity.Resource.ID;
            long capacity = GetResourceCapacity(resourceID);
            int index = resources.FindIndex(match => match.Resource.ID == resourceQuantity.Resource.ID);

            if (index == -1)
            {
                resources.Add(new(resourceQuantity.Resource, 0));
                index = resources.Count - 1;
            }

            // Check if the quantity is unlimited
            if (capacity == -1)
            {
                var resource = resources[index];
                resources[index] = new(resource.Resource, resource.Quantity + resourceQuantity.Quantity);
                return 0;
            }

            long currentResourceQuantity = resources[index].Quantity;

            if (!UseUniqueResourceCapacities)
                capacity -= GetResourcesQuantityForLimitedResources() - currentResourceQuantity;

            long newQuantity = currentResourceQuantity + resourceQuantity.Quantity;
            long newClampedQuantity = UnlimitedResources ? newQuantity : MathUtils.Min(newQuantity, capacity);

            resources[index] = new()
            {
                Resource = resourceQuantity.Resource,
                Quantity = newClampedQuantity
            };

            // Invoke resource update action when resource is updated
            OnResourceUpdate?.Invoke(resourceQuantity.Resource);

            long remainingResource = newQuantity - newClampedQuantity;

            // Check if resource is now FULL
            if (remainingResource > 0)
            {
                OnResourceFull?.Invoke(resourceQuantity.Resource);
            }

            return remainingResource;
        }

        /// <summary>
        /// Removes the provided resource quantity from existing
        /// resource if there is enough of it.
        /// Also invokes <see cref="OnResourceUpdate"/> if resource
        /// was modified.
        /// </summary>
        /// <param name="resourceQuantity">Resource to remove from module.</param>
        /// <returns>
        /// Returns 'false' if there is not enough resource available.
        /// </returns>
        public bool RemoveResource(ResourceQuantity resourceQuantity)
        {
            for (int index = 0; index < resources.Count; index++)
            {
                var currentResourceQuantity = resources[index];

                if (currentResourceQuantity.Resource.ID != resourceQuantity.Resource.ID) continue;
                
                var newQuantity = currentResourceQuantity.Quantity - resourceQuantity.Quantity;

                // Check if there are enough resources present
                if (newQuantity < 0) break;

                resources[index] = new ResourceQuantity()
                {
                    Resource = currentResourceQuantity.Resource,
                    Quantity = newQuantity
                };

                OnResourceUpdate?.Invoke(resourceQuantity.Resource);

                return true;
            }

            return false;
        }

        /// <summary>
        /// Removes the provided resources quantity from existing
        /// resources if there is enough resources available.
        /// Also invokes <see cref="OnResourceUpdate"/> for each resource
        /// that was modified.
        /// </summary>
        /// <param name="resourceQuantity">Resource to remove from module.</param>
        /// <returns>
        /// Returns 'false' if there is not enough resources available.
        /// </returns>
        public bool RemoveResources(ResourceQuantity[] resourceQuantities)
        {
            if (!HasEnoughResources(resourceQuantities)) return false;

            foreach (ResourceQuantity resource in resourceQuantities)
            {
                RemoveResource(resource);
            }
            return true;
        }

        /// <summary>
        /// Removes all present resources.
        /// </summary>
        public void RemoveAllResources()
        {
            RemoveResources(resources.ToArray());
        }

        /// <summary>
        /// Applies changes to current resource capacity. If there
        /// is no existing capacity specifies for this resource resource,
        /// then this capacity will be set as initial value.
        /// </summary>
        /// <param name="resourceCapacity">Resource capacity to modify with.</param>
        public void ModifyResourceCapacity(ResourceQuantity resourceCapacity)
        {
            var resourceID = resourceCapacity.Resource.ID;
            var index = resourceCapacities.FindIndex(
                resource => resource.Resource.ID == resourceID);

            if (index == -1)
            {
                resourceCapacities.Add(resourceCapacity);
            }
            else
            {
                var capacity = resourceCapacities[index];
                capacity.Quantity = MathUtils.Max(0, resourceCapacity.Quantity + capacity.Quantity);
                resourceCapacities[index] = capacity;
            }

            OnResourceCapacityUpdate?.Invoke(resourceCapacity.Resource);
        }

        /// <summary>
        /// Modifies <see cref="CommonResourceCapacity"/>.
        /// </summary>
        /// <param name="capacity">Capacity to add</param>
        public void ModifyCommonResourceCapacity(long capacity)
        {
            commonResourceCapacity += capacity;
            OnResourceCapacityUpdate?.Invoke(null);
        }

        /// <summary>
        /// Set the <see cref="CommonResourceCapacity"/>.
        /// </summary>
        /// <param name="newCapacity">New capacity</param>
        public void SetCommonResourceCapacity(long newCapacity)
        {
            commonResourceCapacity = newCapacity;
            OnResourceCapacityUpdate?.Invoke(null);
        }

        /// <summary>
        /// Setting resource capacity by overriding existing one
        /// or adding a new one if one does not exist.
        /// </summary>
        /// <param name="resourceQuantity">New resource capacity.</param>
        public void SetResourceCapacity(ResourceQuantity resourceQuantity)
        {
            int index = GetIndexFromList(resourceQuantity.Resource.ID, resourceCapacities);

            if (index != -1)
            {
                resourceCapacities[index] = resourceQuantity;
            }
            else
            {
                resourceCapacities.Add(resourceQuantity);
            }

            OnResourceCapacityUpdate?.Invoke(resourceQuantity.Resource);
        }

        /// <summary>
        /// Setting current resource quantity by overriding existing one,
        /// or adding a new one if one does not exist.
        /// </summary>
        /// <param name="resourceQuantity">Overriding resource quantity.</param>
        public void SetResourceQuantity(ResourceQuantity resourceQuantity)
        {
            var resourceID = resourceQuantity.Resource.ID;
            var index = resources.FindIndex(
                resource => resource.Resource.ID == resourceID);

            if (index != -1)
            {
                resources[index] = resourceQuantity;
            }
            else
            {
                resources.Add(resourceQuantity);
            }

            OnResourceUpdate?.Invoke(resourceQuantity.Resource);
        }

        #endregion

        #region Calculate Resources

        /// <summary>
        /// Compares the quantity of the currently available resource.
        /// </summary>
        /// <param name="resource">Resource used for matching</param>
        /// <param name="quantity">Quantity used for comparison</param>
        /// <returns>Returns 'false' if there is not enough resource available.</returns>
        public bool HasEnoughStorage(AProducibleSO resource, long quantity)
        {
            long resourceCapacity = GetResourceCapacity(resource.ID);

            for (int index = 0; index < resources.Count; index++)
            {
                var currentResourceQuantity = resources[index];

                if (currentResourceQuantity.Resource.ID == resource.ID)
                {
                    return currentResourceQuantity.Quantity + quantity <= resourceCapacity;
                }
            }

            // Not entry of the resource yet
            return quantity <= resourceCapacity;
        }

        /// <summary>
        /// Compares the quantity of the currently available resource.
        /// </summary>
        /// <param name="resourceQuantity">Resource and its quantity used for matching.</param>
        /// <returns>Returns 'false' if there is not enough resource available.</returns>
        public bool HasEnoughResource(ResourceQuantity resourceQuantity)
        {
            for (int index = 0; index < resources.Count; index++)
            {
                var currentResourceQuantity = resources[index];

                if (currentResourceQuantity.Resource.ID == resourceQuantity.Resource.ID)
                {
                    return currentResourceQuantity.Quantity >= resourceQuantity.Quantity;
                }
            }

            return false;
        }

        /// <summary>
        /// Checks for quantity of the resources available.
        /// </summary>
        /// <param name="resources">Resources and its quantities used for matching.</param>
        /// <returns>Returns 'false' if there is not enough resources available.</returns>
        public bool HasEnoughResources(ResourceQuantity[] resources)
        {
            foreach (ResourceQuantity resource in resources)
            {
                if (!HasEnoughResource(resource))
                    return false;
            }

            return true;
        }

        /// <summary>
        /// Retrieves the current capacity for a specific resource.
        /// </summary>
        /// <param name="resourceID">ID used for matching with <see cref="resourceCapacities"/></param>
        /// <returns>Returns the resource capacity allowed for this resource module.</returns>
        public long GetResourceCapacity(int resourceID)
        {
            // Check if all resources are unlimited or perhaps this one is specified.
            if (UnlimitedResources) return -1;
            if (unlimitedResourceIDs.Contains(resourceID)) return -1;

            // If unique resources are not used, take common quantity.
            if (!UseUniqueResourceCapacities) return commonResourceCapacity;

            foreach (var resource in resourceCapacities)
            {
                if (resource.Resource.ID == resourceID)
                {
                    return resource.Quantity;
                }
            }

            return commonResourceCapacity;
        }

        /// <summary>
        /// Returns the current resource quantity for the provided
        /// resource ID.
        /// </summary>
        /// <param name="resourceID">ID used for matching the resource</param>
        /// <returns>Returns the current resource quantity for the given ID.</returns>
        public long GetResourceQuantity(int resourceID)
        {
            foreach (var resource in resources)
            {
                var producible = resource.Resource;
                if (producible != null && producible.ID == resourceID)
                    return resource.Quantity;
            }

            return 0;
        }

        /// <summary>
        /// Checks if the resource capacity is reached for the requested resource
        /// </summary>
        /// <param name="resourceID">Resource ID to check.</param>
        public bool IsResourceCapacityReached(int resourceID)
        {
            if (UnlimitedResources) return false;
            if (unlimitedResourceIDs.Contains(resourceID)) return false;

            if (UseUniqueResourceCapacities)
            {
                // If this gets below zero, then we still have some space.
                return GetResourceQuantity(resourceID) - GetResourceCapacity(resourceID) >= 0;
            }

            return GetResourcesQuantityForLimitedResources() >= commonResourceCapacity;
        }

        /// <summary>
        /// Calculates the sum of all resources that are not unlimited in capacity.
        /// This is generally useful when these resources share a common storage.
        /// </summary>
        /// <returns>Returns sum of all limited resources.</returns>
        public long GetResourcesQuantityForLimitedResources()
        {
            long groupQuantity = 0;

            foreach (var resource in resources)
            {
                // Ignore unlimited producibles
                if (unlimitedResourceIDs.Contains(resource.Resource.ID))
                    continue;

                groupQuantity += resource.Quantity;
            }

            return groupQuantity;
        }

        #endregion

        #region Convenience

        /// <summary>
        /// Current resources.
        /// <para>
        /// This quantity can be set below 0 and will generally behave
        /// as it would with 0. This cannot happen with taking resources, but
        /// only with <see cref="SetResourceQuantity(ResourceQuantity)"/>
        /// method if this is ever desired.
        /// </para>
        /// </summary>
        public ResourceQuantity[] GetResources() => resources.ToArray();

        /// <summary>
        /// Current resources capacity. If resource is not specified here,
        /// default value is used (<see cref="CommonResourceCapacity"/>) for
        /// storage capacity.
        /// </summary>
        public ResourceQuantity[] GetResourceCapacity() => resourceCapacities.ToArray();

        /// <summary>
        /// Finds the index of the specified <paramref name="resourceID"/>.
        /// </summary>
        /// <param name="resourceID">ID of the producible</param>
        /// <param name="resources">List of producibles on which the index
        /// will be searched</param>
        /// <returns>Returns index of the producible. If index is negative (-1)
        /// then the producible was not found the in the list.</returns>
        private int GetIndexFromList(int resourceID, List<ResourceQuantity> resources)
        {
            for(int index = 0; index < resources.Count; index++)
            {
                if (resources[index].Resource.ID == resourceID)
                    return index;
            }

            return -1;
        }

        #endregion

    }

}