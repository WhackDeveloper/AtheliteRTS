using System.Collections.Generic;
using UnityEngine;
using System;

namespace TRavljen.UnitSelection
{

    /// <summary>
    /// Component responsible for processing selection and highlights, keeping
    /// those references in <see cref="SelectedUnits"/> and <see cref="HighlightedUnits"/>
    /// lists and invoking actions: <see cref="OnUnitSelectionChange"/>,
    /// <see cref="OnUnitHoverChange"/> and <see cref="OnUnitHighlightChange"/>.
    /// </summary>
    public sealed class ActiveSelections: MonoBehaviour
    {

        #region Properties

        /// <summary>
        /// Specifies if the <see cref="MaxActiveSelections"/> is used or ignored.
        /// Setting this to false means unlimited active selections.
        /// </summary>
        [Tooltip("Specifies if selection is limited with " +
            "maximal active selections.")]
        public bool ApplyMaxActiveSelections = true;

        /// <summary>
        /// Specifies the maximum active selection allowed at once. This can be
        /// disabled by setting false to <see cref="ApplyMaxActiveSelections"/>.
        /// </summary>
        [Range(0, 2000), Tooltip("Maximal active selections count. This will be " +
            "ignored if apply max active selections flag is set to 'false'.")]
        public int MaxActiveSelections = 10;

        [Tooltip("Specifies if logic for grouped units should be executing.\n" +
            "Check out components SelectableGroupUnit and SelectableGroup for more details on how to use it.")]
        [SerializeField]
        private bool supportsGroups = false;

        /// <summary>
        /// Currently selected units.
        /// </summary>
        public List<ISelectable> SelectedUnits { private set; get; } = new List<ISelectable>();

        /// <summary>
        /// Currently highlighted units.
        /// </summary>
        public List<ISelectable> HighlightedUnits { private set; get; } = new List<ISelectable>();

        /// <summary>
        /// Specifies currently hovering unit and used for highlighting.
        /// Use <see cref="ISelectable"/> to apply behaviour on highlight.
        /// </summary>
        internal ISelectable HoveringOverUnit { private set; get; }

        /// <summary>
        /// Specifies currently single clicked unit used for double click behaviour.
        /// </summary>
        internal ISelectable ClickedGameObject;

        /// <summary>
        /// Invoked when any unit is selected or deselected and receives newly
        /// selected units as parameter.
        /// </summary>
        public Action<List<ISelectable>> OnUnitSelectionChange;

        /// <summary>
        /// Invoked when any unit is highlighted or unhighlighted and receives newly
        /// highlighted units as parameter.
        /// </summary>
        public Action<List<ISelectable>> OnUnitHighlightChange;

        /// <summary>
        /// Invoked when hovering state is changed. Either a unit is hovered
        /// or unit is no longer hovered.
        /// </summary>
        public Action<ISelectable> OnUnitHoverChange;

        /// <summary>
        /// Optional selection filters. By providing this you can prioritise
        /// certain units over others by removing some from the list.
        /// Filtering is performed before sorting (<see cref="Sorter"/>).
        /// </summary>
        public readonly List<IFilterSelection> Filters = new();

        /// <summary>
        /// Optional selection sorter. By providing this you can use custom
        /// sort for prioritising units when selected. This will impact the
        /// order in which units are selected, and also impact which units
        /// are ignored when limit is reached (<see cref="ApplyMaxActiveSelections"/>).
        /// If limit is not applied, then this impacts only sorting.
        /// </summary>
        ///
        public ISortSelection Sorter;

        #endregion

        #region Cleanup

        private void Awake()
        {
            UnitManagementEvents.OnUnitRemoved.AddListener(CleanUpAfterUnit);
        }

        private void OnDestroy()
        {
            UnitManagementEvents.OnUnitRemoved.RemoveListener(CleanUpAfterUnit);
        }

        private void OnEnable()
        {
            OnUnitSelectionChange += SelectionEvents.Instance.OnSelectionChange.Invoke;
            OnUnitHighlightChange += SelectionEvents.Instance.OnHighlightChange.Invoke;
            OnUnitHoverChange += SelectionEvents.Instance.OnUnitHoverChange.Invoke;
        }

