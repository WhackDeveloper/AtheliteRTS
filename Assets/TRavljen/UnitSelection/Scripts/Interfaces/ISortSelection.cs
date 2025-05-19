using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TRavljen.UnitSelection
{

    /// <summary>
    /// Implement this interface to sort selection with custom criteria.
    /// This can mostly be useful if the selection system has a limit for max
    /// active selections, which then takes only first selection objects, leaving
    /// out the rest that are above the max limit.
    /// </summary>
    public interface ISortSelection
    {
        /// <summary>
        /// Sort the list to any desired criteria, after they have been filtered.
        /// </summary>
        /// <param name="selectionObjects">Units to be selected or highlighted</param>
        public void Sort(List<ISelectable> selectionObjects);
    }

}