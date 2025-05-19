using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TRavljen.UnitSelection
{

    /// <summary>
    /// Selection type used for filtering.
    /// </summary>
    public enum FilterSelectionType
    {
        /// <summary>
        /// When selection is added to current selection.
        /// </summary>
        AddSelection,
        /// <summary>
        /// When selection is replacing current selection.
        /// </summary>
        ReplaceSelection
    }

    /// <summary>
    /// Implement this interface to filter selection with custom criteria.
    /// Once initialised, set it to <see cref="ActiveSelections.Filter"/>.
    /// </summary>
    /// <example>
    /// When player uses drag to select, he could select stationary (like structures)
    /// or moving units (like workers). In such case you could for example prioritise
    /// moving units if the list contains stationary units by filtering removing them
    /// from the list.
    /// </example>
    public interface IFilterSelection
    {
        /// <summary>
        /// You may filters out and manipulate selections list to any custom criteria.
        /// </summary>
        /// <param name="selectables">Units to be selected or highlighted</param>
        /// <param name="type">Type of selection</param>
        public void Filter(List<ISelectable> selectables, FilterSelectionType type);
    }

}