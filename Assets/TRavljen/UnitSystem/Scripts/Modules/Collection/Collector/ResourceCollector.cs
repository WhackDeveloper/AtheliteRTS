using TRavljen.EditorUtility;
using UnityEngine;
using UnityEngine.Events;

namespace TRavljen.UnitSystem.Collection
{
    using Utility;
    using Interactions;
    using Task;

    /// <summary>
    /// Represents a unit component responsible for collecting resources from 
    /// resource nodes and depositing them into the player's resource system.
    /// </summary>
    [DisallowMultipleComponent]
    public class ResourceCollector : AUnitComponent, IResourceCollector, IUnitInteractorComponent, ITaskProvider
    {

        #region Fields

        [Tooltip("The currently collected resource and its quantity.")]
        [SerializeField, DisableInInspector]
        private ResourceQuantity collectedResource;

        [Tooltip("Delay after depositing resources, allowing other actions to be performed.")]
        [SerializeField, Range(0, 20)]
        private float delayAfterDeposit = 0.3f;

        [Tooltip("Whether the resource collector should follow the target node dynamically.")]
        [SerializeField]
        private bool followTarget = true;

        [Tooltip("Event invoked when the collector is in position and starts collecting from a resource node.")]
        public UnityEvent<IResourceNode> OnStartedCollecting = new();
        
        [Tooltip("Event invoked when the collector stops collecting from a resource node.")]
        public UnityEvent<IResourceNode> OnFinishedCollecting = new();
        
        [Tooltip("Event invoked when certain amount of resources collected.")]
        public UnityEvent<IResourceNode, long> OnCollectedResource = new();

        [Tooltip("Event invoked when a resource is successfully deposited.")]
        public UnityEvent<ProducibleQuantity> OnResourceDeposited = new();

        #endregion

        #region Properties

        /// <summary>
        /// Task for collecting resource from a node.
        /// </summary>
        private readonly ResourceCollectingTask collectingTask = new();

        /// <summary>
        /// Task for depositing a resource to a resource depot.
        /// </summary>
        private readonly DepositResourceTask depositResourceTask = new();

        /// <summary>
        /// Combined all supported tasks.
        /// </summary>
        private ITask[] collectionTasks;

        /// <summary>
        /// Data of the capability.
        /// </summary>
        private IResourceCollectorCapability data;

        #endregion

        #region Getters

        /// <summary>
        /// The currently collected resource and its quantity.
        /// </summary>
        public ResourceQuantity CollectedResource
        {
            get => collectedResource;
            set => collectedResource = value;
        }

        /// <summary>
        /// Checks if carrying capacity is full.
        /// </summary>
        public bool IsCapacityFull => collectedResource.Quantity >= data.Capacity;

        /// <summary>
        /// The types of resources that the collector can gather.
        /// </summary>
        public ResourceSO[] SupportedResources => data.SupportedResources;

        /// <summary>
        /// The time interval between resource collection actions.
        /// </summary>
        public float CollectInterval => data.CollectInterval;

        /// <summary>
        /// The amount of resource collected in a single action.
        /// </summary>
        public int CollectAmount => data.CollectAmount;

        /// <summary>
        /// Whether the resource collector should follow the target node dynamically.
        /// </summary>
        public bool FollowTarget => followTarget;

        /// <summary>
        /// The type of collection behavior used by the collector.
        /// </summary>
        public CollectType CollectType => data.CollectType;

        /// <summary>
        /// Delay after depositing resources.
        /// </summary>
        public float DelayAfterDeposit => delayAfterDeposit;

        #endregion

        #region IUnitInteractor

        /// <summary>
        /// The minimum range required to interact with a resource node.
        /// </summary>
        public float MinInteractionRange => 0;

        /// <summary>
        /// The maximum range within which the collector can interact with a resource node.
        /// </summary>
        public float MaxInteractionRange => data.Range;

        /// <summary>
        /// The position of the resource collector in world space.
        /// </summary>
        public Vector3 Position => transform.position;

        #endregion

        #region Lifecycle

        /// <summary>
        /// Initializes the component, setting up required capabilities and tasks.
        /// </summary>
        protected override void OnInitialize()
        {
            base.OnInitialize();

            if (!Unit.TryGetCapability(out data))
            {
                Debug.LogError("Unit is expected to have IResourceCollectorCapability in order to collect resources.");
                return;
            }

            collectionTasks = new ITask[2]
            {
                collectingTask,
                depositResourceTask
            };
        }

        #endregion

        #region Resource Behaviour

        /// <summary>
        /// Clears the currently collected resources from the collector.
        /// </summary>
        public void ClearResource()
        {
            collectedResource.Resource = null;
            collectedResource.Quantity = 0;
        }

