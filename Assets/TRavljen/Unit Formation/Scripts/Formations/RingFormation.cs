using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TRavljen.UnitFormation.Formations
{

    /// <summary>
    /// Formation that positions units in a ring with specified angle and spacing between units.
    /// </summary>
    [System.Serializable]
    public struct RingFormation : IFormation
    {

        [SerializeField, Range(0.1f, 100f)] private float unitSpacing;
        [SerializeField, Range(180, 360)] private float circleAngle;

        /// <summary>
        /// Instantiates circle formation.
        /// </summary>
        /// <param name="unitSpacing">Specifies spacing between units in cricle</param>
        /// <param name="circleAngle">Specifies angle for units to be placed,
        /// 360 degree means that the units will go entire path around the circle
        /// and 180 degree angle means that only half of the circle will be formed.</param>
        public RingFormation(float unitSpacing, float circleAngle = 360f)
        {
            this.unitSpacing = unitSpacing;
            this.circleAngle = Mathf.Clamp(circleAngle, 180f, 360f);
        }

        public List<Vector3> GetPositions(int unitCount)
        {
            // If there aren't enough points to start with the circle,
            // return the list with zero vector so that position is the target.
            if (unitCount <= 1)
            {
                return new List<Vector3>() { Vector3.zero };
            }

            List<Vector3> unitPositions = new List<Vector3>();
            float x, y;
            float angle = 0f;

            var angleIncrement = circleAngle / unitCount;
            var a = angleIncrement / 2;
            var radius = (unitSpacing / 2) / Mathf.Sin(a * Mathf.Deg2Rad);

            for (int i = 0; i < unitCount; i++)
            {
                x = Mathf.Cos(Mathf.Deg2Rad * angle) * radius;
                y = Mathf.Sin(Mathf.Deg2Rad * angle) * radius;

                unitPositions.Add(new Vector3(x, 0, y));

                angle += angleIncrement;
            }

            return unitPositions;
        }

    }
}