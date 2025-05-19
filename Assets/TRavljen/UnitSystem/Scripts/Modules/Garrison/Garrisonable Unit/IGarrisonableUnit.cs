namespace TRavljen.UnitSystem.Garrison
{
    using Interactions;

    /// <summary>
    /// Defines behavior for units that can enter and exit garrisons.
    /// </summary>
    public interface IGarrisonableUnit : IUnitInteractorComponent
    {
        /// <summary>
        /// Indicates whether the unit is currently inside a garrison.
        /// </summary>
        bool IsGarrisoned { get; }

        /// <summary>
        /// Moves the unit into a garrison. This method is invoked when the unit successfully enters a garrison.
        /// </summary>
        void EnterGarrison();

        /// <summary>
        /// Moves the unit out of a garrison. This method is invoked when the unit leaves a garrison.
        /// </summary>
        void ExitGarrison();

        /// <summary>
        /// Attempts to send the unit to enter the specified garrison.
        /// </summary>
        /// <param name="garrison">The garrison to enter.</param>
        /// <returns>
        /// Returns true if the task to enter the garrison was successfully scheduled; otherwise, false.
        /// </returns>
        bool GoEnterGarrison(IGarrisonEntity garrison);
    }

}