using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TRavljen.UnitSystem.Demo
{
    using Combat;

    /// <summary>
    /// Handles unit interactions for the demo, allowing selected units to move or 
    /// interact with targets using mouse input and raycasting. This script is designed 
    /// to showcase integration of interaction systems with the UnitSystem package.
    /// </summary>
    /// <remarks>
    /// This component uses mouse clicks to command units. It determines whether to 
    /// move units to a formation, interact with entities, or move to a ground position 
    /// based on the clicked area. This implementation is suitable for demos and 
    /// can be replaced with custom logic for full games.
    /// </remarks>
    public class TargetedUnitInteractions : MonoBehaviour
    {

        #region Serialized Fields

        [Tooltip("The key used to trigger interactions (default: Right Mouse Button).")]
        [SerializeField]
        private KeyCode interactionKey = KeyCode.Mouse1;

        [Tooltip("The player who owns the units to control.")]
        [SerializeField]
        private APlayer player;

        [Tooltip("The camera used to cast rays for determining interaction points.")]
        [SerializeField]
        private Camera playerCamera;

        [Tooltip("Maximum distance for raycasting to detect valid targets or ground positions.")]
        [SerializeField]
        private float maxRayDistance = 1000;

        [Tooltip("Layer mask used for detecting valid game objects for interaction.")]
        [SerializeField]
        private LayerMask selectionLayers = ~0;

        [Tooltip("Layer mask used to identify ground surfaces for movement interactions.")]
        [SerializeField]
        private LayerMask groundLayer = ~0;

        [Tooltip("The manager responsible for handling unit selection.")]
        [SerializeField]
        private EntitySelectionManager selectionManager;

        private readonly RaycastHit[] hits = new RaycastHit[10];

        #endregion

        #region Lifecycle

        protected virtual void Start()
        {
            if (playerCamera == null)
            {
                playerCamera = Camera.main;
            }
        }

        protected virtual void OnValidate()
        {
            if (player == null)
            {
                player = transform.GetComponentInParent<APlayer>();

                if (player == null)
                    transform.GetComponentInChildren<APlayer>();
            }

            if (selectionManager == null && player != null)
            {
                selectionManager = player.GetComponentInChildren<EntitySelectionManager>();
            }
        }

        protected virtual void Update()
        {
            if (Input.GetKeyUp(interactionKey))
            {
                HandleAction();
            }
        }

        #endregion

        #region Interaction Logic

        /// <summary>
        /// Handles the interaction click logic for commanding selected units.
        /// </summary>
        protected virtual void HandleAction()
        {
            List<IEntity> ownedEntities = GetSelectedEntities();

            // No entity to command
            if (ownedEntities.Count == 0) return;

            Ray ray = playerCamera.ScreenPointToRay(Input.mousePosition);
            List<IEntity> entities = PerformEntitiesRaycast(ray);
            Vector3 groundPosition = PerformGroundRaycast(ray);

            // Only units can move
            List<Unit> movingUnits = new List<Unit>();
            List<Unit> units = new List<Unit>();

            foreach (var entity in ownedEntities)
            {
                if (entity is Unit unit)
                {
                    units.Add(unit);
                    if (unit.Movement != null)
                        movingUnits.Add(unit);
                }
            }

            // Try formation when no hits for interactions
            if (entities.Count == 0 && MoveToFormation(movingUnits, groundPosition))
            {
                return;
            }

            foreach (var unit in units)
            {
                if (unit.TryScheduleTask(true, entities, out IEntity target))
                {
                    // Remove from moving units if present
                    movingUnits.Remove(unit);

                    // Update stance info for attack if present
                    IDefendPosition.UpdateStance(unit.UnitAttack, target.transform.position, Vector3.zero);
                }
                else
                    // Otherwise try ground interaction with base control
                    SetUnitTargetPosition(unit, groundPosition);
            }

            // Moving units left, were not assigned to do anything yet.
            if (!MoveToFormation(movingUnits, groundPosition))
            {
                // If formation fails use base position movement.
                foreach (var unit in movingUnits)
                    SetUnitTargetPosition(unit, groundPosition);
            }
        }

        #endregion

        /// <summary>
        /// Attempts to move units into a formation at the specified ground position.
        /// </summary>
        protected virtual bool MoveToFormation(List<Unit> units, Vector3 groundPosition)
        {
            // Not enough units for a formation.
            if (units.Count < 2) return false;

            // Calculate group direction of all units in formation
            Vector3 center = Vector3.zero;

            foreach (var unit in units)
                center += unit.transform.position;

            Vector3 targetDirection = center / units.Count - groundPosition;
            float yAngle = Mathf.Atan2(targetDirection.x, targetDirection.z) * Mathf.Rad2Deg;
            Vector3 angles = new(0, yAngle, 0);
            Vector3 pivot = groundPosition;

            float spacing = 4;
            List<Vector3> positions = LineFormation.GetPositions(units.Count, spacing);
            for (int index = 0; index < units.Count; index++)
            {
                // Rotate local formation position to the world position
                Vector3 point = positions[index] + groundPosition;
                point = Quaternion.Euler(angles) * (point - pivot) + pivot;

                SetUnitTargetPosition(units[index], point);
            }

            // No implementation for formation in default demo.
            return true;
        }

        /// <summary>
        /// Updates a unit's target position and adjusts its behavior based on its attack stance.
        /// </summary>
        /// <param name="unit">The unit to be updated.</param>
        /// <param name="groundPosition">The new target position for the unit on the ground.</param>
        /// <remarks>
        /// This method:
        /// <list type="bullet">
        /// <item>Checks the unit's attack stance and updates it accordingly using the appropriate interface (`IDefendPosition` or `IStandGround`).</item>
        /// <item>Removes all current tasks assigned to the unit before moving it to the new position.</item>
        /// <item>Sets the target position using the unit's movement or control interface.</item>
        /// </list>
        /// </remarks>
        public static void SetUnitTargetPosition(Unit unit, Vector3 groundPosition)
        {
            // Cancel all tasks before movement
            unit.RemoveAllTasks();

            // Set target position via interface.
            IControlEntity control = unit.Movement ?? unit.EntityControl;
            control?.SetControlPosition(groundPosition);
        }

        /// <summary>
        /// Retrieves the list of selected entities owned by the player.
        /// </summary>
        private List<IEntity> GetSelectedEntities()
        {
            List<IEntity> entities = new();

            // Check if any unowned unit is selected, then ignore the action itself
            foreach (var selected in selectionManager.SelectedEntities)
            {
                if (selected.Owner == player)
                {
                    entities.Add(selected);
                }
            }

            return entities;
        }

        /// <summary>
        /// Performs raycasting to detect ground for movement.
        /// </summary>
        private Vector3 PerformGroundRaycast(Ray ray)
        {
            if (Physics.Raycast(ray, out RaycastHit groundHit, maxRayDistance, groundLayer))
            {
                return groundHit.point;
            }
            else
            {
                Debug.Log("No ground found for interaction fallback/move destination.");
                // Default ground position is 20 meters in front of the camera. Does not
                // check for ground.
                return playerCamera.transform.position + playerCamera.transform.forward * 20;
            }
        }

        /// <summary>
        /// Performs raycasting to detect entities in the clicked area.
        /// </summary>
        private List<IEntity> PerformEntitiesRaycast(Ray ray)
        {
            int count = Physics.RaycastNonAlloc(ray, hits, maxRayDistance, selectionLayers);
            List<IEntity> entities = new();

            for (int index = 0; index < count; index++)
            {
                if (hits[index].transform.TryGetComponent(out IEntity entity))
                {
                    entities.Add(entity);
                }
            }

            return entities;
        }

        #region Default Line Formation

        public struct LineFormation
        {
            public static List<Vector3> GetPositions(int unitCount, float unitSpacing)
            {
                List<Vector3> unitPositions = new();

                float offset = (unitCount - 1) * unitSpacing / 2f;
                for (int index = 0; index < unitCount; index++)
                {
                    unitPositions.Add(new Vector3(index * unitSpacing - offset, 0, 0));
                }

                return unitPositions;
            }
        }

        #endregion

    }

}