        private void OnDisable()
        {
            OnUnitSelectionChange -= SelectionEvents.Instance.OnSelectionChange.Invoke;
            OnUnitHighlightChange -= SelectionEvents.Instance.OnHighlightChange.Invoke;
            OnUnitHoverChange -= SelectionEvents.Instance.OnUnitHoverChange.Invoke;
        }

        /// <summary>
        /// Removes and performs any cleanup needed for the unit. This should
        /// be used if any of the selectable units are destroyed or removed from
        /// selection to avoid iterating through a list of destroyed objects.
        /// Not using this and destroying a selected unit might cause exceptions.
        /// </summary>
        /// <param name="selectableUnit">selectable unit to be removed from selection</param>
        public void CleanUpAfterUnit(ISelectable selectableUnit)
        {
            if (supportsGroups && selectableUnit.TryGetUnitGroup(out ISelectableGroup group))
                selectableUnit = group;

            // Deselect
            if (SelectedUnits.Contains(selectableUnit))
            {
                Deselect(selectableUnit);
            }

            // Unhighlight
            if (HighlightedUnits.Remove(selectableUnit))
            {
                selectableUnit.Unhighlight();

                // Notify of the change, unit removed from highlight list
                OnUnitHighlightChange?.Invoke(HighlightedUnits);
            }

            // Remove as hovering object
            if (HoveringOverUnit == selectableUnit)
            {
                RemoveHoveringUnit();
            }

            // Remove as clicked object
            if (ClickedGameObject == selectableUnit)
            {
                // Object should be already deselected above
                ClickedGameObject = null;
            }
        }

        #endregion

        #region Selection

        /// <summary>
        /// Replaces currently selected objects with passed objects
        /// and calls <see cref="ISelectable.Select"/> on them if
        /// any object contains component <see cref="ISelectable"/>.
        /// </summary>
        /// <param name="selectables">Selectable units that will be set as
        /// the new selection.</param>
        /// <param name="processSelection">'True' by default. Specifies if gameObjects will be processed
        /// before selection by filtering, sorting and limiting to max active selections.
        /// </param>
        public void ReplaceSelection(List<ISelectable> selectables, bool processSelection = true)
        {
            Deselect(SelectedUnits, false);
            UnhighlightAll();

            if (processSelection)
                AddNewSelections(FilterObjectsForSelection(selectables, false));
            else
            {
                selectables = new List<ISelectable>(selectables);

                if (supportsGroups)
                    selectables.ReplaceGroupUnitsWithGroups();

                AddNewSelections(selectables);
            }
        }

        /// <summary>
        /// Replaces currently selected objects with passed object
        /// and calls <see cref="ISelectable.Select"/> on it if
        /// any object contains component <see cref="ISelectable"/>.
        /// </summary>
        /// <param name="selectable">Selectable unit that will be set as
        /// the new selected objects.</param>
        public void ReplaceSelection(ISelectable selectable)
        {
            ReplaceSelection(new List<ISelectable>() { selectable });
        }

        /// <summary>
        /// Adds passed selectables to the current list of selectables
        /// and calls <see cref="ISelectable.Select"/> on them if
        /// any object contains component <see cref="ISelectable"/>.
        /// </summary>
        /// <param name="selectables">Selectable units to add to current active
        /// selection.</param>
        public void AddSelection(List<ISelectable> selectables)
        {
            var filteredUnits = FilterObjectsForSelection(selectables, true);

            UnhighlightAll();
            AddNewSelections(filteredUnits);
        }

        /// <summary>
        /// Toggles the selection on the selectable passed. If unit is already
        /// selected it will be deselected, otherwise it will be selected and added
        /// to the list of active selections.
        /// </summary>
        /// <param name="selectable">Selectable unit to toggle.</param>
        internal void ToggleSingleSelection(ISelectable selectable)
        {
            if (supportsGroups && selectable.TryGetUnitGroup(out ISelectableGroup group))
                selectable = group;

            // If object is already selected, deselect it
            if (SelectedUnits.Contains(selectable))
            {
                Deselect(selectable);
            }
            // Otherwise select the object if MAX selection has not been reached
            else if (!ApplyMaxActiveSelections ||
                SelectedUnits.Count < MaxActiveSelections)
            {
                AddSelection(new List<ISelectable>() { selectable });
            }
        }

