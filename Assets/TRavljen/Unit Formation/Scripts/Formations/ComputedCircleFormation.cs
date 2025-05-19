using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TRavljen.UnitFormation.Formations
{

    /// <summary>
    /// Formation that positions units within a circle with fill.
    /// Works similar to <see cref="CircleFormation"/>, but this
    /// formation attempts to get the best possible option of a
    /// filled circle for specified unit count. For this in most
    /// cases it will need to do iterations to find the best fit,
    /// there for you control maximum iterations with <see cref="maxIterations"/>.
    /// </summary>
    [System.Serializable]
    public struct ComputedCircleFormation : IFormation
    {
        [SerializeField, Range(0.1f, 100f)]
        private float unitSpacing;

        [SerializeField, Range(1, 100)]
        [Tooltip("Min radius at which to start iterations to find a perfect circle match.")]
        private float minRadius;

        [Tooltip("Maximal allowed iterations to find a perfect match for a circle.")]
        [SerializeField, Range(1, 10_000)]
        private int maxIterations;

        [Tooltip("Radius increment for each iteration. If this value is too big, " +
            "it may skip the potentially perfect circle formation and not complete.")]
        [SerializeField, Range(0.01f, 10f)]
        private float radiusIncrement;

        public ComputedCircleFormation(float unitSpacing)
        {
            this.unitSpacing = unitSpacing;
            minRadius = 1;
            maxIterations = 100;
            radiusIncrement = 0.15f;
        }

        public List<Vector3> GetPositions(int unitCount)
        {
            float radius = Mathf.Max(1, minRadius);
            List<Vector3> result;

            int iterations = 0;
            while (!GetPositionIteration(radius, unitCount, out result))
            {
                radius += radiusIncrement;

                iterations++;
                if (iterations >= maxIterations)
                {
                    Debug.LogWarning("No result after max iterations. Tried using radius: " + radius);
                    return result;
                }
            }

            Debug.Log("Result found after " + iterations + " iterations. Succeded using radius:" + radius);

            return result;
        }

        public bool GetPositionIteration(float radius, int unitCount, out List<Vector3> result)
        {
            List<Vector3> positions = new List<Vector3>();
            int remainingUnits = unitCount;

            // Start from the outside and work your way in
            float currentRadius = radius;
            while (remainingUnits > 0 && currentRadius > 0)
            {
                // Radius should not be smaller than minimal spacing.
                if (currentRadius < unitSpacing && remainingUnits > 3)
                    break;

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

            bool remainder = remainingUnits > 0;

            Debug.Log("Remainder: " + remainder + ", Leftover units: " + remainingUnits);

            while (remainingUnits > 0)
            {
                // Place any leftovers in the center when iteration is invalid
                positions.Add(new Vector3(0, 0, 0));
                remainingUnits--;
            }

            result = positions;
            return !remainder;
        }

    }

}