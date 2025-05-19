using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TRavljen.UnitSystem.Collection
{
    using Task;

    /// <summary>
    /// Represents a task for a resource collector to deposit resources into a valid resource depot.
    /// </summary>
    public class DepositResourceTask : ITask
    {

        [Tooltip("Interval at which the target position is updated while depositing resources.\n" +
            "This is useful for handling moving depots, ensuring the collector continually adjusts its destination.")]
        [SerializeField, Range(0.01f, 2f)]
        private float targetPositionUpdateInterval = 0.5f;

        /// <inheritdoc/>
        public bool CanExecuteTask(ITaskContext context, ITaskInput input)
        {
            // Validate context & input
            if (context is not UnitInteractionContext interactionContext ||
               interactionContext.Target is not IResourceDepot depot ||
               !depot.IsActive ||
               input is not ResourceCollectorInput taskInput ||
               taskInput.collector == null)
                return false;

            // Collector has no resource to deposit
            if (taskInput.collector.CollectedResource.Resource == null) return false;

            return depot.CanDepositResource(taskInput.collector.CollectedResource.Resource);
        }

        /// <inheritdoc/>
        public ITaskHandler CreateHandler()
            => new DepositResourceTaskHandler(targetPositionUpdateInterval);

    }

    /// <summary>
    /// Handles the logic for executing a resource deposit task, including movement to the depot
    /// and the deposit operation itself.
    /// </summary>
    public class DepositResourceTaskHandler : ITaskHandler
    {

        private ResourceCollector collector;
        private IResourceDepot depot;
        private IUnitMovement move;
        private IUnitMovement depotMove;

        private readonly float targetPositionUpdateInterval;
        private float delayAfterDepositTimer = 0;
        private float positionUpdateTimer = 0;

        private bool followsTarget = false;
        private bool isResourceDeposited = false;
        private bool isTaskFinished = false;

        /// <summary>
        /// Initializes a new instance of the <see cref="DepositResourceTaskHandler"/> class.
        /// </summary>
        /// <param name="targetPositionUpdateInterval">
        /// Interval at which the target position is updated while depositing resources.
        /// Target position is updated only if target supports movement.
        /// </param>
        public DepositResourceTaskHandler(float targetPositionUpdateInterval)
        {
            this.targetPositionUpdateInterval = targetPositionUpdateInterval;
        }

        #region ITaskHandler

        /// <inheritdoc/>
        public bool IsFinished() => isTaskFinished;

        /// <inheritdoc/>
        public void StartTask(ITaskContext context, ITaskInput input)
        {
            depot = (context as UnitInteractionContext).Target as IResourceDepot;
            collector = ((ResourceCollectorInput)input).collector;
            move = collector.Unit.Movement;
            bool canDepotMove = collector.Unit.TryGetComponent(out depotMove);
            followsTarget = canDepotMove && collector.FollowTarget;

            MoveToDepot();
        }

        /// <inheritdoc/>
        public void EndTask()
        {
            collector.Unit.InteractionMovement.RemoveInteractionTarget(collector, depot);
        }

        /// <inheritdoc/>
        public void UpdateTask(float deltaTime)
        {
            if (isResourceDeposited)
            {
                delayAfterDepositTimer += deltaTime;
                if (delayAfterDepositTimer >= collector.DelayAfterDeposit)
                {
                    isTaskFinished = true;
                }
            }

            // Check if depot is still valid
            if (depot == null || depot.IsActive == false)
            {
                move.StopInCurrentPosition();
                isTaskFinished = true;
                return;
            }

            if (move.HasReachedDestination())
            {
                DepositResources();
            }
            else if (followsTarget)
            {
                positionUpdateTimer += deltaTime;

                if (!depotMove.HasReachedDestination() &&
                    positionUpdateTimer > targetPositionUpdateInterval)
                {
                    positionUpdateTimer = 0;
                    MoveToDepot();
                }
            }
        }

        #endregion

        /// <summary>
        /// Initiates movement of the resource collector toward the resource depot.
        /// </summary>
        private void MoveToDepot()
        {
            collector.Unit.InteractionMovement.SetInteractionTarget(collector, depot);
        }

        /// <summary>
        /// Deposits resources into the depot, updating the collector's resource state.
        /// </summary>
        private void DepositResources()
        {
            var collectedResource = collector.CollectedResource;

            long remainder = depot.DepositResources(collectedResource);
            long deposited = collectedResource.Quantity - remainder;

            // Update if anything was deposited
            if (deposited > 0)
            {
                collectedResource.Quantity = remainder;
                collector.OnResourceDeposited.Invoke(new(collectedResource.Resource, deposited));
                collector.CollectedResource = collectedResource;
            }

            if (remainder == 0)
            {
                isResourceDeposited = true;
            }
        }

    }
}