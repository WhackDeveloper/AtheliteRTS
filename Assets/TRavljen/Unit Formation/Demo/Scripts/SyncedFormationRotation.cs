using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TRavljen.UnitFormation.Demo
{

    /// <summary>
    /// Demonstration component for syncing direction change of all units.
    /// This will prevent units from facing formation angle until all units
    /// have reached their destination.
    /// </summary>
    public class SyncedFormationRotation : MonoBehaviour
    {

        [SerializeField]
        private UnitFormation formation;

        // Convenience getter. Best not to do this on Update in production.
        // Rather observe add/remove of units.
        private List<AFormationUnit> FormationUnits => formation.Units
            .ConvertAll(unit => unit.GetComponent<AFormationUnit>());

        void Start()
        {
            if (formation == null)
                formation = Object.FindFirstObjectByType<UnitFormation>();
        }

        void Update()
        {
            bool areAllPositioned = true;

            FormationUnits.ForEach(unit =>
            {
                if (!unit.IsWithinStoppingDistance)
                {
                    areAllPositioned = false;
                }
            });

            FormationUnits.ForEach(unit => unit.FacingRotationEnabled = areAllPositioned);
        }

    }

}