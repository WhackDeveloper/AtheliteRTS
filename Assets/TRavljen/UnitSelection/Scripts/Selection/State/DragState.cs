using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace TRavljen.UnitSelection
{

    /// <summary>
    /// Performs drag from start to finish, highlights or selects units in selection area.
    /// If drag is too short it will request selector to perform click with <see cref="ClickState"/>.
    /// Finishes up with moving to <see cref="IdleState"/>.
    /// </summary>
    internal sealed class DragState : SelectorBaseState
    {

        internal override SelectionStateId stateId => SelectionStateId.Drag;

        private bool performClick = true;

        internal override void Enter(UnitSelector selector)
        {
            base.Enter(selector);

            // If dragging is enabled, selection area is required
            if (selector.SelectionArea == null)
            {
                // Log warning only on mouse down to inform the developer
                throw new System.ArgumentNullException("SelectionArea reference is missing! This is required for " +
                    "selection to work as it should do the calculation for selection");
            }

            var mousePosition = selector.InputControl.MousePosition;

            if (selector.SelectionArea.ShouldMouseDragStartSelection(mousePosition))
            {
                selector.MouseState = new MouseClickState(mousePosition, true);
                selector.UpdateSelectionArea();
            }

            performClick = true;
        }

        internal override void Update(UnitSelector selector)
        {
            base.Update(selector);

            if (!selector.IsSelectionEnabled || !selector.MouseState.IsActive)
            {
                selector.ChangeState(SelectionStateId.Idle);
                return;
            }

            PerformHighlight(selector);

            // Clear hovering unit if needed
            if (!performClick && selector.ActiveSelections.HoveringOverUnit != null)
                selector.ActiveSelections.RemoveHoveringUnit();
        }

        public void FinishDrag(UnitSelector selector)
        {
            if (performClick)
            {
                selector.ChangeState(SelectionStateId.Click);
            }
            else
            {
                FinishDragSelection(selector);
                selector.ChangeState(SelectionStateId.Idle);
            }
        }

        private void PerformHighlight(UnitSelector selector)
        {
            selector.MouseState.EndPos = selector.InputControl.MousePosition;

            var config = selector.Configuration;

            if (config.DragSelectionEnabled || config.DragHighlightEnabled)
                selector.UpdateSelectionArea();

            bool dragThresholdReached = selector.MouseState.Distance > config.MouseDragThreshold;
            if (performClick && dragThresholdReached)
                performClick = false;

            // Highlight when enabled and valid
            if (config.DragHighlightEnabled && dragThresholdReached)
            {
                selector.ValidateSelectionArea();
                var objectsToHighlight = selector.SelectionArea.GetCurrentObjectsWithinArea(config.SortSelectionByDistance);
                selector.ActiveSelections.Highlight(objectsToHighlight, selector.IsModifyCurrentSelectionPressed());
            }
        }

        private void FinishDragSelection(UnitSelector selector)
        {
            SelectorConfiguration config = selector.Configuration;

            List<ISelectable> objectsToSelect = new List<ISelectable>(selector.ActiveSelections.HighlightedUnits);

            // If highlight is disabled, selection should be performed now
            if (config.DragSelectionEnabled && !config.DragHighlightEnabled)
            {
                selector.ValidateSelectionArea();
                objectsToSelect = selector.SelectionArea.GetCurrentObjectsWithinArea(config.SortSelectionByDistance);
            }

            // Check if there are more than 1 objects to select, if there are
            // more select those, otherwise check if distance of drag is larger
            // than 20f. If its not, it should not be considered as a drag.
            if (objectsToSelect.Count > 0)
            {
                SetNewSelection(selector, objectsToSelect);
            }
            // Otherwise if modification is not pressed, clear current selection
            else if (!selector.IsModifyCurrentSelectionPressed())
            {
                selector.ActiveSelections.DeselectAll();
            }
        }

        private void SetNewSelection(UnitSelector selector, List<ISelectable> newSelection)
        {
            // Unhighlight units if selection is disabled, this keeps current selection.
            if (!selector.Configuration.DragSelectionEnabled)
            {
                selector.ActiveSelections.UnhighlightAll();
            }
            else
            {
                if (selector.IsModifyCurrentSelectionPressed())
                {
                    selector.ActiveSelections.AddSelection(newSelection);
                }
                else
                {
                    selector.ActiveSelections.ReplaceSelection(newSelection);
                }
            }
        }

    }

}
