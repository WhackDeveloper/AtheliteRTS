using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TRavljen.UnitFormation
{

    /// <summary>
    /// Interface used for communicating formation positions to units and their
    /// facing angles. There is also default implementation available which uses
    /// Unity's NavMesh system, <see cref="FormationUnit"/>.
    /// </summary>
    public interface IFormationUnit
    {
        public bool IsWithinStoppingDistance { get; }

        public void SetTargetDestination(Vector3 newTargetDestination, float newFacingAngle);
    }

}