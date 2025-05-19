using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

namespace TRavljen.UnitFormation
{
    public static class UnitFormationHelper
    {

        /// <summary>
        /// Offset used for raycasts on ground. Takes original position moves it up
        /// and then shoots raycast down to find ground.
        /// </summary>
        private static readonly Vector3 raycastOffset = new Vector3(0, 100, 0);

        /// Finds the nearest hit from top down of the current position using
        /// Physics raycast.
        /// </summary>
        /// <param name="position">Current position</param>
        /// <param name="maxDistance">Max distance for check</param>
        /// <param name="groundPosition">Ground position</param>
        /// <param name="groundMask">LayerMask for ground detection</param>
        /// <returns>Returns true if ground position was found.</returns>
        public static bool TryMovePositionOnGround(Vector3 position, float maxDistance, out Vector3 groundPosition, LayerMask groundMask)
        {
            if (Physics.Raycast(
                    position + raycastOffset,
                    Vector3.down,
                    out RaycastHit hit,
                    maxDistance + raycastOffset.y,
                    groundMask,
                    QueryTriggerInteraction.Ignore))
            {
                groundPosition = hit.point;
                return true;
            }

            groundPosition = position;
            return false;
        }

        /// <summary>
        /// Finds the nearest valid NavMesh area for the position.
        /// </summary>
        /// <param name="position">Current position</param>
        /// <param name="maxDistance">Max distance for check</param>
        /// <param name="groundPosition">Ground position</param>
        /// <param name="areaMask">NavMesh area</param>
        /// <returns>Returns true if ground position was found</returns>
        public static bool TryMovePositionOnGround(
           Vector3 position, float maxDistance, out Vector3 groundPosition, int areaMask = NavMesh.AllAreas)
        {
            if (NavMesh.SamplePosition(position, out NavMeshHit hit, maxDistance, areaMask))
            {
                groundPosition = hit.position;
                return true;
            }

            groundPosition = position;
            return false;
        }

        /// <summary>
        /// Applies offset to the Z axes on positions in order to move positions
        /// from pivot in front of formation, to pivot in center of the formation.
        /// </summary>
        /// <param name="positions">Current positions, method will update the reference values.</param>
        /// <param name="rowCount">Row count produced with formation.</param>
        /// <param name="rowSpacing">Spacing between each row.</param>
        public static void ApplyFormationCentering(List<Vector3> positions, float rowCount, float rowSpacing)
        {
            float offsetZ = Mathf.Max(0, (rowCount - 1) * rowSpacing / 2);

            for (int i = 0; i < positions.Count; i++)
            {
                var pos = positions[i];
                pos.z += offsetZ;
                positions[i] = pos;
            }
        }

        /// <summary>
        /// Generates random "noise" for the position. In reality takes random
        /// range in the offset, does not use actual Math noise methods.
        /// </summary>
        /// <param name="factor">Factor for which the position can be offset.</param>
        /// <returns>Returns local offset for axes X and Z.</returns>
        public static Vector3 GetNoise(float factor)
        {
            return new Vector3(Random.Range(-factor, factor), 0, Random.Range(-factor, factor));
        }
    }
}