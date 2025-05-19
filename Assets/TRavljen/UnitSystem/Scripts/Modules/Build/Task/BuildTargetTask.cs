using UnityEngine;

namespace TRavljen.UnitSystem.Build
{
    using Interactions;
    using Task;

    /// <summary>
    /// Represents a task for building a specific target unit.
    /// This task ensures the builder reaches the target and performs the build process.
    /// </summary>
    public class BuildTargetTask: ITask
    {

        /// <inheritdoc/>
        public bool CanExecuteTask(ITaskContext context, ITaskInput input)
        {
            if (input is not BuildTargetInput buildTarget ||
                buildTarget.builder == null ||
                context is not UnitInteractionContext interactionContext)
                return false;

            return interactionContext.Target is EntityBuilding unitBuilding &&
                unitBuilding.IsActive &&
                !unitBuilding.BuildAutomatically &&
                !unitBuilding.HasReachedLimit();
        }

        /// <inheritdoc/>
        public ITaskHandler CreateHandler()
        {
            return new BuildTargetTaskHandler();
        }
    }

}