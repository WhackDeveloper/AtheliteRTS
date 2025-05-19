using UnityEngine;
using System.Collections.Generic;

namespace TRavljen.UnitSelection
{

    public static class SelectableGroupUtility
    {

        public static List<ISelectable> GetSingleSelectableUnits(this IUnitManager manager)
        {
            List<ISelectable> units = new List<ISelectable>(manager.SelectableUnits);
            units.ReplaceGroupsWithGroupUnits();
            return units;
        }

        /// <summary>
        /// Removes any <see cref="ISelectableGroup"/> and replaces it with
        /// <see cref="ISelectableGroup.GroupUnits"/>.
        /// </summary>
        /// <param name="selectables">List of selectables to modify</param>
        public static void ReplaceGroupsWithGroupUnits(this List<ISelectable> selectables)
        {
            for (int index = selectables.Count - 1; index >= 0; index--)
            {
                ISelectable selectable = selectables[index];

                if (selectable is ISelectableGroup group)
                {
                    // Remove group and replace it with its single units
                    selectables.RemoveAt(index);
                    selectables.AddRange(group.GroupUnits);
                }
            }
        }

        /// <summary>
        /// Calculates the center of all units within the group.
        /// This can be used to display some UI over the group position.
        /// </summary>
        /// <param name="group">Group to get center from.</param>
        /// <returns>Returns center of all units.</returns>
        public static Vector3 CalculateGroupCenter(this ISelectableGroup group)
        {
            return CalculateGroupCenter(group.GroupUnits);
        }

        /// <summary>
        /// Calculates the center of all the <paramref name="selectables"/> in the list.
        /// This can be used to display some UI over the group position.
        /// </summary>
        /// <param name="selectables">List of group units.</param>
        /// <returns>Returns center of all units.</returns>
        public static Vector3 CalculateGroupCenter(List<ISelectableGroupUnit> selectables)
        {
            Vector3 center = Vector3.zero;

            foreach (ISelectable selectable in selectables)
            {
                center += selectable.gameObject.transform.position;
            }

            return center / selectables.Count;
        }

        /// <summary>
        /// Replaces any <see cref="ISelectableGroupUnit"/> occurrences with
        /// <see cref="ISelectableGroup"/> for grouped units should be handled
        /// through their group.
        /// </summary>
        /// <param name="selectables">List of selectables to modify</param>
        public static void ReplaceGroupUnitsWithGroups(this List<ISelectable> selectables)
        {
            HashSet<ISelectableGroup> newlySelectedGroups = new();

            for (int index = selectables.Count - 1; index >= 0; index--)
            {
                var selectable = selectables[index];

                // Do this only for units of groups
                if (!TryGetUnitGroup(selectable, out ISelectableGroup group))
                    continue;

                // If this group already exists, remove it to avoid duplicates
                if (newlySelectedGroups.Contains(group))
                {
                    selectables.RemoveAt(index);
                }
                else
                {
                    newlySelectedGroups.Add(group);
                    selectables[index] = group;
                }
            }
        }

        /// <summary>
        /// Checks if selectable is a group unit and retrieves the group from it.
        /// </summary>
        /// <param name="selectable">Selectable object, potentially group selectable.</param>
        /// <param name="group">Group of the selectable group unit.</param>
        /// <returns>Returns true if the group was retrieved.</returns>
        public static bool TryGetUnitGroup(this ISelectable selectable, out ISelectableGroup group)
        {
            if (selectable is ISelectableGroup _group)
            {
                group = _group;
                return true;
            }

            if (selectable is ISelectableGroupUnit groupUnit)
            {
                group = groupUnit.Group;

                // Validate
                if (groupUnit.Group == null)
                {
                    Debug.LogWarning("Selecting a group unit without Group reference.");
                    group = null;
                    return false;
                }

                return true;
            }

            group = null;
            return false;
        }

    }

}