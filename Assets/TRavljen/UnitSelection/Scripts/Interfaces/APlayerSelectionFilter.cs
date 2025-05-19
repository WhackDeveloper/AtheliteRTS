using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TRavljen.UnitSelection
{

    /// <summary>
    /// Implement this filter component to get strategy-like selection filtering.
    /// It is intended to manage and prioritise player's units versus friendly,
    /// neutral or enemy units.
    /// This is achieved by overriding the method <see cref="IsOwnedByPlayer(ISelectable)"/>.
    ///
    /// Extend method <see cref="Filter(List{ISelectable}, FilterSelectionType)"/>
    /// to adjust it as needed or call base.Filter() to use default behaviour.
    /// </summary>
    public abstract class APlayerSelectionFilter : MonoBehaviour, IFilterSelection
    {

        private ActiveSelections activeSelections;

        protected virtual void Start()
        {
            // Save reference for later use
            activeSelections = UnitSelector.Instance.ActiveSelections;

            // Set custom filter
            activeSelections.Filters.Add(this);
        }

        /// <summary>
        /// Implement this to perform a check if selectable unit is owned by
        /// the player. If game supports clicking only objects that player
        /// can interact with (no enemy/neutral), then this filtering should
        /// not be used as it would unnecessarily impact peformance.
        /// </summary>
        /// <param name="selectable">Selectable to be checked</param>
        /// <returns>Returns true if unit is owned by the player.</returns>
        protected abstract bool IsOwnedByPlayer(ISelectable selectable);

        /// <summary>
        /// Filters out enemy or friendly units accordingly.
        /// </summary>
        /// <param name="selectables">New selectables.</param>
        /// <param name="type">Type of selection action.</param>
        public virtual void Filter(List<ISelectable> selectables, FilterSelectionType type)
        {
            // Do not filter if component is disabled.
            if (!enabled) return;

            switch (type)
            {
                case FilterSelectionType.ReplaceSelection:
                    FilterOutEnemies(selectables);
                    break;

                case FilterSelectionType.AddSelection:
                    // If there is no active selection, simply filter out enemies.
                    if (activeSelections.SelectedUnits.Count == 0)
                    {
                        FilterOutEnemies(selectables);
                        return;
                    }

                    // Find all enemies
                    bool hasSelectedEnemies = ContainsEnemy(activeSelections.SelectedUnits);

                    // Check if current selection contains enemies
                    if (hasSelectedEnemies)
                    {
                        // Keep only enemies/filter out friendly units
                        FilterOutFriendlies(selectables);
                    }
                    // Check if there are friendlies only in existing selection
                    else if (!hasSelectedEnemies && activeSelections.SelectedUnits.Count > 0)
                    {
                        // Filter out enemies
                        FilterOutEnemies(selectables, forceRemove: true);
                    }
                    break;
            }
        }

        /// <summary>
        /// Filter out friendly units from the list.
        /// </summary>
        /// <param name="selectables">Selectable list to modify.</param>
        protected void FilterOutFriendlies(List<ISelectable> selectables)
        {
            // Find all enemies and save their indexes.
            for (int index = selectables.Count - 1; index >= 0; index--)
            {
                var selectedUnit = selectables[index];
                if (IsOwnedByPlayer(selectedUnit))
                {
                    selectables.RemoveAt(index);
                }
            }
        }

        /// <summary>
        /// Filter out enemy units from the list. If enemies occupy the entire selection list
        /// they must be removed by force. Only if they are partially present, will they be
        /// removed by default.
        /// </summary>
        /// <param name="selectables">Selectable list to modify.</param>
        /// <param name="forceRemove">Remove them by force, even if it means clearing the list.</param>
        protected void FilterOutEnemies(List<ISelectable> selectables, bool forceRemove = false)
        {
            List<int> enemyIndexes = GetIndexesOfEnemies(selectables);

            // If there was less enemies than the entire selection, filter them out.
            // Or if we need to remove it by force, regardless of this criteria.
            if (enemyIndexes.Count < selectables.Count)
            {
                for (int index = enemyIndexes.Count - 1; index >= 0; index--)
                {
                    selectables.RemoveAt(enemyIndexes[index]);
                }
            }
            else if (forceRemove)
            {
                selectables.Clear();
            }
        }

        /// <summary>
        /// Get indexes of enemies within the list.
        /// </summary>
        /// <param name="selectables">List to check.</param>
        /// <returns>Returns enemy index positions within the list.</returns>
        protected List<int> GetIndexesOfEnemies(List<ISelectable> selectables)
        {
            List<int> enemyIndexes = new List<int>();

            // Find all enemies and save their indexes.
            for (int index = 0; index < selectables.Count; index++)
            {
                var selectedUnit = selectables[index];
                if (!IsOwnedByPlayer(selectedUnit))
                {
                    enemyIndexes.Add(index);
                }
            }

            return enemyIndexes;
        }

        /// <summary>
        /// Check if the list contains a single enemy. Use this method for
        /// optimal performance when enemy count or indexes are not needed.
        /// </summary>
        /// <param name="selectables">List to check an enemy.</param>
        /// <returns>Return true once an enemy is found.</returns>
        protected bool ContainsEnemy(List<ISelectable> selectables)
        {
            // Find all enemies and save their indexes.
            foreach (var selectable in selectables)
            {
                if (!IsOwnedByPlayer(selectable))
                    return true;
            }

            return false;
        }
    }

}