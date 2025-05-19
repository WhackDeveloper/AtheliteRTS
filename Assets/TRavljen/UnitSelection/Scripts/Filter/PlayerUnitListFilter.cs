using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TRavljen.UnitSelection
{

    /// <summary>
    /// Simple example of filter usage, primarily utilising a list of player units.
    /// Preferrably use <see cref="ManagedPlayerUnitsFilter"/>.
    /// </summary>
    public class PlayerUnitListFilter : APlayerSelectionFilter
    {

        [Tooltip("List of player units that can be defined in the editor.\n" +
            "Add and remove units during runtime with Add and Remove methods on filter.\n\n" +
            "Adjusting unit list won't work during runtime, even in the editor.")]
        [SerializeField]
        private List<GameObject> playerUnits = new List<GameObject>();

        private readonly List<ISelectable> playerSelectableUnits = new List<ISelectable>();

        public List<ISelectable> PlayerSelectables => playerSelectableUnits;

        protected override void Start()
        {
            base.Start();

            // Convert game objects to selectable list, so that it is done
            // once and not for every filtering
            foreach (var unit in playerUnits)
            {
                if (unit.TryGetComponent(out ISelectable selectable))
                    playerSelectableUnits.Add(selectable);
            }

            playerUnits.Clear();

            // Cleanup when killed or disabled.
            UnitManagementEvents.OnUnitRemoved.AddListener(RemoveUnit);
        }

        protected override bool IsOwnedByPlayer(ISelectable selectable)
            => playerSelectableUnits.Contains(selectable);

        /// <summary>
        /// Add selectable to players unit list.
        /// </summary>
        /// <param name="selectable">Selectable to add</param>
        public void AddUnit(ISelectable selectable)
        {
            playerSelectableUnits.Add(selectable);
        }

        /// <summary>
        /// Remove seletable from player unit list.
        /// </summary>
        /// <param name="selectable">Selectable to remove</param>
        public void RemoveUnit(ISelectable selectable)
        {
            playerSelectableUnits.Remove(selectable);
        }

    }

}