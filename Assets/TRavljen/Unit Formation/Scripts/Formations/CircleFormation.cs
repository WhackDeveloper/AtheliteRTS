using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TRavljen.UnitFormation.Formations
{
  
    /// <summary>
    /// Formation that positions units within a circle with fill.
    /// This formation must be adjusted manually in parameters for each
    /// unit count specifically as it is completely controlled by the
    /// <see cref="outerRadius"/> and <see cref="unitSpacing"/>. These
    /// two parameters are in complete control how units are positioned.
    /// </summary>
    [System.Serializable]
    public struct CircleFormation : IFormation
    {
        [Tooltip("Maximal radius allowed for the circle formation")]
        [SerializeField]
        private float outerRadius;

        [SerializeField]
        private float unitSpacing;

        public CircleFormation(float outerRadius, float unitSpacing)
        {
            this.outerRadius = outerRadius;
            this.unitSpacing = unitSpacing;
        }

        public List<Vector3> GetPositions(int unitCount)
        {
            List<Vector3> positions = new List<Vector3>();
            int remainingUnits = unitCount;

            // Start from the outside and work your way in
            float currentRadius = outerRadius;
            while (remainingUnits > 0 && currentRadius > 0)
            {
                // Calculate number of units that fit in the current ring
                float circumference = 2 * Mathf.PI * currentRadius;
                int unitsInThisRing = Mathf.Min(remainingUnits, Mathf.FloorToInt(circumference / unitSpacing));

                if (unitsInThisRing == 0 && remainingUnits > 0)  // Ensure there's always at least one unit in a ring
                    unitsInThisRing = remainingUnits;

                float angleStep = 360.0f / unitsInThisRing;

                for (int i = 0; i < unitsInThisRing; i++)
                {
                    float angle = i * angleStep * Mathf.Deg2Rad;
                    float x = Mathf.Cos(angle) * currentRadius;
                    float z = Mathf.Sin(angle) * currentRadius;
                    positions.Add(new Vector3(x, 0, z));
                }

                remainingUnits -= unitsInThisRing;
                currentRadius -= unitSpacing;  // Decrease radius for next inner ring
            }

            Debug.Log("Leftover units: " + remainingUnits);
            // Put leftovers in the middle...
            while (remainingUnits > 0)
            {
                positions.Add(new Vector3(0, 0, 0));
                remainingUnits--;
            }

            return positions;
        }
    }

}