namespace TRavljen.UnitSystem.Task
{

    /// <summary>
    /// Interface for validating and executing tasks within a specific context.
    /// </summary>
    public interface ITaskProvider
    {
        /// <summary>
        /// Determines whether the component has a task that can be run in the
        /// given context and outputs the task to run.
        /// </summary>
        /// <param name="context">The task context for evaluation.</param>
        /// <param name="taskToRun">The provided task, if applicable.</param>
        /// <returns>True if a task can be provided, otherwise false.</returns>
        bool CanProvideTaskForContext(ITaskContext context, out ITask taskToRun);

        /// <summary>
        /// Schedules the specified task within the given context.
        /// </summary>
        /// <param name="context">The task context for execution.</param>
        /// <param name="task">The task to execute.</param>
        /// <returns>True if the task was successfully executed, otherwise false.</returns>
        bool ScheduleTask(ITaskContext context, ITask task);
    }

}