        /// <summary>
        /// Clears the selected unit list and invokes <see cref="ISelectable.Deselect"/>.
        /// on each of them. At the end <see cref="OnUnitSelectionChange"/> is also invoked.
        /// </summary>
        public void DeselectAll()
        {
            Deselect(SelectedUnits);
        }

        /// <summary>
        /// Deselects passed selectable units, removes them from current selection
        /// and calls <see cref="ISelectable.Deselect"/> on them if
        /// any object contains component <see cref="ISelectable"/>.
        /// </summary>
        /// <param name="selectables">List of selected units</param>
        public void Deselect(List<ISelectable> selectables)
        {
            Deselect(selectables, true);
        }

        /// <summary>
        /// Deselects passed selectable units, removes them from current selection
        /// and calls <see cref="ISelectable.Deselect"/> on them if
        /// any object contains component <see cref="ISelectable"/>.
        /// </summary>
        /// <param name="selectables">List of selected units</param>
        /// <param name="invokeUpdateEvent">If enabled invokes the event for change.</param>
        private void Deselect(List<ISelectable> selectables, bool invokeUpdateEvent)
        {
            if (selectables.Count > 0)
            {
                bool removed = false;
                for (int index = selectables.Count - 1; index >= 0; index--)
                {
                    var selectable = selectables[index];

                    if (supportsGroups && selectable.TryGetUnitGroup(out ISelectableGroup group))
                        selectable = group;

                    if (SelectedUnits.Remove(selectable))
                    {
                        removed = true;
                        selectable.Deselect();
                    }
                }

                if (invokeUpdateEvent && removed)
                    OnUnitSelectionChange?.Invoke(SelectedUnits);
            }
        }

        public void Deselect(ISelectable selectable)
        {
            if (selectable.TryGetUnitGroup(out ISelectableGroup group))
                selectable = group;

            if (SelectedUnits.Remove(selectable))
            {
                selectable.Deselect();
                OnUnitSelectionChange?.Invoke(SelectedUnits);
            }
        }

        /// <summary>
        /// Adds objects to current active selection list and calls
        /// <see cref="ISelectable.Select"/> on them if
        /// any object contains component <see cref="ISelectable"/>.
        /// <see cref="ChangeListener"/> is also notified.
        /// </summary>
        /// <param name="objectToSelect"></param>
        private void AddNewSelections(List<ISelectable> objectToSelect)
        {
            SelectedUnits.AddRange(objectToSelect);
            objectToSelect.ForEach(selectable => selectable.Select());

            // Once finished, check if any objects were selected and notify the handler
            if (objectToSelect.Count > 0)
            {
                OnUnitSelectionChange?.Invoke(SelectedUnits);
            }
        }

        #endregion

        #region Highlights

        /// <summary>
        /// Highlights any newly highlighted objects that are currently not
        /// on the highlighted list, and unhighlights any objects that are no
        /// longer on the newly highlighted object list.
        /// </summary>
        /// <param name="selectables">Selectable units that will replace currently
        /// highlighted units</param>
        /// <param name="filterOutAlreadySelectedUnits">Specifies if selected units
        /// will be filtered out as well</param>
        public void Highlight(List<ISelectable> selectables, bool filterOutAlreadySelectedUnits)
        {
            var seletables = FilterObjectsForSelection(selectables, filterOutAlreadySelectedUnits);

            // Call highlight on the newly selected units
            seletables.ForEach(selectable => {
                if (!HighlightedUnits.Contains(selectable))
                    selectable.Highlight();
            });

            // Call unhighlight on units that are no longer in the
            // selection area
            HighlightedUnits.ForEach(selectable =>
            {
                if (!seletables.Contains(selectable))
                    selectable.Unhighlight();
            });

            HighlightedUnits = seletables;

            OnUnitHighlightChange?.Invoke(HighlightedUnits);
        }

        /// <summary>
        /// Clears the highlighted unit list and invokes <see cref="ISelectable.Unhighlight"/>
        /// on each of them.
        /// </summary>
        public void UnhighlightAll()
        {
            UnhighlightAll(true);
        }

