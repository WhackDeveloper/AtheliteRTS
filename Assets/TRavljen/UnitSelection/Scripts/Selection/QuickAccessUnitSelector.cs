using System.Collections.Generic;
using UnityEngine;

namespace TRavljen.UnitSelection
{

    /// <summary>
    /// Selector for quick access to units. Supports saving and selecting saved units.
    /// <see cref="InputControl"/> is responsible for invoking quick selection
    /// actions.
    /// 
    /// When action <see cref="IInputControl.OnQuickSelectionToggle"/> is
    /// invoked and <see cref="IInputControl.IsQuickSavePressed"/> is 'true',
    /// the currently selected units will be saved under the index of the
    /// invoked action.
    /// If <see cref="IInputControl.IsQuickSavePressed"/> is 'false' when toggle
    /// is invoked, then current selection will be replaced with units saved
    /// under the index of action invoked; This is only true if there is something
    /// saved for the index.
    /// </summary>
    [RequireComponent(typeof(ActiveSelections), typeof(UnitSelector))]
    public class QuickAccessUnitSelector : MonoBehaviour
    {

        #region Properties

        /// <summary>
        /// Specifies if the quick access feature is enabled. If this is set to
        /// 'false' they will simply be ignored.
        /// </summary>
        [Tooltip("Specifies if the quick access keys are enabled. If this is set " +
            "to 'false' they will simply be ignored.")]
        public bool EnableQuickAccess = true;

        /// <summary>
        /// Current input reference in use.
        /// </summary>
        private IInputControl _inputControl;

        /// <summary>
        /// Quick selection stored reference. Each index is correlated to its
        /// action index available on input control.
        /// </summary>
        private readonly Dictionary<int, List<ISelectable>> quickAccessSelections = new Dictionary<int, List<ISelectable>>();

        private ActiveSelections activeSelections;

        /// <summary>
        /// Returns the original copy of saved selection, open for modification.
        /// </summary>
        /// <returns></returns>
        public Dictionary<int, List<ISelectable>> GetSavedSelections()
            => quickAccessSelections;

        #endregion

        #region Lifecycle

        private void Start()
        {
            activeSelections = GetComponent<ActiveSelections>();
            UnitManagementEvents.OnUnitRemoved.AddListener(CleanUpAfterUnit);

            if (_inputControl == null && TryGetComponent(out UnitSelector unitSelector))
            {
                SetInputControl(unitSelector.InputControl);
            }
        }

        private void OnDestroy()
        {
            if (_inputControl != null)
                _inputControl.OnQuickSelectionToggle -= ToggleQuickSelection;
            UnitManagementEvents.OnUnitRemoved.RemoveListener(CleanUpAfterUnit);
        }

        private void CleanUpAfterUnit(ISelectable unit)
        {
            foreach (var access in quickAccessSelections)
            {
                // Removes unit if present. To avoid any null references in future.
                access.Value.Remove(unit);
            }
        }

        #endregion

        #region Logic

        /// <summary>
        /// Set a new input control reference. This method is internally
        /// invoked by <see cref="UnitSelector"/> when it's input is updated.
        /// </summary>
        /// <param name="input">New input control</param>
        internal void SetInputControl(IInputControl input)
        {
            if (_inputControl != null)
                _inputControl.OnQuickSelectionToggle -= ToggleQuickSelection;

            _inputControl = input;

            if (_inputControl != null)
                _inputControl.OnQuickSelectionToggle += ToggleQuickSelection;
        }

        /// <summary>
        /// Manually save selection on a desired index.
        /// </summary>
        /// <param name="actionIndex">Index to save on</param>
        /// <param name="selection">Selection objects to save</param>
        public void SaveSelection(int actionIndex, List<ISelectable> selection)
        {
            if (!EnableQuickAccess) return;

            RemoveSavedSelection(actionIndex);
            quickAccessSelections.Add(actionIndex, selection);
        }

        /// <summary>
        /// Remove selection from desired index.
        /// </summary>
        /// <param name="actionIndex">Index to remove from</param>
        /// <returns>
        /// Returns true if remove was successful, false is returned
        /// when there is no valid index saved.
        /// </returns>
        public bool RemoveSavedSelection(int actionIndex)
        {
            if (quickAccessSelections.ContainsKey(actionIndex))
                return quickAccessSelections.Remove(actionIndex);
            return false;
        }

        /// <summary>
        /// Get selection from desired index.
        /// </summary>
        /// <param name="actionIndex">Index of saved selection</param>
        /// <param name="selection">Selection result</param>
        /// <returns>returns false if there is no valid selection for specified index</returns>
        public bool TryGetSavedSelection(int actionIndex, out List<ISelectable> selection)
            => quickAccessSelections.TryGetValue(actionIndex, out selection);

        /// <summary>
        /// Toggle quick selection, may switch between indexes or save
        /// current selection.
        /// </summary>
        /// <param name="actionIndex">Action index</param>
        private void ToggleQuickSelection(int actionIndex)
        {
            if (!EnableQuickAccess) return;

            // Check if player is saving units
            if (_inputControl.IsQuickSavePressed)
            {
                if (quickAccessSelections.ContainsKey(actionIndex))
                {
                    quickAccessSelections.Remove(actionIndex);
                }

                var currentSelection = activeSelections.SelectedUnits;
                quickAccessSelections.Add(
                    actionIndex, new List<ISelectable>(currentSelection));
            }
            // Otherwise check if player has unist to select on the index
            else if (quickAccessSelections
                .TryGetValue(actionIndex, out List<ISelectable> list))
            {
                // Avoid any processing on the passed selectables, since they are
                // pre-defined and should not be changed.
                // Either they were manually saved by game or the player is able
                // to select such a group based on selection configurations.
                activeSelections.ReplaceSelection(list, processSelection: false);
            }
        }

        #endregion
    }

}