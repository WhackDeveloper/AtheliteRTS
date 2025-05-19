using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TRavljen.UnitSystem.Collection
{
    using Interactions;
    using Task;

    /// <summary>
    /// Represents a task for a resource collector to gather resources from a specified resource node.
    /// </summary>
    public class CollectNodeResourceTask : ITask
    {

        /// <inheritdoc/>
        public bool CanExecuteTask(ITaskContext context, ITaskInput input)
        {
            if (context is not UnitInteractionContext { Target: IResourceNode node } ||
                input is not ResourceCollectorInput taskInput ||
                taskInput.collector.IsNull())
                return false;

            if (node.IsNotNull() &&
                node.Entity.Health is { IsDepleted: false })
                return false;

            // Check if interaction with this node is valid
            if (!node.IsActive || node.IsDepleted) return false;

            if (node is ILimitedUnitInteractionTarget limits && limits.HasReachedLimit())
                return false;

            // Check if there are no supported resources, or if node's resource
            // is contained within the supported resource list.
            return taskInput.collector.CanCollect(node.ResourceAmount.Resource);
        }

        /// <inheritdoc/>
        public ITaskHandler CreateHandler()
            => new CollectNodeResourceTaskHandler();
    }

    /// <summary>
    /// Handles the logic for executing a resource collection task, including movement to the node
    /// and collecting resources based on the collector's configuration.
    /// </summary>
    public class CollectNodeResourceTaskHandler : ITaskHandler
    {
        private bool isTaskFinished;
        private IResourceNode node;
        private ResourceCollector collector;
        private IUnitMovement move;

        private float collectProgression;
        private bool isCollecting;

        #region ITaskHandler

        /// <inheritdoc/>
        public bool IsFinished() => isTaskFinished;

        /// <inheritdoc/>
        public void StartTask(ITaskContext context, ITaskInput input)
        {
            node = (context as UnitInteractionContext).Target as IResourceNode;
            collector = ((ResourceCollectorInput)input).collector;
            move = collector.Unit.Movement;

            GoCollect(node);
        }

        /// <inheritdoc/>
        public void UpdateTask(float deltaTime)
        {
            if (node.IsNull() || !node.IsActive || node.IsDepleted)
            {
                if (collector.CollectType == CollectType.GatherAndDeposit &&
                    collector.IsCapacityFull == false)
                {
                    // Needs new target, finish task.
                    isTaskFinished = true;
                }
                else
                {
                    // Stop task, needs to deposit.
                    move.StopInCurrentPosition();
                    isTaskFinished = true;
                }
                
                if (isCollecting)
                    collector.OnFinishedCollecting.Invoke(node.IsNull() ? null : node);

                return;
            }

            if (isCollecting)
            {
                UpdateCollecting(deltaTime);
            }
            else if (move.HasReachedDestination())
            {
                isCollecting = true;
                collector.OnStartedCollecting.Invoke(node);
            }
        }

        /// <inheritdoc/>
        public void EndTask()
        {
            if (node != null)
            {
                // Remove assigned component used for limitations
                collector.Unit.InteractionMovement.RemoveInteractionTarget(collector, node);

                if (node is ILimitedUnitInteractionTarget limits)
                    limits.Unassign(collector.Unit);
            }
        }

        #endregion

        /// <summary>
        /// Handles the resource collection process, updating the collector's resource state
        /// and managing the task's completion based on the collector's type and capacity.
        /// </summary>
        private void UpdateCollecting(float deltaTime)
        {
            // Collect in intervals. Cannot do it per frame since we are not dealing with
            // floating numbers on resources. To collect, value must be 1 or higher.
            collectProgression += deltaTime;

            if (collectProgression >= collector.CollectInterval)
            {
                collector.CollectResource(node);

                collectProgression = 0;

                switch (collector.CollectType)
                {
                    case CollectType.GatherAndDeposit:
                        if (collector.IsCapacityFull)
                            // Finish collection, collector full
                            isTaskFinished = true;
                        break;

                    case CollectType.RealtimeCollect:
                        collector.DepositResource();

                        // Clear what is left, should not be stacking up in this case.
                        var collected = collector.CollectedResource;
                        collected.Quantity = 0;
                        collector.CollectedResource = collected;
                        break;

                    case CollectType.StackAndCollect:
                        if (collector.IsCapacityFull)
                            collector.DepositResource();
                        break;
                }
            }
        }

        /// <summary>
        /// Determines the appropriate action based on the collector's state and the target resource node.
        /// </summary>
        /// <param name="node">The resource node to collect from.</param>
        private void GoCollect(IResourceNode node)
        {
            if (collector.CollectedResource.Quantity == 0)
            {
                MoveToNode();
                return;
            }

            if (collector.CollectedResource.Resource != null &&
                node.ResourceAmount.Resource.ID != collector.CollectedResource.Resource.ID)
            {
                collector.ClearResource();

                MoveToNode();
            }
            else if (collector.IsCapacityFull)
            {
                isTaskFinished = true;
            }
            else
            {
                // Same resource and not full, continue collecting.
                MoveToNode();
            }
        }

        /// <summary>
        /// Initiates movement toward the resource node, handling assignment for limited targets.
        /// </summary>
        private void MoveToNode()
        {
            // Assign first if its limiting target
            if (node is ILimitedUnitInteractionTarget limits && !limits.Assign(collector.Unit))
            {
                Debug.Log("Attempting to assign to a full node:" + node + ". Task will be stopped");
                isTaskFinished = true;
                return;
            }

            collector.Unit.InteractionMovement.SetInteractionTarget(collector, node);
        }
    }

}