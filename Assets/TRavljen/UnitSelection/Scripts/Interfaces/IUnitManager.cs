using System.Collections.Generic;
using UnityEngine;

namespace TRavljen.UnitSelection
{

    /// <summary>
    /// Interface for providing managed units for selection. These can
    /// be any units that have a component that implements <see cref="ISelectable"/>
    /// attached to them.
    /// </summary>
    public interface IUnitManager
    {

        /// <summary>
        /// List of selectable units.
        /// When units are removed from this list, make sure to notify selection
        /// system with <see cref="ActiveSelections.CleanUpAfterUnit(ISelectable)"/>
        /// to perform any necessary cleanups.
        /// </summary>
        public List<ISelectable> SelectableUnits { get; }

    }

}
