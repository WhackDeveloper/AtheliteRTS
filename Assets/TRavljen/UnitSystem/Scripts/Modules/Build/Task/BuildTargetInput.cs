using TRavljen.UnitSystem.Task;

namespace TRavljen.UnitSystem.Build
{

    /// <summary>
    /// Represents input data required for executing a build task.
    /// </summary>
    public struct BuildTargetInput : ITaskInput
    {
        /// <summary>
        /// The build component providing data and behavior for the task.
        /// </summary>
        public Builder builder;

        /// <summary>
        /// Creates a new instance with the specified builder component.
        /// </summary>
        /// <param name="builder">The Builder component initiating the task.</param>
        public BuildTargetInput(Builder builder)
        {
            this.builder = builder;
        }
    }

}