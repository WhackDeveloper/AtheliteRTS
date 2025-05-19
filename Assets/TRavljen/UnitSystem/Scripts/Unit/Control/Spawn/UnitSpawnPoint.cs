using UnityEngine;

namespace TRavljen.UnitSystem
{

    /// <summary>
    /// A MonoBehaviour that implements the <see cref="IUnitSpawn"/> interface,
    /// responsible for spawning and respawning units at a designated position
    /// and optionally moving them to a specified target location.
    /// </summary>
    public class UnitSpawnPoint : MonoBehaviour, IUnitSpawn
    {

        #region Properties

        // This is world position duo to units ability to move and spawn point with it.
        [SerializeField, Tooltip("Set default spawn position (world position). " +
            "If this is set to zero, it will not move units on spawn as it presumes " +
            "there is no position set.")]
        protected Vector3 spawnedUnitTargetPosition = Vector3.zero;

        [Tooltip("Specifies the local spawn position relative to this GameObject's transform.")]
        [SerializeField]
        protected Vector3 spawnPosition = Vector3.zero;

        [Tooltip("Indicates whether a gizmo should be draw to visualize the " +
            "spawnd unit target position in the Scene View.")]
        [SerializeField]
        protected bool targetGizmoEnabled = true;

        [Tooltip("Specifies the color of the gizmo drawn at the target position.")]
        [SerializeField]
        protected Color targetPositionGizmoColor = Color.red;

        #endregion

        #region Lifecycle

        /// <summary>
        /// Draws a gizmo in the Scene view to indicate the target position for spawned units
        /// when the object is selected in the editor.
        /// </summary>
        protected virtual void OnDrawGizmosSelected()
        {
            if (targetGizmoEnabled)
            {
                Gizmos.color = targetPositionGizmoColor;
                var size = new Vector3(.1f, 1.2f, .1f);
                Gizmos.DrawCube(spawnedUnitTargetPosition + size / 2, size);

                Gizmos.DrawSphere(transform.position + transform.rotation * spawnPosition, 0.25f);
            }
        }

        #endregion

        #region IUnitSpawn

        public Vector3 SpawnPoint => GetUnitSpawnPosition();

        /// <inheritdoc/>
        public void SpawnUnit(AUnitSO unitData, bool moveToTarget, System.Action<Unit> unitSpawned)
        {
            unitData.LoadUnitPrefab(prefab =>
            {
                Unit unit = SpawnUnit(prefab, moveToTarget);
                unitSpawned.Invoke(unit);
            });
        }

        /// <inheritdoc/>
        public Unit SpawnUnit(Unit prefab, bool moveToTarget)
        {
            var newUnit = Instantiate(prefab, GetUnitSpawnPosition(), Quaternion.identity);

            if (moveToTarget)
            {
                MoveUnitToTargetPosition(newUnit);
            }

            return newUnit;
        }

        /// <inheritdoc/>
        public void RespawnUnit(Unit sceneUnit, bool moveToTarget)
        {
            sceneUnit.transform.position = GetUnitSpawnPosition();
            sceneUnit.gameObject.SetActive(true);

            if (moveToTarget)
            {
                MoveUnitToTargetPosition(sceneUnit);
            }
        }

        /// <inheritdoc/>
        public void SetControlPosition(Vector3 target)
        {
            spawnedUnitTargetPosition = target;
        }

        /// <summary>
        /// Returns the target position of the spawn point.
        /// </summary>
        public Vector3 GetControlPosition()
        {
            return spawnedUnitTargetPosition != Vector3.zero ? spawnedUnitTargetPosition : GetUnitSpawnPosition();
        }


        #endregion

        #region Virtual

        /// <summary>
        /// Moves the specified unit to the <see cref="spawnedUnitTargetPosition"/> by utilizing its movement system.
        /// </summary>
        /// <param name="unit">The unit to be moved.</param>
        protected virtual void MoveUnitToTargetPosition(Unit unit)
        {
            if (spawnedUnitTargetPosition != Vector3.zero)
            {
                // Use control interface to specify units destination - similar to player's command.
                unit.Movement?.SetControlPosition(spawnedUnitTargetPosition);
            }
        }

        /// <summary>
        /// The world-space position where units are spawned. 
        /// </summary>
        protected virtual Vector3 GetUnitSpawnPosition() => transform.TransformPoint(spawnPosition);

        #endregion
    }

}