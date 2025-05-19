using UnityEngine;

namespace TRavljen.UnitSystem.Collection
{
    using Task;
    using Interactions;
    using TRavljen.UnitSystem;

    /// <summary>
    /// Represents a task for managing the collection and depositing of resources
    /// by a resource collector. This task handles the logic for switching between
    /// collecting resources from nodes and depositing them at depots.
    /// </summary>
    public class ResourceCollectingTask : ITask
    {

        [SerializeField]
        protected ITask collectNodeTask = new CollectNodeResourceTask();

        [SerializeField]
        protected ITask depositTask = new DepositResourceTask();

        /// <inheritdoc/>
        public bool CanExecuteTask(ITaskContext context, ITaskInput input)
        {
            if (context is not UnitInteractionContext interactionContext ||
                interactionContext.Target is not IResourceNode node ||
                input is not ResourceCollectorInput taskInput ||
                taskInput.collector == null)
                return false;

            if (node is IEntityComponent component)
            {
                bool isAlly = EntityRelationshipHelper.IsAlly(component.Entity, taskInput.collector.Entity);

                // Check for friendly and if entity's HEALTH is depleted
                if (isAlly == false &&
                    component != null &&
                    component.Entity.Health != null &&
                    !component.Entity.Health.IsDepleted)
                    return false;
            }

            // Check if interaction with this node is valid
            if (!node.IsActive || node.IsDepleted) return false;

            // Check if there are no supported resources, or if node's resource
            // is contained within the supported resource list.
            return taskInput.collector.CanCollect(node.ResourceAmount.Resource);
        }

        /// <inheritdoc/>
        public ITaskHandler CreateHandler()
        {
            return new ResourceCollectingTaskHandler(collectNodeTask, depositTask);
        }

    }

    /// <summary>
    /// Manages the execution of a resource collecting task, including transitions
    /// between collecting from resource nodes and depositing resources at depots.
    /// </summary>
    public class ResourceCollectingTaskHandler : ITaskHandler
    {

        private enum State { Idle, Collecting, Depositing, Finished }

        #region Properties

        private IResourceNode targetNode;
        private IResourceDepot targetDepot;
        private ResourceCollector collector;

        private readonly ITask depositTask;
        private readonly ITask collectTask;
        private ITaskHandler activeTask = null;
        private Vector3 lastNodePosition;

        private CollectionModule module;
        private ResourceSO collectingResource;

        private State state = State.Idle;

        #endregion

        #region Getters

        public bool IsDepositing => state == State.Depositing;
        public bool IsCollecting => state == State.Collecting;
        public IResourceDepot TargetDepot => targetDepot;
        public IResourceNode TargetNode => targetNode;

        #endregion

        #region ITaskHandler

        public bool IsFinished() => state == State.Finished;

        /// <summary>
        /// Initializes a new instance of the <see cref="ResourceCollectingTaskHandler"/>.
        /// </summary>
        /// <param name="collectTask">The task for collecting resources from a node.</param>
        /// <param name="depositTask">The task for depositing resources at a depot.</param>
        public ResourceCollectingTaskHandler(ITask collectTask, ITask depositTask)
        {
            this.collectTask = collectTask;
            this.depositTask = depositTask;
        }

        /// <inheritdoc/>
        public void StartTask(ITaskContext context, ITaskInput input)
        {
            collector = ((ResourceCollectorInput)input).collector;
            module = collector.Unit.Owner.GetModule<CollectionModule>();
            targetNode = (context as UnitInteractionContext).Target as IResourceNode;
            collectingResource = targetNode.ResourceAmount.Resource;

            // Initially we start with resource node, never depot
            StartCollectionTask(targetNode);
        }

        /// <inheritdoc/>
        public void UpdateTask(float deltaTime)
        {
            activeTask.UpdateTask(deltaTime);

            if (!activeTask.IsFinished()) return;

            activeTask.EndTask();

            switch (state)
            {
                case State.Collecting:
                    UpdateCollectingState();

                    break;

                case State.Depositing:
                    UpdateDepositingState();
                    break;
            }
        }

        /// <inheritdoc/>
        public void EndTask()
        {
            activeTask?.EndTask();
        }

        #endregion

        private void UpdateCollectingState()
        {
            // If collector capacity is not full, find another node
            Vector3 searchPos = targetNode.IsNull() ? collector.Position : targetNode.Position;

            if (collector.IsCapacityFull == false &&
                module.FindNearbyNode(searchPos, collectingResource, out IResourceNode node))
            {
                lastNodePosition = node.transform.position;
                targetNode = node;
                StartInternalTask(collectTask, node);
            }
            else
            {
                // Collector is full or no nearby node was found

                // Check if collector is not empty, then find depot
                if (collector.CollectedResource.Quantity > 0)
                {
                    if (module.FindNearestDepot(collector.Position, collectingResource, out IResourceDepot depot))
                    {
                        targetDepot = depot;
                        StartInternalTask(depositTask, depot);
                        state = State.Depositing;
                    }
                    else
                    {
                        // Collector here has resources, either full or not & no node to collect from nearby.
                        CollectionEvents.Instance.OnMissingDepot.Invoke(collector);
                    }
                }
                else
                {
                    // Collector is empty and no nearby node was found, collector will now be idle
                    state = State.Finished;
                }
            }
        }

        private void UpdateDepositingState()
        {
            // If resources are still present, unit failed to drop in depot.
            if (collector.CollectedResource.Quantity > 0 ||
                targetDepot == null ||
                !targetDepot.IsActive)
            {
                // Failed to deposit, try to find another depot
                if (module.FindNearestDepot(collector.Position, collectingResource, out IResourceDepot depot) &&
                    targetDepot != depot)
                {
                    StartInternalTask(depositTask, depot);
                }
                else
                {
                    targetDepot = null;
                    // Invoke event for missing depot for the resource collected.
                    // Cannot continue collecting before dopositing.
                    CollectionEvents.Instance.OnMissingDepot.Invoke(collector);

                    // Collector is now idle.
                }
                return;
            }

            // Resources were deposited, return to previous node if valid
            if (targetNode.IsNotNull() &&
                targetNode.IsActive &&
                !targetNode.IsDepleted &&
                // Either its not limiting or it's limit hasn't been reached
                (targetNode is not ILimitedUnitInteractionTarget limits || !limits.HasReachedLimit()))
            {
                // Could call StartCollectionTask but don't need to update position or node.
                StartInternalTask(collectTask, targetNode);
                state = State.Collecting;
            }
            // Find new node near previous one
            else if (module.FindNearbyNode(lastNodePosition, collectingResource, out var node))
            {
                StartCollectionTask(node);
            }
            // Otherwise try again near collector itself
            else if (module.FindNearbyNode(collector.Position, collectingResource, out node))
            {
                StartCollectionTask(node);
            }
            else
            {
                // No nearby nodes after collecting
                state = State.Finished;
            }
        }

        private void StartInternalTask(ITask task, IUnitInteracteeComponent target)
        {
            activeTask = task.CreateHandler();
            activeTask.StartTask(new UnitInteractionContext(target), new ResourceCollectorInput(collector));
        }

        private void StartCollectionTask(IResourceNode node)
        {
            lastNodePosition = node.transform.position;
            targetNode = node;
            state = State.Collecting;
            StartInternalTask(collectTask, node);
        }

    }
}