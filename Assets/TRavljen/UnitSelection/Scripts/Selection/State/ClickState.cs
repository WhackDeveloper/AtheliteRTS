using System.Collections.Generic;
using TRavljen.UnitSelection.Utility;
using UnityEngine;

namespace TRavljen.UnitSelection
{

    /// <summary>
    /// Performs click and moves selector to <see cref="IdleState"/>.
    /// </summary>
    internal sealed class ClickState : RaycastState
    {

        internal override SelectionStateId stateId => SelectionStateId.Click;

        private readonly DoubleClickHandler doubleClickHandler = new DoubleClickHandler();

        internal override void Enter(UnitSelector selector)
        {
            base.Enter(selector);

            if (!selector.ShouldIgnoreSelection())
                PerformClick(selector);
        }

        internal override void Update(UnitSelector selector)
        {
            base.Update(selector);

            selector.ChangeState(SelectionStateId.Idle);
        }

        private void PerformClick(UnitSelector selector)
        {
            var config = selector.Configuration;
            var selections = selector.ActiveSelections;

            // Make sure selection is enabled, otherwise leave it to exit.
            if (!selector.IsSelectionEnabled) return;

            if (!TryGetSelectable(selector, out ISelectable selectable))
            {
                // If mouse click MISSED a selectable object without holding
                // down the modification action, it should deselect all.
                if (!selector.IsModifyCurrentSelectionPressed())
                {
                    selections.DeselectAll();
                }

                selections.ClickedGameObject = null;
                return;
            }

            // Check if this click is double click (second click in short interval)
            // and if the second click is on the same unit as before.
            if (config.DoubleClickSelectionEnabled &&
                doubleClickHandler.HandleClick() &&
                selections.ClickedGameObject == selectable)
            {
                if (selector.UnitManager == null)
                {
                    Debug.Log("Unit manager missing on double click. Creating a new instance.");
                    selector.UnitManager = UnitManager.GetOrCreate();
                }   

                List<ISelectable> selectableUnits = selector.UnitManager.SelectableUnits;

                var unitsOnScreen = SelectableUnitsUtility.SortUnitsBasedOnScreenPosition(
                    selector.InputControl.MousePosition,
                    selectableUnits,
                    selector.Camera);

                if (selector.IsModifyCurrentSelectionPressed())
                {
                    selections.AddSelection(unitsOnScreen);
                }
                else
                {
                    selections.ReplaceSelection(unitsOnScreen);
                }
            }
            else
            {
                selections.ClickedGameObject = selectable;

                if (selector.IsModifyCurrentSelectionPressed())
                {
                    selections.ToggleSingleSelection(selectable);
                }
                else
                {
                    selections.ReplaceSelection(selectable);
                }
            }
        }

    }

}