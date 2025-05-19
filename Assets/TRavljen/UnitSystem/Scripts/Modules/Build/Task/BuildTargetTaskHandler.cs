using UnityEngine;
using UnityEngine.Events;

namespace TRavljen.UnitSystem.Build
{
    using Interactions;
    using Task;

    /// <summary>
    /// Handles the execution of a build task, moving the builder to the target unit and performing the build process.
    /// </summary>
    public class BuildTargetTaskHandler : ITaskHandler
    {

        #region Properties

        private Builder builder;
        private EntityBuilding currentTarget;
        private IEntity buildingEntity;
        private IUnitMovement movement;

        private float buildTimer = 0;
        private bool isBuilding = false;
        private bool isTaskFinished = false;

        #endregion

        #region Getters

        /// <summary>
        /// The current target being built.
        /// </summary>
        public EntityBuilding CurrentTarget => currentTarget;

        /// <summary>
        /// Indicates whether the builder is actively building the target.
        /// </summary>
        public bool IsBuilding => isBuilding;

        #endregion

        #region ITaskHandler

        /// <inheritdoc/>
        public bool IsFinished() => isTaskFinished;

        /// <inheritdoc/>
        public void StartTask(ITaskContext context, ITaskInput input)
        {
            var target = (context as UnitInteractionContext).Target;
            builder = ((BuildTargetInput)input).builder;
            movement = builder.Unit.Movement;
            currentTarget = target as EntityBuilding;
            buildingEntity = currentTarget?.Entity;

            if (currentTarget.IsNull() || buildingEntity.IsNull() || buildingEntity.transform.IsNull())
            {
                isTaskFinished = true;
            }
            else
            {
                MoveToBuildingUnit();
            }
        }

        /// <inheritdoc/>
        public void UpdateTask(float deltaTime)
        {
            // Check if target missing, disabled or finished building
            if (currentTarget != null &&
                currentTarget.IsActive &&
                !currentTarget.IsBuilt)
            {
                UpdateWithTarget(deltaTime);
            }
            else
            {
                isTaskFinished = true;

                // Invoke this event only if builder started building.
                if (isBuilding)
                    builder.OnFinishBuilding.Invoke(buildingEntity.IsNull() ? null : buildingEntity);

                BuildEvents.Instance.OnBuilderFinished.Invoke(builder, buildingEntity);
            }
        }

        /// <inheritdoc/>
        public void EndTask()
        {
            // Stop unit from moving if it did not start the work yet.
            if (!isBuilding)
                movement.StopInCurrentPosition();

            if (currentTarget)
            {
                builder.Unit.InteractionMovement.RemoveInteractionTarget(builder, currentTarget);

                if (currentTarget is ILimitedUnitInteractionTarget limits)
                    limits.Unassign(builder.Unit);
            }

            currentTarget = null;
        }

        #endregion

        #region Convenience

        /// <summary>
        /// Moves the builder to the target unit's position to begin the build process.
        /// </summary>
        private void MoveToBuildingUnit()
        {
            // Check if target has limit and if it went over the limit.
            if (currentTarget is not ILimitedUnitInteractionTarget limits ||
                limits.Assign(builder.Unit))
            {
                builder.Unit.InteractionMovement.SetInteractionTarget(builder, currentTarget);
            }
            else
            {
                Debug.Log("Assigning worker failed. This task will stop.");
                isTaskFinished = true;
                BuildEvents.Instance.OnBuilderFinished.Invoke(builder, currentTarget.Entity as Unit);
            }
        }

        /// <summary>
        /// Updates the build process when the builder is at the target position.
        /// </summary>
        private void UpdateWithTarget(float deltaTime)
        {
            if (isBuilding)
            {
                if (builder.BuildWithIntervals)
                {
                    buildTimer += deltaTime;

                    if (buildTimer < builder.BuildInterval) return;

                    // Do this in case interval passed more than once.
                    var remainder = buildTimer - builder.BuildInterval;
                    var count = (int)(buildTimer / builder.BuildInterval);

                    // Store remainder for next iteration.
                    buildTimer = remainder;

                    // Apply count of the intervals to build power.
                    currentTarget.BuildWithPower(builder.BuildPower * builder.BuildInterval * count);
                }
                else
                {
                    currentTarget.BuildWithPower(builder.BuildPower * deltaTime);
                }
            }
            else if (movement.HasReachedDestination() && movement.IsFacingTargetDirection())
            {
                isBuilding = true;
                builder.OnStartBuilding.Invoke(buildingEntity);
            }
        }

        #endregion
    }
}