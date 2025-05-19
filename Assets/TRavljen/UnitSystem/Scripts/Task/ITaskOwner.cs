namespace TRavljen.UnitSystem.Task
{

    /// <summary>
    /// Defines a contract for components that manage ownership of a single active task.
    /// This interface is typically implemented by components attached to game objects
    /// to track and manage tasks assigned to them.
    /// <see cref="TaskSystem"/> will update component that implements this interface
    /// with the currently active task handler for its game object.
    /// </summary>
    public interface ITaskOwner
    {
        /// <summary>
        /// Gets or sets the currently active task handler for the implementing game object.
        /// </summary>
        /// <remarks>
        /// This property is managed by the <see cref="TaskSystem"/>, which ensures that only one task
        /// can be active per game object at any given time. When the current task completes,
        /// the next task in the queue (if any) will be assigned as the active task.
        /// <para>
        /// Implementing this interface is optional but provides a convenient way to synchronize
        /// task states with the components of a game object.
        /// </para>
        /// </remarks>
        ITaskHandler ActiveTask { get; set; }
    }

}