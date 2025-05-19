using System.Collections;
using System.Collections.Generic;

namespace IntegrationDemo
{

    using TRavljen.UnitSystem;
    using TRavljen.UnitSystem.Demo;
    using TRavljen.UnitSelection;

    /// <summary>
    /// Integration layer for managing unit selection between the UnitSelector system 
    /// and the core <see cref="EntitySelectionManager"/>. This component ensures 
    /// synchronization between the selectable units in the integration layer and the 
    /// EntitySelectionManager's selection system.
    /// </summary>
    /// <remarks>
    /// - Observes changes in unit selection via the <see cref="UnitSelector"/> and 
    /// applies them to the <see cref="EntitySelectionManager"/>.
    /// - Updates the <see cref="UnitSelector"/> when entities are selected, deselected, 
    /// or highlighted.
    /// </remarks>
    public class PlayerUnitSelectionManager : EntitySelectionManager
    {

        private UnitSelector unitSelector;

        #region Lifecycle

        private void Start()
        {
            unitSelector = UnitSelector.Instance;
            unitSelector.ActiveSelections.OnUnitSelectionChange += SelectionChanged;
        }

        private void OnEnable()
        {
            // Observes selector changes and apply them to the manager
            if (unitSelector != null)
                unitSelector.ActiveSelections.OnUnitSelectionChange += SelectionChanged;
        }

        private void OnDisable()
        {
            if (unitSelector != null && unitSelector.isActiveAndEnabled)
                unitSelector.ActiveSelections.OnUnitSelectionChange -= SelectionChanged;
        }

        /// <summary>
        /// Called when the selection changes in the <see cref="UnitSelector"/>.
        /// Updates the selected entities in the <see cref="EntitySelectionManager"/>.
        /// </summary>
        /// <param name="selectables">The list of selected <see cref="ISelectable"/> units.</param>
        private void SelectionChanged(List<ISelectable> selectables)
        {
            List<Entity> selectedEntities = new();

            foreach (var selectable in selectables)
            {
                if (selectable.gameObject.TryGetComponent(out Entity entity))
                    selectedEntities.Add(entity);
            }

            SetSelectedEntities(selectedEntities);
        }

        #endregion

        #region Override

        /// <inheritdoc />
        public override void DeselectEntity(Entity unit)
        {
            base.DeselectEntity(unit);

            if (unit.TryGetComponent(out ISelectable selectable) &&
                unitSelector.ActiveSelections.SelectedUnits.Contains(selectable))
                unitSelector.ActiveSelections.Deselect(selectable);
        }

        /// <inheritdoc />
        public override void ClearSelectedEntities()
        {
            base.ClearSelectedEntities();

            if (unitSelector.ActiveSelections.SelectedUnits.Count > 0)
                unitSelector.ActiveSelections.DeselectAll();
        }

        /// <inheritdoc />
        public override void SetHighlightedEntities(List<Entity> entities)
        {
            FilterSelectables(ref entities, out List<ISelectable> selectables);

            // Prevents looping since this is observer + setter.
            if (AreListsEqualUnordered(entities, highlightedEntities))
                return;

            base.SetHighlightedEntities(entities);
            unitSelector.ActiveSelections.Highlight(selectables, true);
        }

        /// <inheritdoc />
        public override void SetSelectedEntities(List<Entity> entities)
        {
            FilterSelectables(ref entities, out List<ISelectable> selectables);

            // Prevents looping since this is observer + setter.
            if (AreListsEqualUnordered(entities, SelectedEntities))
                return;

            base.SetSelectedEntities(entities);
            unitSelector.ActiveSelections.ReplaceSelection(selectables, true);
        }

        #endregion

        #region Convenience

        /// <summary>
        /// Filters a list of entities and extracts their corresponding selectables.
        /// Removes entities that do not support selection in the integration.
        /// </summary>
        /// <param name="entities">The list of entities to filter.</param>
        /// <param name="selectables">The resulting list of selectables corresponding to the entities.</param>
        private void FilterSelectables(ref List<Entity> entities, out List<ISelectable> selectables)
        {
            selectables = new();

            for (int index = entities.Count - 1; index >= 0; index--)
            {
                Entity entity = entities[index];
                if (entity.TryGetComponent(out ISelectable selectable))
                {
                    selectables.Add(selectable);
                }
                else
                {
                    // Remove entities that do not support selection for this integration
                    entities.RemoveAt(index);
                }
            }
        }

        /// <summary>
        /// Checks whether two lists of entities are equal irrespective of order.
        /// </summary>
        /// <param name="list1">The first list of entities.</param>
        /// <param name="list2">The second list of entities.</param>
        /// <returns>
        /// Returns true if both lists contain the same entities, irrespective of order; 
        /// otherwise, false.
        /// </returns>
        private bool AreListsEqualUnordered(List<Entity> list1, List<Entity> list2)
        {
            // Check for null
            if (list1 == null || list2 == null)
                return false;

            // Check lengths first for optimization
            if (list1.Count != list2.Count)
                return false;

            // Use HashSet for unordered comparison
            return new HashSet<Entity>(list1).SetEquals(list2);
        }

        #endregion
    }

}