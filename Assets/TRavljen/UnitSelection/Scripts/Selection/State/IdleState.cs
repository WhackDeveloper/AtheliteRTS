using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TRavljen.UnitSelection
{
    /// <summary>
    /// Resets any selection and moves to either <see cref="HoverState"/> or
    /// <see cref="DragState"/>, depending on <see cref="UnitSelector"/> configuration.
    /// </summary>
    internal sealed class IdleState : SelectorBaseState
    {
        internal override SelectionStateId stateId => SelectionStateId.Idle;

        internal override void Enter(UnitSelector selector)
        {
            base.Enter(selector);

            // Reset state if active
            if (selector.MouseState.IsActive)
            {
                if (selector.SelectionArea)
                    selector.SelectionArea.MouseDragStops();

                selector.MouseState = new MouseClickState();
            }
        }

        internal override void Update(UnitSelector selector)
        {
            base.Update(selector);

            if (selector.IsSelectionEnabled)
            {
                if (selector.MouseState.IsActive)
                {
                    // Drag is active, move to corresponding state
                    selector.ChangeState(SelectionStateId.Drag);
                }
                else if (selector.Configuration.HighlightOnMouseHover)
                {
                    // Not dragging and hover is enabled
                    selector.ChangeState(SelectionStateId.Hover);
                }
            }
        }
    }

}