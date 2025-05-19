using System.Collections.Generic;
using TRavljen.UnitFormation.Placement;
using UnityEngine;

namespace TRavljen.UnitSystem.Demo
{

    public class TargetedFormationUnitInteractions : TargetedUnitInteractions
    {

        [Tooltip("Handles formation placement for selected units.")]
        [SerializeField]
        private FormationPlacement formationPlacement;

        protected override void OnValidate()
        {
            base.OnValidate();
            
            if (formationPlacement.IsNull())
                formationPlacement = Object.FindFirstObjectByType<FormationPlacement>();
        }

        protected override bool MoveToFormation(List<Unit> units, Vector3 groundPosition)
        {
            if (!formationPlacement.IsNotNull() || units.Count <= 0) 
                return base.MoveToFormation(units, groundPosition);
            
            // If count matches, all were added to the formation and we can proceed
            var formationActive = !formationPlacement.IsPlacementActive && 
                                  formationPlacement.UnitFormation.Units.Count == units.Count;

            // If no entities were hit for interactions and formation is active,
            // then position the units. This is very basic logic, cases
            // where some interact with entity but some don't and should keep formation
            // is not handled.
            if (!formationActive) return base.MoveToFormation(units, groundPosition);
            
            foreach (var unit in units)
                unit.RemoveAllTasks();

            formationPlacement.PositionUnits(groundPosition, true);
            return true;

        }

    }

}