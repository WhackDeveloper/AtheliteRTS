using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TRavljen.UnitSelection
{

    internal abstract class RaycastState : SelectorBaseState
    {
        private RaycastHit[] tempHits = new RaycastHit[10];

        internal override void Enter(UnitSelector selector)
        {
            base.Enter(selector);

            int maxHits = Mathf.Max(1, selector.Configuration.MaxRaycastHits);
            if (maxHits != tempHits.Length)
            {
                tempHits = new RaycastHit[maxHits];
            }
        }

        internal override void Exit(UnitSelector selector)
        {
            base.Exit(selector);

            // Cleanup hits
            for (int index = 0; index < tempHits.Length; index++)
            {
                tempHits[index] = default;
            }
        }

        /// <summary>
        /// Performs raycast based on <see cref="UnitSelector.configuration"/>
        /// and attempts to return a selectable object if one was hit on the
        /// mouse position.
        /// </summary>
        /// <param name="selector">Selector, the main component</param>
        /// <param name="selectable">Returned selectable if found</param>
        /// <returns>Returns true if selectable was found, otherwise returns false.</returns>
        protected bool TryGetSelectable(UnitSelector selector, out ISelectable selectable)
        {
            Ray ray = selector.Camera.ScreenPointToRay(selector.InputControl.MousePosition);
            var config = selector.Configuration;

            var type = selector.Configuration.RaycastType;
            switch (type)
            {
                // Use raycast all and sort them
                case SelectionRaycastType.Nearest:
                case SelectionRaycastType.Furthest:
                    int length = Physics.RaycastNonAlloc(ray, tempHits, config.MaxSelectionDistance, config.SelectableLayerMask);

                    selectable = null;

                    if (length == 0) return false;

                    RaycastHit hit;
                    float currentDistance = -1;

                    for (int index = 0; index < length; index++)
                    {
                        hit = tempHits[index];

                        if (hit.collider.TryGetComponent(out ISelectable temp))
                        {
                            float distance = Vector3.Distance(hit.point, ray.origin);
                            // First valid selectable item.
                            if (currentDistance < 0)
                            {
                                selectable = temp;
                                currentDistance = distance;
                            }
                            else if (type == SelectionRaycastType.Nearest &&
                                distance < currentDistance)
                            {
                                selectable = temp;
                                currentDistance = distance;
                            }
                            else if (type == SelectionRaycastType.Furthest &&
                                distance > currentDistance)
                            {
                                selectable = temp;
                                currentDistance = distance;
                            }
                        }
                    }

                    return selectable != null;

                // Use single raycast
                case SelectionRaycastType.SingleHit:
                default:
                    if (Physics.Raycast(ray, out RaycastHit singleHit, config.MaxSelectionDistance, config.SelectableLayerMask))
                    {
                        selectable = singleHit.collider.GetComponent<ISelectable>();
                        return selectable != null;
                    }

                    selectable = null;
                    return false;

            }
        }
    }
}