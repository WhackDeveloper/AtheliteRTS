using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TRavljen.UnitSelection
{

    /// <summary>
    /// Performs raycasts for mouse hover over a unit. If unit does not have
    /// a collider, this feature will not work.
    /// This state is not responsible for switching to other behaviours.
    /// </summary>
    internal sealed class HoverState : RaycastState
    {

        internal override SelectionStateId stateId => SelectionStateId.Hover;

        internal override void Update(UnitSelector selector)
        {
            base.Update(selector);

            // Check if UI element is in the way, then unhighlight the unit
            if (selector.ShouldIgnoreSelection())
            {
                selector.ActiveSelections.RemoveHoveringUnit();
            }
            else
            {
                PerformHover(selector);
            }
        }

        private void PerformHover(UnitSelector selector)
        {
            if (!TryGetSelectable(selector, out ISelectable selectable))
            {
                selector.ActiveSelections.RemoveHoveringUnit();
                return;
            }

            selector.ActiveSelections.SetHoveringUnit(selectable);
        }
    }

}