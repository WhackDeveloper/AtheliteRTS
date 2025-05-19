namespace TRavljen.UnitSystem.Garrison
{

    using Task;

    /// <summary>
    /// Represents the input required for executing the <see cref="EnterGarrisonTask"/>.
    /// </summary>
    public struct EnterGarrisonInput : ITaskInput
    {
        /// <summary>
        /// The unit that is attempting to hide in a garrison.
        /// </summary>
        public IGarrisonableUnit garrisonableUnit;

        /// <summary>
        /// Initializes a new instance of the <see cref="EnterGarrisonInput"/> struct.
        /// </summary>
        /// <param name="garrisonableUnit">The garrisonable unit providing task input.</param>
        public EnterGarrisonInput(IGarrisonableUnit garrisonableUnit)
        {
            this.garrisonableUnit = garrisonableUnit;
        }
    }

}
