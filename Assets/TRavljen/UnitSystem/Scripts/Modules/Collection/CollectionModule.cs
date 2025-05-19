using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TRavljen.UnitSystem.Collection
{

    /// <summary>
    /// Manages the resource collection process for a player, including tracking 
    /// resource collectors and depots, and handling interactions with resource nodes.
    /// </summary>
    /// <remarks>
    /// This module allows units with collection capabilities to interact with 
    /// resource nodes and deposit resources into depots. It can be customized 
    /// to prioritize the nearest nodes or randomly select from available nodes.
    /// </remarks>
    [DisallowMultipleComponent]
    public class CollectionModule : APlayerModule
    {

        #region Properties

        [Tooltip("Specifies the layer used for detecting nearby resource nodes.")]
        [SerializeField]
        internal LayerMask resourceNodeLayer = ~0;

        [Tooltip("Defines the search range for the next resource.")]
        [SerializeField]
        private float nextNodeRange = 30;

        [Tooltip("If enabled, always selects the nearest resource node instead of random selection.")]
        [SerializeField]
        private bool alwaysPickNearestNode = false;

        [Tooltip("Tracks active resource collectors.")]
        [SerializeField]
        private List<IResourceCollector> collectors = new();

        [Tooltip("Tracks active resource depots.")]
        [SerializeField]
        private List<IResourceDepot> resourceDepots = new();
        
        private IResourceNodeFinder NodeFinder;

        #endregion

        #region Lifecycle

        protected override void Awake()
        {
            base.Awake();

            NodeFinder ??= new ResourceNodeFinder(resourceNodeLayer);
            collectors = new();
            resourceDepots = new();
        }

        private void OnEnable()
        {
            player.OnUnitAdded.AddListener(UnitAdded);
            player.OnUnitRemoved.AddListener(UnitRemoved);

        }

        private void OnDisable()
        {
            player.OnUnitAdded.RemoveListener(UnitAdded);
            player.OnUnitRemoved.RemoveListener(UnitRemoved);
        }

        protected override void OnValidate()
        {
            base.OnValidate();

            nextNodeRange = Mathf.Max(0, nextNodeRange);
        }

        #endregion

        #region Public Interface

        /// <summary>
        /// Gets the available collectors that are idle and capable of collecting resources.
        /// </summary>
        public IResourceCollector[] GetAvailableCollectors()
        {
            var system = Task.TaskSystem.GetOrCreate();

            List<IResourceCollector> availableCollectors = new();

            foreach (var collector in collectors)
            {
                // Check if collector is active (like in garrison, they may be disabled)
                if (!collector.Unit.gameObject.activeSelf)
                    continue;

                // Check if collector has any active tasks
                if (system.HasActiveTask(collector.Unit.gameObject) == false)
                {
                    availableCollectors.Add(collector);
                }
            }

            return availableCollectors.ToArray();
        }

        /// <summary>
        /// Finds a nearby resource node for a specified resource type. It uses the default
        /// <see cref="nextNodeRange"/> for the search.
        /// </summary>
        /// <param name="position">The position from which to search.</param>
        /// <param name="resource">The resource type to search for.</param>
        /// <param name="node">The closest or randomly selected resource node found.</param>
        /// <returns>True if a resource node was found; otherwise, false.</returns>
        public bool FindNearbyNode(Vector3 position, ResourceSO resource, out IResourceNode node)
        {
            return FindNearbyNode(position, resource, nextNodeRange, out node);
        }

        /// <summary>
        /// Finds a nearby resource node for a specified resource type.
        /// </summary>
        /// <param name="position">The position from which to search.</param>
        /// <param name="resource">The resource type to search for.</param>
        /// <param name="node">The closest or randomly selected resource node found.</param>
        /// <param name="range">Maximal range of the search.</param>
        /// <returns>True if a resource node was found; otherwise, false.</returns>
        public bool FindNearbyNode(Vector3 position, ResourceSO resource, float range, out IResourceNode node)
        {
            if (alwaysPickNearestNode)
                return NodeFinder.FindClosest(position, resource, range, out node);

            var nearbyNodes = NodeFinder.FindNearby(position, resource, range);

            // Warning: There is no checks for CanExecute tasks here, if this turns out to be a problem
            // add them. But generally once a unit can deal with resource node with specific resource
            // it should be able to deal with others than contain the same one.
            if (nearbyNodes.Length > 0)
            {
                node = nearbyNodes[Random.Range(0, nearbyNodes.Length)];
                return true;
            }

            node = null;
            return false;
        }

        /// <summary>
        /// Finds the nearest resource depot capable of accepting a specified resource.
        /// </summary>
        /// <param name="position">The starting position for the search.</param>
        /// <param name="resource">The resource to deposit.</param>
        /// <param name="depot">The closest resource depot found.</param>
        /// <returns>True if a resource depot was found; otherwise, false.</returns>
        public bool FindNearestDepot(Vector3 position, ResourceSO resource, out IResourceDepot depot)
        {
            List<(IResourceDepot depot, float distance)> availableDepots = new();
            foreach (var _depot in resourceDepots)
            {
                if (_depot.CanDepositResource(resource))
                    availableDepots.Add((_depot, Vector3.Distance(position, _depot.transform.position)));
            }

            // If there are no deposits left, player is probably losing.
            if (availableDepots.Count > 0)
            {
                availableDepots.Sort((a, b) => a.distance.CompareTo(b.distance));
                depot = availableDepots[0].depot;
                return true;
            }

            depot = null;
            return false;
        }

        #endregion

        /// <summary>
        /// Adds a unit to the module if it is a resource collector or depot.
        /// </summary>
        /// <param name="unit">The unit to add.</param>
        private void UnitAdded(IUnit unit)
        {
            if (unit?.ResourceCollector.IsNotNull() == true)
            {
                collectors.Add(unit.ResourceCollector);
            }

            if (unit?.ResourceDepot.IsNotNull() == true)
            {
                resourceDepots.Add(unit.ResourceDepot);
            }
        }

        /// <summary>
        /// Removes a unit from the module if it is a resource collector or depot.
        /// </summary>
        /// <param name="unit">The unit to remove.</param>
        private void UnitRemoved(IUnit unit)
        {
            collectors.Remove(unit?.ResourceCollector);
            resourceDepots.Remove(unit?.ResourceDepot);
        }

    }
}