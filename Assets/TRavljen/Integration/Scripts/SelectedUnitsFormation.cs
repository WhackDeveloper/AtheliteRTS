using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace IntegrationDemo
{
    using TRavljen.UnitSystem;
    using TRavljen.UnitSystem.Demo;
    using TRavljen.UnitFormation;

    /// <summary>
    /// Manages units within the controlled formation. This component listens to 
    /// selection changes on the <see cref="EntitySelectionManager"/> and updates 
    /// the formation to include only the selected units belonging to the player 
    /// that have movement capabilities.
    /// </summary>
    public class SelectedUnitsFormation : MonoBehaviour
    {

        #region Serialized Fields

        /// <summary>
        /// Specifies the player owning the units. Units not owned by this player 
        /// will be ignored when updating the formation.
        /// </summary>
        [Tooltip("Specifies the player owner for units.")]
        [SerializeField]
        private APlayer player;

        /// <summary>
        /// The selection manager responsible for managing entity selection.
        /// </summary>
        [Tooltip("Specifies the selection manager of the player.")]
        [SerializeField]
        private EntitySelectionManager selectionManager;

        /// <summary>
        /// The unit formation that this component updates. Only units with movement 
        /// capabilities and owned by the specified player are added to the formation.
        /// </summary>
        [Tooltip("Specifies the unit formation of the player controls.")]
        [SerializeField]
        private UnitFormation formation;

        #endregion

        private void Start()
        {
            // Subscribes to selection changes to update the formation dynamically.
            selectionManager.OnSelectionChange.AddListener(SelectionChanged);
        }

        private void OnValidate()
        {
            if (player.IsNull())
                player = GetComponentInParent<APlayer>();
            
            if (selectionManager.IsNull())
                selectionManager = UnityEngine.Object.FindFirstObjectByType<EntitySelectionManager>();
            
            if (formation.IsNull())
                formation = UnityEngine.Object.FindFirstObjectByType<UnitFormation>();
        }

        /// <summary>
        /// Updates the formation based on the current selection. Only includes units 
        /// that are owned by the player and have movement capabilities.
        /// </summary>
        /// <param name="selectedEntities">The list of currently selected entities.</param>
        private void SelectionChanged(List<Entity> selectedEntities)
        {
            formation.Units.Clear();

            // Adds only moving units
            List<Transform> movingUnits = new();
            foreach (var entity in selectedEntities)
            {
                if (entity is Unit unit && unit.Movement != null && entity.Owner == player)
                    movingUnits.Add(unit.transform);
            }

            formation.Units.AddRange(movingUnits);
        }

    }

}