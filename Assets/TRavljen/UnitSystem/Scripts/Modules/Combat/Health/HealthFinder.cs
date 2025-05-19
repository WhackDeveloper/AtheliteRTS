using UnityEngine;

namespace TRavljen.UnitSystem.Combat
{
    
    using Utility;

    /// <summary>
    /// Provides utilities for scanning health components in the game world,
    /// with methods for finding nearby and closest health components.
    /// This is a default implementation and can be simply replaced by
    /// implementing <see cref="IHealthFinder"/>.
    /// </summary>
    [System.Serializable]
    public class HealthFinder: IHealthFinder
    {
        
        [SerializeField]
        private LayerMask layerMask = ~0;
        
        [SerializeReference]
        private NearbyColliderFinder nearbyColliderFinder;

        public HealthFinder(LayerMask layerMask, bool useNonAllocating = false, int allocationSize = 100)
        {
            this.layerMask = layerMask;
            nearbyColliderFinder = new NearbyColliderFinder(layerMask, useNonAllocating, allocationSize);
        }

        public IHealth[] FindNearbyHealth(IUnitAttack attacker, float range)
        {
            return FindNearbyHealth(attacker, attacker.Position, range);
        }

        public IHealth[] FindNearbyHealth(IUnitAttack attacker, Vector3 position, float range)
        {
            var overlapDistance = Mathf.Min(attacker.LineOfSight, range);
            var validateTypes = attacker.InvalidTargetTypes.Length > 0;

            return nearbyColliderFinder.FindNearby<IHealth>(position, attacker.Position, overlapDistance, 
                (health) => ValidateHealth(attacker, health, validateTypes));
        }

        public bool FindNearestHealth(IUnitAttack attacker, Vector3 position, float range, out IHealth closestHealth)
        {
            var overlapDistance = Mathf.Min(attacker.LineOfSight, range);
            var validateTypes = attacker.InvalidTargetTypes.Length > 0;

            return nearbyColliderFinder.FindClosest(position, attacker.Position, overlapDistance, out closestHealth, 
                (health) => ValidateHealth(attacker, health, validateTypes));
        }

        private static bool ValidateHealth(IUnitAttack attacker, IHealth health, bool validateTypes)
        {
            if (health.transform.gameObject == attacker.transform.gameObject ||
                health.IsDepleted ||
                // Check if its limiting not interactions or if limit has not been reached.
                (health is Interactions.ILimitedUnitInteractionTarget limits && limits.HasReachedLimit())) 
                return false;
            
            if (health.Entity == null || health.Entity.IsAlly(attacker.Entity))
                return false;
            
            // Check if type of the unit should be ignored.
            return !validateTypes ||
                   health.Entity.Data is not AUnitSO unitData ||
                   !unitData.DoesMatchAnyType(attacker.InvalidTargetTypes);
        }

    }
}