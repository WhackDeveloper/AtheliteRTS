using TRavljen.UnitSystem.Task;

namespace TRavljen.UnitSystem.Collection
{

    /// <summary>
    /// Represents input data required for a resource collection task.
    /// </summary>
    /// <remarks>
    /// This input structure provides the resource collector instance necessary
    /// for executing tasks related to resource gathering and depositing.
    /// </remarks>
    public struct ResourceCollectorInput : ITaskInput
    {
        /// <summary>
        /// The resource collector executing the task.
        /// </summary>
        public ResourceCollector collector;

        /// <summary>
        /// Initializes a new instance of <see cref="ResourceCollectorInput"/> with the specified collector.
        /// </summary>
        /// <param name="collector">The resource collector executing the task.</param>
        public ResourceCollectorInput(ResourceCollector collector)
        {
            this.collector = collector;
        }
    }

}