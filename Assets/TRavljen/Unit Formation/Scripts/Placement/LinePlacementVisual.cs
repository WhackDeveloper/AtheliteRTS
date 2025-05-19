using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

namespace TRavljen.UnitFormation.Placement
{

    /// <summary>
    /// Component for showing formation center and direction during active placement.
    /// This can be used with <see cref="FormationPlacement.placementVisuals"/>.
    /// It uses <see cref="LineRenderer"/> for drawing the direction.
    /// If another technique is desired, use <see cref="APlacementVisuals"/>
    /// to implement your own.
    /// </summary>
    [RequireComponent(typeof(LineRenderer))]
    public class LinePlacementVisual : APlacementVisuals
    {

        private LineRenderer lineRenderer;

        private void Awake()
        {
            lineRenderer = GetComponent<LineRenderer>();
            lineRenderer.enabled = false;
        }

        public override void StartPlacement(Vector3 start)
        {
            lineRenderer.SetPosition(0, start);
            lineRenderer.SetPosition(1, start);
            lineRenderer.enabled = true;
        }

        public override void ContinuePlacement(Vector3 end)
        {
            lineRenderer.SetPosition(1, end);
        }

        public override void StopPlacement()
        {
            lineRenderer.enabled = false;
        }

    }

}