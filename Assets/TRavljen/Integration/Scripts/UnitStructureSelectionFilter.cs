using System.Collections.Generic;
using UnityEngine;

namespace IntegrationDemo
{

    using TRavljen.UnitSelection;
    using TRavljen.UnitSystem.Build;
    using TRavljen.UnitSystem;

    /// <summary>
    /// Filters out buildings from multi-selection when there are other units present.
    /// </summary>
    public class UnitStructureSelectionFilter : MonoBehaviour, IFilterSelection
    {
        private UnitSelector unitSelector;

        #region Lifecycle

        private void Start()
        {
            unitSelector = UnitSelector.Instance;
            unitSelector.ActiveSelections.Filters.Add(this);
        }

        private void OnEnable()
        {
            // Observes selector changes and apply them to the manager
            if (unitSelector != null)
            {
                unitSelector.ActiveSelections.Filters.Remove(this);
                unitSelector.ActiveSelections.Filters.Add(this);
            }
        }

        private void OnDisable()
        {
            if (unitSelector != null && unitSelector.isActiveAndEnabled)
            {
                unitSelector.ActiveSelections.Filters.Remove(this);
            }
        }

        #endregion

        #region IFilterSelection

        /// <summary>
        /// Filters unit selections based on the current selection type, ensuring 
        /// that structures and units are handled appropriately.
        /// </summary>
        public void Filter(List<ISelectable> selectables, FilterSelectionType selectionType)
        {
            switch (selectionType)
            {
                case FilterSelectionType.ReplaceSelection:
                    // Filter structures out if they are not the only selection
                    if (GetStructureCount(selectables) != selectables.Count)
                        FilterStructures(selectables, true);

                    break;

                case FilterSelectionType.AddSelection:
                    bool isStructureSelected = GetStructureCount(selectables) > 0;
                    FilterStructures(selectables, !isStructureSelected);

                    break;
            }
        }

        private int GetStructureCount(List<ISelectable> selectables)
        {
            if (selectables.Count == 0) return 0;

            int count = 0;
            foreach (ISelectable selectable in selectables)
            {
                if (IsStructure(selectable.gameObject))
                    count++;
            }

            return count;
        }

        private void FilterStructures(List<ISelectable> selectables, bool filterOut)
        {
            for (int index = selectables.Count - 1; index >= 0; index--)
            {
                bool isStructure = IsStructure(selectables[index].gameObject);
                if (filterOut && isStructure)
                    selectables.RemoveAt(index);
                else if (!filterOut && !isStructure)
                    selectables.RemoveAt(index);
            }
        }

        protected virtual bool IsStructure(GameObject gameObject)
        {
            if (gameObject.TryGetComponent(out Unit unit))
            {
                // Generally structures/buildings should be defined by some
                // UnitTypeSO asset, but this is a simplified check that might
                // actually fail. We only check if entity requires building.
                return unit.TryGetCapability(out BuildableCapability _);
            }
            return false;
        }

        #endregion

    }
}