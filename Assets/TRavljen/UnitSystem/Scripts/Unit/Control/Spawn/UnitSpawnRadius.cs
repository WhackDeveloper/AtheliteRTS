using UnityEngine;

namespace TRavljen.UnitSystem
{

    /// <summary>
    /// A spawn point implementation that spawns units within a defined radius from the base spawn point.
    /// </summary>
    public class UnitSpawnRadius : UnitSpawnPoint
    {

        #region Properties

        [Tooltip("Minimum radius from the spawn point for unit placement.")]
        [SerializeField]
        private float minSpawnRadius = 2f;

        [Tooltip("Maximum radius from the spawn point for unit placement.")]
        [SerializeField]
        private float maxSpawnRadius = 10f;

        [Tooltip("Minimum radius from the spawn point for post-spawn movement.")]
        [SerializeField]
        private float minMovePositionRadius = 2f;

        [Tooltip("Maximum radius from the spawn point for post-spawn movement.")]
        [SerializeField]
        private float maxMovePositionRadius = 15f;

        [Tooltip("Whether to visualize the spawn radius gizmos in the editor.")]
        [SerializeField]
        private bool spawnRadiusGizmoEnabled = true;

        [Tooltip("The color used to visualize the spawn radius gizmos.")]
        [SerializeField]
        private Color spawnRadiusGizmoColor = Color.blue;

        #endregion

        #region Override

        /// <summary>
        /// Calculates a random position within the spawn radius for unit placement.
        /// </summary>
        /// <returns>A <see cref="Vector3"/> representing the spawn position.</returns>
        protected override Vector3 GetUnitSpawnPosition()
        {
            float range = Random.Range(minSpawnRadius, maxSpawnRadius);
            // Apply random randius offset to centre spawn position
            return base.GetUnitSpawnPosition() + GetRandomPoint(range);
        }

        /// <summary>
        /// Moves the unit to a random position within the movement radius after it has spawned.
        /// </summary>
        /// <param name="unit">The unit to move.</param>
        protected override void MoveUnitToTargetPosition(Unit unit)
        {
            // Ignores position check with Vector3, since it uses radius position
            // is calculated and not a single point.
            float range = Random.Range(minMovePositionRadius, maxMovePositionRadius);
            unit.Movement.SetControlPosition(spawnedUnitTargetPosition + GetRandomPoint(range));
        }

        /// <summary>
        /// Visualizes the spawn and movement radii in the Unity editor.
        /// </summary>
        protected override void OnDrawGizmosSelected()
        {
            // Draw spawn radius
            if (spawnRadiusGizmoEnabled)
            {
                // Get base spawn position, without additions of the radius logic.
                Vector3 spawnPos = base.GetUnitSpawnPosition();
                Gizmos.color = spawnRadiusGizmoColor;
                Gizmos.DrawWireSphere(spawnPos, minSpawnRadius);
                Gizmos.DrawWireSphere(spawnPos, maxSpawnRadius);
            }

            if (targetGizmoEnabled)
            {
                Gizmos.color = targetPositionGizmoColor;
                Gizmos.DrawWireSphere(spawnedUnitTargetPosition, minMovePositionRadius);
                Gizmos.DrawWireSphere(spawnedUnitTargetPosition, maxMovePositionRadius);
            }
        }

        #endregion

        #region Utility

        /// <summary>
        /// Generates a random point within a circle of a given radius on the XZ plane.
        /// </summary>
        /// <param name="radius">The radius of the circle.</param>
        /// <returns>A <see cref="Vector3"/> representing a random point within the circle.</returns>
        public static Vector3 GetRandomPoint(float radius)
        {
            // Generate a random angle between 0 and 2π
            var theta = Random.Range(0f, 2 * Mathf.PI);
            // Generate a random radius, using square root to ensure uniform distribution
            var r = radius * Mathf.Sqrt(Random.Range(0f, 1f));
            // Convert polar coordinates to Cartesian coordinates
            var x = r * Mathf.Cos(theta);
            var z = r * Mathf.Sin(theta);
            return new Vector3(x, 0f, z);
        }

        #endregion

    }

}