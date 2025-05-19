namespace TRavljen.UnitSystem.Task
{

    /// <summary>
    /// Represents the contract for a task, providing methods to validate execution
    /// and create handlers for task logic.
    /// </summary>
    public interface ITask
    {

        /// <summary>
        /// Determines whether the task can be executed in the specified context
        /// with the given input.
        /// </summary>
        /// <param name="context">The context in which the task will be executed.</param>
        /// <param name="input">The input data required for task execution.</param>
        /// <returns>True if the task can be executed, otherwise false.</returns>
        public abstract bool CanExecuteTask(ITaskContext context, ITaskInput input);

        /// <summary>
        /// Creates a handler to manage the task's execution process.
        /// </summary>
        /// <returns>An instance of <see cref="ITaskHandler"/> for task execution.</returns>
        public abstract ITaskHandler CreateHandler();
    }

}