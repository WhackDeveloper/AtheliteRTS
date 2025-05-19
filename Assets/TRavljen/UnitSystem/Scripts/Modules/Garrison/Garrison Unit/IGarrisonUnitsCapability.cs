namespace TRavljen.UnitSystem.Garrison
{

    /// <summary>
    /// Defines the capability for a unit to act as a garrison,
    /// allowing other units to enter and be stored within it.
    /// </summary>
    public interface IGarrisonUnitsCapability : IEntityCapability
    {
        /// <summary>
        /// Specifies the maximum number of units that can enter the garrison.
        /// </summary>
        int Capacity { get; }

        /// <summary>
        /// Determines whether a specific unit is eligible to enter the garrison.
        /// This can be used to restrict or permit certain units or unit types.
        /// </summary>
        /// <param name="unit">The unit to check for eligibility.</param>
        /// <returns>Returns true if the unit is eligible to enter, false otherwise.</returns>
        bool IsEligibleToEnter(AUnitSO unit);
    }


}