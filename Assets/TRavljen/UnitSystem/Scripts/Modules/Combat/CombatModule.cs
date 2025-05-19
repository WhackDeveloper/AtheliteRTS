using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

namespace TRavljen.UnitSystem.Combat
{

    /// <summary>
    /// Represents a combat module that manages unit attacks and enemy interactions.
    /// </summary>
    [DisallowMultipleComponent]
    public class CombatModule : APlayerModule
    {

        #region Properties

        [Tooltip("Specifies the layer on which the enemies should be detected.")]
        [SerializeField]
        private LayerMask enemyLayerMask = ~0;

        /// <summary>
        /// Whether the attack timer should reset when the unit changes targets.
        /// </summary>
        [SerializeField]
        private bool resetAttackOnExit = false;

        /// <summary>
        /// Specifies if module should be checking for nearby enemies periodically.
        /// If this is done manually, simply set this flag to false.
        /// </summary>
        [Tooltip("Specifies if module should be checking for nearby enemies periodically." +
            "If this is done manually, simply set this flag to false.")]
        [SerializeField]
        private bool automaticallyCheckForNearbyEnemies = true;

        [Tooltip("Specifies the time between each search for nearby enemies for " +
            "combat units that are currently not attacking and have aggressive stance.")]
        [SerializeField]
        private float nearbyEnemySearchInterval = 0.5f;

        [SerializeField, HideInInspector]
        private int totalUnitLostCount = 0;

        [Tooltip("Optional health scanning component reference. If this is not " +
                 "set nor a script is present on player's game object which implements " +
                 "this interface, a default implementation will be instantiated (HealthFinder).")]
        [SerializeField, RequiresType(typeof(IHealthFinder))]
        private GameObject healthFinderObject;

        /// <summary>
        /// The health scanning system used by the combat module for identifying
        /// potential targets.
        /// </summary>
        /// <remarks>
        /// By default, this is an instance of <see cref="Combat.HealthFinder"/>.
        /// This can be replaced or injected with a custom implementation to
        /// provide additional customization and behavior.
        /// </remarks>
        public IHealthFinder HealthFinder;

        [SerializeReference, HideInInspector]
        public IAttackDistanceHandler AttackDistanceHandler = new UnitAttackDistanceHandler();

        [SerializeReference, HideInInspector]
        public IAttackTargetPositionGetter AttackTargetPositionGetter = new UnitAttackTargetPositionGetter();

        private readonly List<IUnitAttack> units = new();

        public int TotalUnitsLost => totalUnitLostCount;
        public bool ResetAttackOnExit => resetAttackOnExit;

        #endregion

        #region Lifecycle

        protected override void Awake()
        {
            base.Awake();

            if (HealthFinder == null)
            {
                if (healthFinderObject != null && healthFinderObject.TryGetComponent(out HealthFinder))
                {
                    // Done, found existing one
                }
                else if (!player.TryGetComponent(out HealthFinder))
                {
                    // Create default instance.
                    HealthFinder = new HealthFinder(enemyLayerMask, true, 100);
                }
            }

            foreach (var unit in player.GetUnits())
                UnitAdded(unit);
        }

        private void OnEnable()
        {
            player.OnUnitAdded.AddListener(UnitAdded);
            player.OnUnitRemoved.AddListener(UnitRemoved);

            if (automaticallyCheckForNearbyEnemies)
                player.StartCoroutine(CheckForNearbyEnemy());
        }

        private void OnDisable()
        {
            player.OnUnitAdded.RemoveListener(UnitAdded);
        }

        #endregion

        public Vector3 GetAttackPosition(IUnitAttack attack, IHealth target)
        {
            return AttackTargetPositionGetter.GetAttackPosition(attack, target);
        }

        public bool IsTargetInAttackRange(IUnitAttack attack, IHealth target)
        {
            Vector3 position = AttackTargetPositionGetter.GetAttackPosition(attack, target);
            return AttackDistanceHandler.IsTargetInAttackRange(attack, position, target);
        }
        
        #region Find Enemies

        /// <summary>
        /// Finds the nearest enemy for the unit that is attempting to attack.
        /// </summary>
        /// <param name="attack">The attacking unit.</param>
        /// <param name="health">The health of the nearest enemy found.</param>
        /// <returns>True if an enemy is found; otherwise, false.</returns>
        public bool FindNearestEnemy(IUnitAttack attack, out IHealth health)
        {
            return FindNearestEnemyByStance(attack, null, out health);
        }

        /// <summary>
        /// Finds the nearest enemy while considering the attack stance of the unit.
        /// </summary>
        /// <param name="attack">The attacking unit.</param>
        /// <param name="ignoringTarget">A target to ignore in the search.</param>
        /// <param name="health">The health of the nearest enemy found.</param>
        /// <returns>True if an enemy is found; otherwise, false.</returns>
        public bool FindNearestEnemyByStance(IUnitAttack attack, IHealth ignoringTarget, out IHealth health)
        {
            IHealth[] targets;

            if (attack.Stance == AttackStance.Defensive &&
                attack is IDefendPosition defendingPosition)
            {
                // Find next valid target within defending position, not around
                // the attacker itself.
                Vector3 position = defendingPosition.DefendPosition;
                float range = defendingPosition.DefensiveRange;

                targets = FindNearbyEnemies(attack, position, range);
            }
            else
            {
                targets = FindNearbyEnemies(attack, attack.Position, attack.LineOfSight);
            }

            if (targets.Length > 0)
            {
                for (int index = 0; index < targets.Length; index++)
                {
                    if (targets[index] != ignoringTarget)
                    {
                        health = targets[index];
                        return true;
                    }
                }
            }

            health = null;
            return false;
        }

