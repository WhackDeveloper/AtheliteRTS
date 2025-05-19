using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TRavljen.UnitSelection.Utility
{

    public static class SelectableUnitsUtility
    {

        /// <summary>
        /// Finds all visible units on screen and sorts them based on distance
        /// their screen position and distance from the mouse.
        /// </summary>
        /// <param name="screenPosition">Mouse screen position</param>
        /// <param name="selectables">Game objects to filter</param>
        /// <param name="camera">Player camera</param>
        /// <returns>Returns sorted selectable objects visible on screen.</returns>
        public static List<ISelectable> SortUnitsBasedOnScreenPosition(
            Vector3 screenPosition,
            List<ISelectable> selectables,
            Camera camera)
        {
            List<SortUnit> unitsOnScreen = new List<SortUnit>();
            var viewportRect = new Rect(0f, 0f, 1f, 1f);

            // First check if the unit is present on the screen
            foreach (ISelectable unit in selectables)
            {
                Vector3 unitPosition = unit.gameObject.transform.position;
                Vector2 unitViewportPos = camera.WorldToViewportPoint(unitPosition);
                var isWithinCameraFrame = viewportRect.Contains(unitViewportPos);

                // Check if unit is on screen
                if (isWithinCameraFrame)
                {
                    Vector2 screenPos = camera
                        .WorldToScreenPoint(unitPosition);
                    float distance = Vector3.Distance(screenPos, screenPosition) * 100;

                    unitsOnScreen.Add(new SortUnit(unit, (int)distance));
                }
            }

            unitsOnScreen.Sort((a, b) => a.Distance.CompareTo(b.Distance));
            return unitsOnScreen.ConvertAll(pair => pair.Unit);
        }
    }

}