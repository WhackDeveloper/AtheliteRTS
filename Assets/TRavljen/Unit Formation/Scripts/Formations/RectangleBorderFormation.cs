using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TRavljen.UnitFormation.Formations
{

    /// <summary>
    /// Formation positions along the edges of a rectangle. When there are less
    /// than 4 units to position, straight line is formed. If units cannot be
    /// split equally amongs the edges, leftovers are placed in the front line.
    /// </summary>
    [System.Serializable]
    public struct RectangleBorderFormation : IFormation
    {
        [SerializeField, Range(0.1f, 100f)] private float unitSpacing;

        [Tooltip("Aspect ratio of the rectangle")]
        [SerializeField, Range(0.2f, 5f)] private float aspectRatio;

        [Tooltip("Specifies if middle is used for rotation pivot.")]
        [SerializeField] private bool pivotInCenter;

        public RectangleBorderFormation(float unitSpacing, float aspectRatio = 2.0f, bool pivotInCenter = true)
        {
            this.unitSpacing = unitSpacing;
            this.aspectRatio = aspectRatio;
            this.pivotInCenter = pivotInCenter;
        }

        public List<Vector3> GetPositions(int unitCount)
        {
            // Validate count
            if (unitCount < 4)
            {
                return new LineFormation(unitSpacing).GetPositions(unitCount);
            }

            // Calculate the total perimeter based on the unit count and spacing
            float totalPerimeter = unitCount * unitSpacing;

            // Calculate dimensions based on the aspect ratio
            float height = totalPerimeter / (2 * (aspectRatio + 1));
            float width = aspectRatio * height;

            List<Vector3> positions = new List<Vector3>();
            int unitsPerWidthSide = Mathf.RoundToInt(width / unitSpacing);
            int unitsPerHeightSide = Mathf.RoundToInt(height / unitSpacing);

            int leftOvers = unitCount - unitsPerWidthSide * 2 - unitsPerHeightSide * 2;

            // Define vertices of the rectangle
            Vector3 a = new Vector3(-width / 2, 0, -height / 2);
            Vector3 b = new Vector3(width / 2, 0, -height / 2);
            Vector3 c = new Vector3(width / 2, 0, height / 2);
            Vector3 d = new Vector3(-width / 2, 0, height / 2);

            float pivotOffset = 0;
            if (!pivotInCenter)
            {
                pivotOffset = height / -2f;
            }

            void AddPosition(Vector3 pos)
            {
                pos.z += pivotOffset;
                positions.Add(pos);
            }

            // Distribute units along each side
            for (int i = 0; i <= unitsPerWidthSide; i++)
            {
                AddPosition(Vector3.Lerp(a, b, (float)i / unitsPerWidthSide)); // AB side
            }

            for (int i = 1; i < unitsPerHeightSide; i++)
            {
                AddPosition(Vector3.Lerp(b, c, (float)i / unitsPerHeightSide)); // BC side
                if (i < unitsPerHeightSide)
                    AddPosition(Vector3.Lerp(d, a, (float)i / unitsPerHeightSide)); // DA side
            }

            // Add leftovers to CD side
            float side = unitsPerWidthSide + leftOvers;
            for (int i = 0; i <= side; i++)
            {
                AddPosition(Vector3.Lerp(c, d, i / side));
            }

            return positions;
        }
    }

}