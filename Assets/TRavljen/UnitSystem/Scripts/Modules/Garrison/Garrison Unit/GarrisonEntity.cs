using System.Collections.Generic;
using UnityEngine;

namespace TRavljen.UnitSystem.Garrison
{
    using Interactions;

    /// <summary>
    /// Garrison component designed for units that can accommodate other units within them.
    /// This component manages the behavior, events, and capacity for units entering and exiting the garrison.
    /// Requires the <see cref="AEntitySO.capabilities"/> to include 
    /// <see cref="GarrisonUnitsCapability"/> or a custom <see cref="IGarrisonUnitsCapability"/> implementation.
    /// </summary>
    [DisallowMultipleComponent]
    public class GarrisonEntity : AEntityComponent, IGarrisonEntity, IUnitInteractingPosition
    {

        #region Properties

        [Tooltip("Specifies if garrisoned units are released when the garrison is disabled or destroyed.\n" +
            "If false, garrisoned units will be destroyed along with the garrison.")]
        [SerializeField]
        private bool releaseUnitsWhenDisabledOrDestroyed = true;

        [SerializeField, Tooltip("Specifies the entrance point of the garrison where units should approach to enter.")]
        private Bounds entrance = new Bounds(Vector3.zero, Vector3.one);
        
        [Tooltip("List of units currently inside the garrison.")]
        private List<IGarrisonableUnit> garrisonedUnits = new();

        private IGarrisonUnitsCapability data;
        
        private Vector3 WorldEntrancePoint => transform.TransformPoint(entrance.center);

        /// <summary>
        /// Read-only list of units currently in the garrison.
        /// </summary>
        public List<IGarrisonableUnit> GarrisonedUnits => garrisonedUnits;

        /// <summary>
        /// The position of the garrison in world space.
        /// </summary>
        public Vector3 Position => transform.position;

        /// <summary>
        /// Specifies whether the garrison is active and operational.
        /// </summary>
        public override bool IsActive => Entity.IsOperational;

        /// <summary>
        /// Capacity of the garrison, as defined by its capability.
        /// </summary>
        public int GarrisonCapacity => data.Capacity;

#if UNITY_EDITOR
        [SerializeField, Tooltip("Color of the gizmo for the entrance point in the editor.")]
        private Color entrancePointGizmoColor = new Color(0, 0, 1, 0.5f);
#endif

        #endregion

        #region Lifecycle

        /// <summary>
        /// Initializes the garrison and ensures that the necessary capability data is present.
        /// </summary>
        protected override void OnInitialize()
        {
            base.OnInitialize();

            if (!Entity.TryGetCapability(out data))
            {
                Debug.LogError("Cannot use Garrison component without specifying " +
                    "IGarrisonUnitsCapability in the Unit's capabilities: " + Entity.transform.name);
            }
        }

        private void OnDisable()
        {
            // Units should already be disabled when in garrison,
            // they will either be destroyed in the OnDestroy or
            // released here.

            if (releaseUnitsWhenDisabledOrDestroyed)
                // Release all units when object is disabled or destroyed.
                RemoveAllUnits();
        }

        private void OnDestroy()
        {
            if (releaseUnitsWhenDisabledOrDestroyed)
            {
                // Release in case they were somehow added between disabled
                // and destroy; unexpected behaviour.
                RemoveAllUnits();
            }
            else
            {
                // Destroy units along with the garrison.
                foreach (var unit in garrisonedUnits)
                {
                    if (unit.IsNotNull() && unit.Entity.IsNotNull())
                        // Destroy units on disable
                        unit.Entity.DestroyEntity();
                }
            }
        }

        #endregion

        #region IUnitInteractingPosition

        public Vector3 GetAvailableInteractionPosition(IUnitInteractorComponent _, bool reserve) => WorldEntrancePoint;
        public virtual bool ReleaseInteractionPosition(IUnitInteractorComponent interactor) => true;

        #endregion

        #region Manage Units

        public virtual bool IsEligibleToEnter(IGarrisonableUnit garrisonable)
        {
            // Check if unit is friendly and eligible
            return garrisonable.Unit.IsAlly(Entity) && data.IsEligibleToEnter(garrisonable.Unit.Data);
        }
        
        public virtual bool IsInRangeToEnter(IGarrisonableUnit unit)
        {
            return new Bounds(WorldEntrancePoint, entrance.size)
                .Contains(unit.Position);
        }

        /// <summary>
        /// Adds a unit to the garrison if there is available capacity.
        /// </summary>
        /// <param name="unit">The unit to add.</param>
        /// <returns>True if the unit was successfully added, false otherwise.</returns>
        public virtual bool AddUnit(IGarrisonableUnit unit)
        {
            // Add unit to the list only if capacity has not been reached.
            if (garrisonedUnits.Count < GarrisonCapacity)
            {
                garrisonedUnits.Add(unit);
                unit?.EnterGarrison();
                GarrisonEvents.Instance.OnUnitEnterGarrison.Invoke(this, unit);
                return true;
            }

            return false;
        }

        /// <summary>
        /// Removes a unit from the garrison.
        /// </summary>
        /// <param name="unit">The unit to remove.</param>
        /// <returns>True if the unit was successfully removed, false otherwise.</returns>
        public virtual bool RemoveUnit(IGarrisonableUnit unit)
        {
            // Nothing to remove if the list does not contain the unit.
            if (!garrisonedUnits.Contains(unit)) return false;

            garrisonedUnits.Remove(unit);
            
            GarrisonEvents.Instance.OnUnitExitGarrison.Invoke(this, unit);

            unit?.ExitGarrison();

            if (unit.IsNotNull())
                Entity.UnitSpawn?.RespawnUnit(unit?.Unit, true);

            return true;
        }

        /// <summary>
        /// Removes all units currently in the garrison.
        /// </summary>
        public void RemoveAllUnits()
        {
            for (var index = garrisonedUnits.Count-1; index >= 0; index--)
            {
                var unit = garrisonedUnits[index];
                if (unit.IsNotNull())
                    RemoveUnit(unit);
            }

            garrisonedUnits.Clear();
        }

        #endregion

#if UNITY_EDITOR
        private void OnDrawGizmosSelected()
        {
            Gizmos.color = entrancePointGizmoColor;
            Gizmos.DrawCube(entrance.center, entrance.size);
        }
#endif
    }

}