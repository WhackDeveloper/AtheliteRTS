using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace TRavljen.UnitSelection
{

    /// <summary>
    /// Basic implementation of a <see cref="ISelectableGroup"/>.
    /// Class primarily manages the selection state of the group of units
    /// and delegates events to each <see cref="ISelectableGroupUnit"/>
    /// contained within the <see cref="GroupUnits"/>.
    /// 
    /// This component can be used when selecting a single unit from the group
    /// should select the group itself.
    /// </summary>
    public class SelectableGroup : SelectableUnit, ISelectableGroup
    {

        #region Properties

        public List<ISelectableGroupUnit> GroupUnits { get; private set; } = new();

        [Tooltip("Specifies the list of group units within the Editor.\n\n" +
            "This can also be achieved by referencing a group from unit within the Editor " +
            "or by using add/remove group unit methods in runtime.")]
        [SerializeField]
        private List<SelectableGroupUnit> groupUnits = new();

        [Tooltip("Group should be destroyed when empty to avoid using it for selection in any way." +
            "\nKeep this enabled if you are not removing this component or the game object by yourself.")]
        [SerializeField]
        private bool destroyGroupWhenEmptied = true;

        [SerializeField]
        [Tooltip("Specifies if selection indicator should be moved to the calculated center of all units within it.\n\n" +
            "This can remain disabled if there is no indicator or the indicator will be manually positioned by your systems.")]
        private bool centerSelectionIndicator = false;

        [Tooltip("Event invoked when all units were removed. Typically this happens" +
            "when all the units are killed.")]
        public UnityEvent OnAllGroupUnitsRemoved;

        #endregion

        #region ISelectableGroup

        protected virtual void Awake()
        {
            foreach (var unit in groupUnits)
                AddGroupUnit(unit);

            groupUnits.Clear();
        }

        /// <summary>
        /// Adds a new unit to the group, if not present already.
        /// Override this to extend behaviour.
        /// </summary>
        /// <param name="groupUnit">New group unit to add.</param>
        public virtual void AddGroupUnit(ISelectableGroupUnit groupUnit)
        {
            groupUnit.SetGroup(this);

            if (!GroupUnits.Contains(groupUnit))
            {
                if (IsSelected)
                    groupUnit.Select();
                if (IsHighlighted)
                    groupUnit.Highlight();

                GroupUnits.Add(groupUnit);
            }
        }

        /// <summary>
        /// Removes a unit from the group, if one is present.
        /// Override this to extend behaviour.
        /// </summary>
        /// <param name="groupUnit">Group unit to remove.</param>
        public virtual void RemoveGroupUnit(ISelectableGroupUnit groupUnit)
        {
            int previousCount = GroupUnits.Count;
            GroupUnits.Remove(groupUnit);
            int newCount = GroupUnits.Count;

            // Clear states
            groupUnit.Deselect();
            groupUnit.Unhighlight();

            // Check if list was emptied with this removal.
            if (newCount == 0 && previousCount != newCount)
            {
                OnAllGroupUnitsRemoved.Invoke();

                if (destroyGroupWhenEmptied)
                {
                    Destroy(this);
                }
                else
                {
                    // At least disable the component.
                    enabled = false;
                }
            }
        }

        #endregion

        #region ISelectable

        /// <summary>
        /// Deselects the group and all of its units.
        /// </summary>
        public override void Deselect()
        {
            base.Deselect();

            // Delegate events to group units
            foreach (var group in GroupUnits)
            {
                group.Deselect();
            }
        }

        /// <summary>
        /// Highlights the group and all of its units.
        /// </summary>
        public override void Highlight()
        {
            base.Highlight();

            IsHighlighted = true;
            foreach (var group in GroupUnits)
                group.Highlight();
        }

        /// <summary>
        /// Selects the group and all of its units.
        /// </summary>
        public override void Select()
        {
            base.Select();

            foreach (var group in GroupUnits)
                group.Select();
        }

        /// <summary>
        /// Unhighlights the group and all of its units.
        /// </summary>
        public override void Unhighlight()
        {
            base.Unhighlight();

            foreach (var group in GroupUnits)
                group.Unhighlight();
        }

        #endregion

        public override void UpdateSelectionIndicator()
        {
            if (selectionIndicator == null) return;

            base.UpdateSelectionIndicator();

            if (centerSelectionIndicator && (IsSelected || IsHighlighted))
            {
                selectionIndicator.transform.position = this.CalculateGroupCenter();
            }
        }

    }

}