        /// <summary>
        /// Finds the nearest enemy within a specified range.
        /// </summary>
        /// <param name="attacker">The unit attacking.</param>
        /// <param name="range">The range within which to find the enemy.</param>
        /// <param name="health">The health of the nearest enemy found.</param>
        /// <returns>True if an enemy is found; otherwise, false.</returns>
        public bool FindNearestEnemy(IUnitAttack attacker, float range, out IHealth health)
        {
            return HealthFinder.FindNearestHealth(attacker, attacker.Position, range, out health);
        }

        /// <summary>
        /// Finds all nearby enemies within a specific range.
        /// </summary>
        /// <param name="attacker">The unit attacking.</param>
        /// <param name="position">The position from which to find nearby enemies.</param>
        /// <param name="range">The range within which to find enemies.</param>
        /// <returns>An array of nearby enemies.</returns>
        public IHealth[] FindNearbyEnemies(IUnitAttack attacker, Vector3 position, float range)
        {
            return HealthFinder.FindNearbyHealth(attacker, position, range);
        }
        
        #endregion

        #region Events

        /// <summary>
        /// Handles the event when a unit is damaged.
        /// </summary>
        /// <param name="health">The health of the damaged unit.</param>
        /// <param name="attacker">The attacking unit.</param>
        /// <param name="damage">Amount of damage dealt.</param>
        private void UnitDamaged(IHealth health, UnitAttack attacker, int damage)
        {
            if (health.IsDepleted) return;

            // Check if attacker is even still valid
            if (attacker == null || attacker.IsActive == false) return;

            // Check if damaged entity is unit with attack component,
            // and in valid stance, then it should try to engage back.
            if (health.Entity is Unit unit &&
                !unit.HasActiveTask() &&
                unit.UnitAttack is UnitAttack attack &&
                attack.Stance != AttackStance.NoAttack &&
                attack.Stance != AttackStance.StandGround &&
                attacker.Unit.Health != null)
            {
                if (attack.Stance == AttackStance.Defensive &&
                    attack is IDefendPosition defend)
                {
                    float distance = Vector3.Distance(defend.DefendPosition, attack.Position);
                    if (distance > defend.DefensiveRange)
                    {
                        // Ignore the attack
                        return;
                    }
                    else
                    {
                        attack.GoAttackEntity(attacker.Unit.Health);
                    }
                }
                else
                {
                    // Preventing moving targets from randomly picking up damage
                    // and engaging
                    if (attack.Unit.Movement?.HasReachedDestination() != false)
                        attack.GoAttackEntity(attacker.Unit.Health);
                }
            }
        }

        /// <summary>
        /// Handles the event when a unit is killed.
        /// </summary>
        /// <param name="health">The health component of the unit killed.</param>
        /// <param name="_">The attacker of the unit (unused).</param>
        private void UnitKilled(IHealth health, UnitAttack _)
        {
            totalUnitLostCount++;

            // Remove unit from player, the rest of the cleanup should be done
            // by the player and its observers of the unit removal.
            if (health.Entity is Unit unit)
                unit.RemoveEntityFromOwner();
        }

        private void UnitAdded(IUnit unit)
        {
            if (unit.UnitAttack.IsNotNull() && !units.Contains(unit.UnitAttack))
            {
                units.Add(unit.UnitAttack);
            }

            if (unit.Health.IsNotNull())
            {
                unit.Health.OnHitpointDecreased.AddListener(UnitDamaged);
                unit.Health.OnHealthDepleted.AddListener(UnitKilled);
            }
        }

        private void UnitRemoved(IUnit unit)
        {
            units.Remove(unit?.UnitAttack);
            
            if (unit?.Health.IsNotNull() == true)
            {
                unit.Health.OnHitpointDecreased.RemoveListener(UnitDamaged);
                unit.Health.OnHealthDepleted.RemoveListener(UnitKilled);
            }
        }

        /// <summary>
        /// Periodically checks nearby enemies for attackers which have no existing
        /// tasks & are not moving.
        /// </summary>
        /// <returns></returns>
        private IEnumerator CheckForNearbyEnemy()
        {
            while (isActiveAndEnabled)
            {
                yield return new WaitForSeconds(nearbyEnemySearchInterval);

                foreach (var attack in units)
                {
                    // Prevent engaging with units that are missing or disabled.
                    if (attack.IsNull() || !attack.IsActive || attack.Unit == null || !attack.Unit.isActiveAndEnabled)
                        continue;

                    // Ignore unit if its moving with supported interface.
                    if (attack.Unit.Movement?.HasReachedDestination() == false)
                        continue;

                    // If already on active task or has no attack stance, no need to check for enemies
                    if (attack.Unit.HasActiveTask() || attack.Stance == AttackStance.NoAttack)
                        continue;

                    if (FindNearestEnemy(attack, out var enemy))
                    {
                        attack.GoAttackEntity(enemy);
                    }
                }
            }
        }

        #endregion
    }
    
}
