using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TRavljen.UnitFormation.Placement
{

    /// <summary>
    /// Abstract component for showing visuals when placing a formation
    /// with <see cref="FormationPlacement"/>. This can be extended to support
    /// any type of visuals, you can check out <see cref="LinePlacementVisual"/> and
    /// <see cref="FormationIndicatorVisual"/> implementations for examples.
    /// </summary>
    public abstract class APlacementVisuals : MonoBehaviour
    {

        /// <summary>
        /// Invoked at the start of placement with the initial position.
        /// </summary>
        /// <param name="start">World position of placement start</param>
        public virtual void StartPlacement(Vector3 start) { }

        /// <summary>
        /// Invoked when placement is active and new end position was calculated.
        /// </summary>
        /// <param name="newPosition">New position of active placement</param>
        public virtual void ContinuePlacement(Vector3 newPosition) { }

        /// <summary>
        /// Invoked when placement has stopped.
        /// </summary>
        public virtual void StopPlacement() { }

        /// <summary>
        /// Invoked when formation is calculated for visuals during placement.
        /// To enable this <see cref="FormationPlacement.alwaysCalculatePositions"/>
        /// must be enabled. This can be disabled to improve performance when
        /// it is not required to calculate formation during placement.
        /// </summary>
        /// <param name="formation">New unit formation data</param>
        public virtual void OnFormationReady(UnitFormationData formation) { }

    }
}
