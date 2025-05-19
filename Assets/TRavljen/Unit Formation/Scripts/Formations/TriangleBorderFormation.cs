using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TRavljen.UnitFormation.Formations
{

    /// <summary>
    /// Formation that positions units along the borders of a triangle.
    /// Since there are 3 sides to a triangle and units cannot always
    /// fit equally, leftovers are filled in the back row, while left
    /// and right side are always equal in unit count.
    /// </summary>
    [System.Serializable]
    public struct TriangleBorderFormation : IFormation
    {
        [SerializeField, Range(0.1f, 100f)] private float unitSpacing;

        [Tooltip("Specifies if center is used for rotation pivot.")]
        [SerializeField] private bool pivotInCenter;

        public TriangleBorderFormation(float unitSpacing, bool pivotInCenter = true)
        {
            this.unitSpacing = unitSpacing;
            this.pivotInCenter = pivotInCenter;
    }

        public List<Vector3> GetPositions(int unitCount)
        {
            // Validate count
            if (unitCount <= 2)
            {
                return new LineFormation(unitSpacing).GetPositions(unitCount);
            }

            float sideLength = Mathf.Max(1, unitSpacing * unitCount / 3);
            List<Vector3> positions = new List<Vector3>();
            float perSide = unitCount / 3;

            // Each vertex of the triangle
            Vector3 a = new Vector3(-sideLength / 2, 0, -Mathf.Sqrt(3) * sideLength / 6);
            Vector3 b = new Vector3(sideLength / 2, 0, -Mathf.Sqrt(3) * sideLength / 6);
            Vector3 c = new Vector3(0, 0, Mathf.Sqrt(3) * sideLength / 3);

            int leftOvers = unitCount % 3;

            float offset = pivotInCenter ? 0 : -c.z;
            void AddPosition(Vector3 pos)
            {
                pos.z += offset;
                positions.Add(pos);
            }

            // If there are 2 leftovers give them to sides for equal split,
            // otherwise spacing on single side gets too small.
            if (leftOvers == 2)
            {
                float morePerSide = perSide + leftOvers / 2;

                // Sides: BC, CA + leftovers divided
                for (int i = 0; i < morePerSide; i++)
                {
                    AddPosition(Vector3.Lerp(b, c, i / morePerSide));
                    AddPosition(Vector3.Lerp(c, a, i / morePerSide));
                }

                // Side: AB
                for (int i = 0; i < perSide; i++)
                {
                    AddPosition(Vector3.Lerp(a, b, i / perSide));
                }
            }
            // If there is one leftover unit put it on the back side
            else
            {
                float morePerSide = perSide + leftOvers;

                // Sides: BC, CA
                for (int i = 0; i < perSide; i++)
                {
                    AddPosition(Vector3.Lerp(b, c, i / perSide));
                    AddPosition(Vector3.Lerp(c, a, i / perSide));
                }

                // Side: AB with leftovers
                for (int i = 0; i < morePerSide; i++)
                {
                    AddPosition(Vector3.Lerp(a, b, i / morePerSide));
                }
            }

            return positions;
        }
    }

}