        /// <summary>
        /// Collects resources from a specified resource node.
        /// </summary>
        /// <param name="node">The resource node to collect resources from.</param>
        public void CollectResource(IResourceNode node)
        {
            long collectAmount = data.CollectAmount;

            // Take only what is there, not more.
            collectAmount = MathUtils.Min(collectAmount, node.ResourceAmount.Quantity);

            // Collector might not be able to store full quantity, thus we need remainder
            long remainder = Collect(node.ResourceAmount.Resource, collectAmount);

            // Remove collected quantity from resource.
            long toRemove = collectAmount - remainder;

            // Invoke an event
            OnCollectedResource.Invoke(node, toRemove);

            node.ReduceResource(toRemove);
        }

        /// <summary>
        /// Checks whether the collector can gather a specific type of resource.
        /// </summary>
        /// <param name="resource">The resource to check.</param>
        /// <returns>
        /// Returns true if the resource can be collected, otherwise false.
        /// </returns>
        public bool CanCollect(ResourceSO resource)
        {
            var resources = data.SupportedResources;

            // No limits
            if (resources == null || resources.Length == 0) return true;

            foreach (var res in SupportedResources)
                if (res.ID == resource.ID) return true;

            return false;
        }

        /// <summary>
        /// Collects a specified quantity of a resource, respecting the collector's capacity.
        /// </summary>
        /// <param name="resource">The resource to collect.</param>
        /// <param name="quantity">The quantity to collect.</param>
        /// <returns>
        /// The remainder of the quantity that could not be collected due to capacity limitations.
        /// </returns>
        private long Collect(ResourceSO resource, long quantity)
        {
            if (IsCapacityFull) return quantity;

            // Apply capacity to not overload the unit
            long capacity = GetCapacityFor(resource);

            // Calculate remainder, then update quantity to be applied.
            long remainder = MathUtils.Max(0, quantity - capacity);
            quantity = MathUtils.Min(quantity, capacity);

            if (collectedResource.Resource != null &&
                collectedResource.Resource.ID == resource.ID)
            {
                collectedResource.Quantity += quantity;
            }
            else
            {
                collectedResource.Resource = resource;
                collectedResource.Quantity = quantity;
            }
             
            return remainder;
        }

        /// <summary>
        /// Deposits the currently held resources directly into the owner's resource pool.
        /// </summary>
        /// <returns>The remainder of resources that could not be deposited.</returns>
        public long DepositResource()
        {
            var collectedResource = CollectedResource;

            long remainder = Unit.Owner.AddResource(collectedResource);
            long deposited = collectedResource.Quantity - remainder;
            collectedResource.Quantity = remainder;

            OnResourceDeposited.Invoke(new(collectedResource.Resource, deposited));
            CollectedResource = collectedResource;

            return remainder;
        }

        /// <summary>
        /// Retrieves the remaining capacity for a specific resource type.
        /// </summary>
        /// <param name="producible">The resource type to check.</param>
        /// <returns>The remaining capacity available for the specified resource.</returns>
        private long GetCapacityFor(AProducibleSO producible)
        {
            if (collectedResource.Resource != null &&
                producible.ID == collectedResource.Resource.ID)
            {
                return data.Capacity - collectedResource.Quantity;
            }

            return data.Capacity;
        }

        #endregion

        #region Task Management

        /// <summary>
        /// Initiates a collection task for a specified resource node.
        /// </summary>
        /// <param name="node">The resource node to collect from.</param>
        /// <returns>True if the task was successfully scheduled, otherwise false.</returns>
        public bool GoCollectResource(IResourceNode node)
        {
            return RunTask(new UnitInteractionContext(node), collectingTask);
        }

        /// <summary>
        /// Initiates a deposit task for a specified resource depot.
        /// </summary>
        /// <param name="node">The resource depot to deposit to.</param>
        /// <returns>True if the task was successfully scheduled, otherwise false.</returns>
        public bool GoDepositResource(IResourceDepot depot)
        {
            return RunTask(new UnitInteractionContext(depot), depositResourceTask);
        }

        #endregion

        #region ITaskProvider

        bool ITaskProvider.CanProvideTaskForContext(ITaskContext context, out ITask taskToRun)
        {
            // In attack situations, target may be entity or position
            ResourceCollectorInput input = new(this);
            return TaskHelper.CanRunTask(context, input, collectionTasks, out taskToRun);
        }

        bool ITaskProvider.ScheduleTask(ITaskContext context, ITask task)
        {
            return RunTask(context, task);
        }

        /// <summary>
        /// Executes a specified task with a given context.
        /// </summary>
        /// <param name="context">The task context.</param>
        /// <param name="task">The task to execute.</param>
        /// <returns>
        /// Returns true if the task was executed successfully, otherwise false.
        /// </returns>
        private bool RunTask(ITaskContext context, ITask task)
        {
            ResourceCollectorInput input = new(this);
            if (!task.CanExecuteTask(context, input)) return false;

            Unit.ScheduleTask(context, input, task);
            return true;
        }

        #endregion

    }

}