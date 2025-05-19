using UnityEngine;

namespace TRavljen.UnitSystem.Task
{
    /// <summary>
    /// A task context for tasks that require a target position for execution,
    /// such as movement or positioning tasks.
    /// </summary>
    public class PositionContext : ITaskContext
    {
        /// <summary>
        /// Gets the target position for the task.
        /// </summary>
        public Vector3 TargetPosition { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="PositionContext"/> class
        /// with the specified target position.
        /// </summary>
        /// <param name="position">The target position for the task.</param>
        public PositionContext(Vector3 position)
        {
            TargetPosition = position;
        }
    }

}