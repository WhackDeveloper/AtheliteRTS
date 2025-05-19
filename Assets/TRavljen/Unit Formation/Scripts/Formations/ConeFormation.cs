using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TRavljen.UnitFormation.Formations
{

    /// <summary>
    /// Formation that positions units in a cone shape with specified spacing.
    /// </summary>
    [System.Serializable]
    public struct ConeFormation : IFormation
    {
        [SerializeField, Range(0.1f, 100f)] private float unitSpacing;
        [SerializeField] private bool pivotInCenter;

        /// <summary>
        /// Instantiates cone formation.
        /// </summary>
        /// <param name="unitSpacing">Specifies spacing between units.</param>
        /// <param name="pivotInCenter">Specifies if the pivot of the formation is
        /// in the middle of units. By default it is in first row of the formation.
        /// If this is set to true, rotation of formation will be in the center.</param>
        public ConeFormation(float unitSpacing, bool pivotInCenter = true)
        {
            this.unitSpacing = unitSpacing;
            this.pivotInCenter = pivotInCenter;
        }

        public List<Vector3> GetPositions(int unitCount)
        {
            List<Vector3> unitPositions = new List<Vector3>();

            // Offset starts at 0, then each row is applied change for half of spacing
            float currentRowOffset = 0f;
            float x, z;
            int columnsInRow;
            int row;

            for (row = 0; unitPositions.Count < unitCount; row++)
            {
                columnsInRow = row + 1;

                x = 0 * unitSpacing + currentRowOffset;
                z = row * unitSpacing;

                unitPositions.Add(new Vector3(x, 0, -z));

                if (unitPositions.Count < unitCount && columnsInRow > 1)
                {
                    x = (columnsInRow - 1) * unitSpacing + currentRowOffset;
                    z = row * unitSpacing;

                    unitPositions.Add(new Vector3(x, 0, -z));
                }

                currentRowOffset -= unitSpacing / 2;
            }

            if (pivotInCenter)
                UnitFormationHelper.ApplyFormationCentering(unitPositions, row, unitSpacing);

            return unitPositions;
        }

    }
}