        /// <summary>
        /// Clears the highlighted unit list and invokes <see cref="ISelectable.Unhighlight"/>
        /// on each of them.
        /// </summary>
        /// <param name="invokeChangeEvent">Set true when update event should be invoked after the update.</param>
        private void UnhighlightAll(bool invokeChangeEvent)
        {
            // Clear highlighted objects
            HighlightedUnits.ForEach(selectable => selectable.Unhighlight());
            HighlightedUnits.Clear();

            if (invokeChangeEvent)
                // Invoke change event with empty list
                OnUnitHighlightChange?.Invoke(HighlightedUnits);
        }

        /// <summary>
        /// Stores the new hovering unit reference and updates its highlighted
        /// state if it implements <see cref="ISelectable"/> interface.
        /// </summary>
        /// <param name="newHoverUnit">Object to be highlighted</param>
        internal void SetHoveringUnit(ISelectable newHoverUnit)
        {
            // Use group if present
            if (supportsGroups && newHoverUnit.TryGetUnitGroup(out ISelectableGroup group))
                newHoverUnit = group;

            if (HoveringOverUnit == newHoverUnit)
            {
                // Hovering object is the same, nothing to change.
                return;
            }

            if (HoveringOverUnit != null)
            {
                RemoveHoveringUnit();
            }

            HoveringOverUnit = newHoverUnit;
            newHoverUnit.Highlight();

            OnUnitHoverChange?.Invoke(HoveringOverUnit);
        }

        /// <summary>
        /// Removes highlight from hovering unit and clears the reference.
        /// </summary>
        internal void RemoveHoveringUnit()
        {
            if (HoveringOverUnit == null) return;

            // Make sure the unit is not highlighted by drag as well, otherwise
            // unhighlight will remove its visuals and not reset them
            if (!HighlightedUnits.Contains(HoveringOverUnit))
            {
                HoveringOverUnit.Unhighlight();
            }

            HoveringOverUnit = null;
        }

        #endregion

        #region Helpers

        /// <summary>
        /// Removes objects at the end of the list if the max selection has
        /// been reached. Also filters out selected units if specified and invokes
        /// use of <see cref="Filter"/> and <see cref="Sorter"/>.
        /// </summary>
        /// <param name="selectables">Selectables to manipulate</param>
        /// <param name="filterOutAlreadySelectedUnits">Specifies if selected units
        /// will be filtered out as well</param>
        /// <returns>Returns list of filtered out objects, if holding down SHIFT
        /// or reaches maximum allowed selection.</returns>
        private List<ISelectable> FilterObjectsForSelection(
            List<ISelectable> selectables,
            bool filterOutAlreadySelectedUnits)
        {
            if (selectables.Count == 0) return selectables;

            int possibleSelections = MaxActiveSelections;
            List<ISelectable> tempSelectables = new(selectables);

            // First convert single units to group units if supported and required
            if (supportsGroups)
                tempSelectables.ReplaceGroupUnitsWithGroups();

            if (filterOutAlreadySelectedUnits)
            {
                // If shift is being held down, objects will be appended and
                // not replaced, so we need to reduce the max allowed count.
                possibleSelections -= SelectedUnits.Count;
                FilterOutSelectedObjects(tempSelectables);
            }

            // If we are filtering selected units out it means we are adding
            // to the existing selection, otherwise we are replacing it.
            FilterSelectionType selectionType = filterOutAlreadySelectedUnits ? FilterSelectionType.AddSelection : FilterSelectionType.ReplaceSelection;
            foreach (var filter in Filters)
                filter.Filter(tempSelectables, selectionType);

            // Additionally sort with custom sorter if set before limiting selection.
            Sorter?.Sort(tempSelectables);

            if (ApplyMaxActiveSelections)
            {
                var selectCount = Mathf.Min(possibleSelections, tempSelectables.Count);
                tempSelectables = tempSelectables.GetRange(0, selectCount);
            }

            return tempSelectables;
        }

        /// <summary>
        /// Filters out objects that are already in present in <see cref="SelectedUnits"/>
        /// </summary>
        /// <param name="selectables">List from which elements will be filtered.</param>
        private void FilterOutSelectedObjects(List<ISelectable> selectables)
        {
            for (int index = selectables.Count-1; index >= 0; index--)
            {
                // If object is already selected, remove it from list
                if (SelectedUnits.Contains(selectables[index]))
                {
                    selectables.RemoveAt(index);
                }
            }
        }

        #endregion

    }

}