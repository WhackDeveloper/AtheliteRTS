using System.Collections.Generic;

namespace TRavljen.UnitSystem.Garrison
{

    using Interactions;

    /// <summary>
    /// Interface used for entities that have capability to
    /// garrison units. 
    /// </summary>
    public interface IGarrisonEntity: IUnitInteracteeComponent
    {

        /// <summary>
        /// Max capacity that this unit can garrison.
        /// </summary>
        int GarrisonCapacity { get; }

        /// <summary>
        /// Units currently garrisoned within the unit.
        /// </summary>
        List<IGarrisonableUnit> GarrisonedUnits { get; }

        /// <summary>
        /// Checks if unit is eligible for entering this specific garrison.
        /// </summary>
        /// <param name="unit">Unit to check</param>
        /// <returns>Returns `true` if unit is eligible.</returns>
        bool IsEligibleToEnter(IGarrisonableUnit unit);

        /// <summary>
        /// Checks if unit is close enough to entrance to enter the garrison.
        /// </summary>
        /// <param name="unit">Unit to check</param>
        /// <returns>Returns `true` if unit is in range.</returns>
        bool IsInRangeToEnter(IGarrisonableUnit unit);

        /// <summary>
        /// Removes all garrisoned units from the garrison.
        /// </summary>
        void RemoveAllUnits();

        /// <summary>
        /// Adds a new unit to the garrison.
        /// </summary>
        /// <param name="unit">Unit to add to garrison.</param>
        /// <returns>Returns `true` if unit was added.</returns>
        bool AddUnit(IGarrisonableUnit unit);

        /// <summary>
        /// Removes a new from the garrison.
        /// </summary>
        /// <param name="unit">Unit to remove from garrison.</param>
        /// <returns>Returns `true` if unit was removed.</returns>
        bool RemoveUnit(IGarrisonableUnit unit);

    }

}
