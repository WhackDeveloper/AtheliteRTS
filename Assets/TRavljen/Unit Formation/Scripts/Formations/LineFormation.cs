using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TRavljen.UnitFormation.Formations
{

    /// <summary>
    /// Formation that positions units in a straight line 
    /// with specified spacing.
    /// </summary>
    [System.Serializable]
    public struct LineFormation : IFormation
    {

        [SerializeField, Range(0.1f, 100f)] private float unitSpacing;

        /// <summary>
        /// Instantiates line formation.
        /// </summary>
        /// <param name="unitSpacing">Specifies spacing between units.</param>
        public LineFormation(float unitSpacing)
        {
            this.unitSpacing = unitSpacing;
        }

        public List<Vector3> GetPositions(int unitCount)
        {
            List<Vector3> unitPositions = new List<Vector3>();

            float offset = (unitCount-1) * unitSpacing / 2f;
            for (int index = 0; index < unitCount; index++)
            {
                unitPositions.Add(new Vector3(index * unitSpacing - offset, 0, 0));
            }

            return unitPositions;
        }
    }

}
