namespace TRavljen.UnitSystem.Task
{
    /// <summary>
    /// Handles the execution lifecycle of a task, including starting,
    /// updating, and completing the task.
    /// </summary>
    public interface ITaskHandler
    {
        /// <summary>
        /// Starts the task using the provided context and input data.
        /// </summary>
        /// <param name="context">The context in which the task will run.</param>
        /// <param name="input">The input data required for task execution.</param>
        void StartTask(ITaskContext context, ITaskInput input);

        /// <summary>
        /// Updates the task logic, called each frame with the elapsed time.
        /// </summary>
        /// <param name="deltaTime">The time elapsed since the last update.</param>
        void UpdateTask(float deltaTime);

        /// <summary>
        /// Checks whether the task has finished execution.
        /// </summary>
        /// <returns>True if the task is finished, otherwise false.</returns>
        bool IsFinished();

        /// <summary>
        /// Ends the task, performing any necessary cleanup or finalization.
        /// </summary>
        void EndTask();
    }
}