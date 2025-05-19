using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TRavljen.UnitSelection
{

    /// <summary>
    /// Basic implementation of the <see cref="ISelectableGroupUnit"/> intended
    /// to be used with <see cref="SelectableGroup"/>. This class defines
    /// a single selectable unit within the group. Selecting this unit will
    /// select it's group instead. Though it will still delegate events and
    /// update states on each unit within the group.
    /// 
    /// This component can be used when selecting a single unit from the group
    /// should select the group itself.
    /// </summary>
    public class SelectableGroupUnit : SelectableUnit, ISelectableGroupUnit
    {

        // Editor version
        [Tooltip("Specifies reference of the unit's group.")]
        [SerializeField]
        private SelectableGroup group;

        // Common version
        private ISelectableGroup _group;

        // Public getter
        public ISelectableGroup Group => _group;

        protected virtual void Awake() => _group = group;

        public void SetGroup(ISelectableGroup group)
            => _group = group;

        protected override void OnEnable()
        {
            base.OnEnable();

            _group.AddGroupUnit(this);
        }

        protected virtual void OnDisable()
        {
            _group.RemoveGroupUnit(this);
        }

    }

}