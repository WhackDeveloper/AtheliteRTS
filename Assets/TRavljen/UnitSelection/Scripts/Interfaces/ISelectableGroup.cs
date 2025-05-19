using System.Collections.Generic;
using UnityEngine;

namespace TRavljen.UnitSelection
{

    /// <summary>
    /// Interfaces used for communication between a group and <see cref="UnitSelector"/>
    /// and its used to manage selection of grouped units.
    /// </summary>
    public interface ISelectableGroup : ISelectable
    {

        /// <summary>
        /// Specifies the selectable units that belong to this group.
        /// </summary>
        [Tooltip("Specifies the selectable units that belong to this group.")]
        public List<ISelectableGroupUnit> GroupUnits { get; }

        /// <summary>
        /// Add a new unit to the group.
        /// </summary>
        /// <param name="groupUnit">New group unit to add.</param>
        public void AddGroupUnit(ISelectableGroupUnit groupUnit);
     
        /// <summary>
        /// Remove a unit from the group.
        /// </summary>
        /// <param name="groupUnit">Group unit to remove.</param>
        public void RemoveGroupUnit(ISelectableGroupUnit groupUnit);
    }

}