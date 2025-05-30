﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TRavljen.UnitFormation.Formations
{

    /// <summary>
    /// Formation that positions units in a triangle with specified spacing.
    /// </summary>
    [System.Serializable]
    public struct TriangleFormation : IFormation
    {
        [SerializeField, Range(0.1f, 100f)] private float unitSpacing;
        [SerializeField] private bool centerUnits;
        [SerializeField] private bool pivotInCenter;

        /// <summary>
        /// Instantiates triangle formation.
        /// </summary>
        /// <param name="unitSpacing">Specifies spacing between units.</param>
        /// <param name="centerUnits">Specifies if units should be centered if
        /// they do not fill the full space of the row.</param>
        /// <param name="pivotInCenter">Specifies if the pivot of the formation is
        /// in the middle of units. By default it is in first row of the formation.
        /// If this is set to true, rotation of formation will be in the center.</param>
        public TriangleFormation(float unitSpacing, bool centerUnits = true, bool pivotInCenter = false)
        {
            this.unitSpacing = unitSpacing;
            this.centerUnits = centerUnits;
            this.pivotInCenter = pivotInCenter;
        }

        public List<Vector3> GetPositions(int unitCount)
        {
            List<Vector3> unitPositions = new List<Vector3>();

            // Offset starts at 0, then each row is applied change for half of spacing
            float currentRowOffset = 0f;
            float x, z;
            int row;

            for (row = 0; unitPositions.Count < unitCount; row++)
            {
                // Current unit positions are the index of first unit in row
                var columnsInRow = row + 1;
                var firstIndexInRow = unitPositions.Count;

                for (int column = 0; column < columnsInRow; column++)
                {
                    x = column * unitSpacing + currentRowOffset;
                    z = row * unitSpacing;

                    // Check if centering is enabled and if row has less than maximum
                    // allowed units within the row.
                    if (centerUnits &&
                        row != 0 &&
                        firstIndexInRow + columnsInRow > unitCount)
                    {
                        // Alter the offset to center the units that do not fill the row
                        var emptySlots = firstIndexInRow + columnsInRow - unitCount;
                        x += emptySlots / 2f * unitSpacing;
                    }

                    unitPositions.Add(new Vector3(x, 0, -z));

                    if (unitPositions.Count >= unitCount) break;
                }

                currentRowOffset -= unitSpacing / 2;
            }

            if (pivotInCenter)
                UnitFormationHelper.ApplyFormationCentering(unitPositions, row, unitSpacing);

            return unitPositions;
        }

    }

}
