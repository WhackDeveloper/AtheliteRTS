using UnityEngine;

namespace TRavljen.UnitSystem.Combat
{
    using Interactions;
    using Task;

    /// <summary>
    /// Represents a task for attacking a specified target. This task checks if the attack 
    /// can be executed based on the attacker, the target's state, and other conditions. 
    /// It uses an interval to update target positions for moving targets.
    /// </summary>
    public class MobileAttackTargetTask : ITask
    {

        /// <summary>
        /// Specifies the interval, in seconds, between checks for the new position of a moving target.
        /// This avoids frequent updates, which could lead to performance issues.
        /// </summary>
        [Tooltip("Specifies the interval between checks for new position of a moving target." +
            "Avoid doing this every frame for it will update the target position of the attacker.")]
        [SerializeField, Range(0f, 30f)]
        private float movingUnitsCheckInterval = 0.3f;

        [Tooltip("Specifies range at which aggressive units and auto-engaging " +
            "stand ground units may search for closer units while already engaged on one.")]
        [SerializeField]
        private float closerEnemyRange = 2f;

        [SerializeField, Range(0, 60)]
        private float closerEnemyCheckInterval = 0.5f;

        /// <inheritdoc/>
        public bool CanExecuteTask(ITaskContext context, ITaskInput input)
        {
            if (context is not UnitInteractionContext interactionContext || input is not AttackTaskInput attackInput)
                return false;

            if (attackInput.attack == null)
                return false;

            Unit attacker = attackInput.attack.Unit;

            // Ensure the attacker is operational.
            if (!attacker.IsOperational)
                return false;

            IUnitInteracteeComponent target = interactionContext.Target;

            if (target is not IHealth health ||
                health.IsNull() ||
                health.IsDepleted)
                return false;

            // Check if the target has reached its interaction limit.
            if (target is ILimitedUnitInteractionTarget limits &&
                limits.HasReachedLimit())
            {
                return false;
            }

            // If the target is an entity, check if it is an enemy.
            if (health.Entity != null)
            {
                return attacker.IsEnemy(health.Entity);
            }

            // Allow interaction if the entity is missing.
            return true;
        }

        /// <inheritdoc/>
        public ITaskHandler CreateHandler() =>
            new MobileAttackTargetTaskHandler(movingUnitsCheckInterval, closerEnemyRange, closerEnemyCheckInterval);

    }

    public class MobileAttackTargetTaskHandler : AAttackTargetTaskHandler
    {

        #region Properties

        private readonly float movingUnitsCheckInterval;
        private readonly float closerEnemyRange;
        private readonly float closerEnemyCheckInterval;

        private IUnitMovement movement;
        private IUnitMovement targetMove;

        private bool followsTarget = false;
        private float checkMovementTimer = 0;
        private float checkCloserEnemyTimer = 0;
        private bool commandedByOwner = false;

        #endregion

        public MobileAttackTargetTaskHandler(float checkInterval, float closerEnemyRange, float closerEnemyCheckInterval)
        {
            movingUnitsCheckInterval = checkInterval;
            this.closerEnemyRange = closerEnemyRange;
            this.closerEnemyCheckInterval = closerEnemyCheckInterval;
        }

        #region ITaskHandler

        /// <summary>
        /// Initializes the task handler and assigns the target if possible.
        /// Marks the task as complete if the target cannot be assigned.
        /// </summary>
        /// <param name="context">The context containing interaction details, such as the attacker and target.</param>
        /// <param name="input">The input data for the task, including attack-related information.</param>
        public override void StartTask(ITaskContext context, ITaskInput input)
        {
            base.StartTask(context, input);

            if (isComplete) return;

            commandedByOwner = (context as UnitInteractionContext).Commanded;

            movement = attack.Unit.Movement;

            MoveToTarget();
        }

        public override void EndTask()
        {
            base.EndTask();

            // Check task was completed before ending & if unit in defensive position
            if (isComplete && attack.Stance == AttackStance.Defensive)
            {
                // Go back to original position
                movement.SetDestinationAndDirection(attack.DefendPosition, attack.DefendDirection);
            }
        }

        #endregion

        protected override bool RequestNewTarget()
        {
            switch (attack.Stance)
            {
                case AttackStance.Aggressive:
                    // Engages targets in sight or stays put.
                    if (Module.FindNearestEnemy(attack, attack.LineOfSight, out IHealth nearbyTarget))
                    {
                        return attack.GoAttackEntity(nearbyTarget);
                    }
                    break;

                case AttackStance.Defensive:
                    // Engages target in sight to defend position or stays put.
                    if (Module.FindNearestEnemyByStance(attack, attackTarget, out nearbyTarget))
                    {
                        return attack.GoAttackEntity(nearbyTarget);
                    }
                    break;

                case AttackStance.StandGround:
                    // Engages target in range, does not move.
                    if (Module.FindNearestEnemy(attack, attack.MaxInteractionRange, out nearbyTarget))
                    {
                        return attack.GoAttackEntity(nearbyTarget);
                    }
                    break;

                case AttackStance.NoAttack:
                    // Do nothing
                    break;
            }

            return false;
        }

        protected override void UpdateAttack(float deltaTime)
        {
            float distance = Vector3.Distance(attack.Position, CurrentTarget.Position);

            // Checks if target is out of range for attack completely,
            // but only when not commanded by the player.
            if (!commandedByOwner && distance > attack.LineOfSight)
            {
                // Finish attack
                movement.StopInCurrentPosition();
                isComplete = true;
                return;
            }

            if (CheckEnemiesForStance(deltaTime))
            {
                return;
            }

            // Check if is in attack distance.
            isAttacking = Module.IsTargetInAttackRange(attack, CurrentTarget);

            // Check if following is required
            if (!isAttacking && followsTarget && targetMove.IsMoving)
            {
                checkMovementTimer += deltaTime;
                
                // Wait for move to next position.
                if (checkMovementTimer >= movingUnitsCheckInterval)
                {
                    checkMovementTimer = 0;
                    
                    Vector3 attackPosition = Module.GetAttackPosition(attack, attackTarget);
                    movement.SetDestination(attackPosition);
                    return;
                }
            }

            if (!isAttacking) return;

            if (movement.IsMoving)
                movement.StopInCurrentPosition();

            Vector3 direction = (CurrentTarget.Position - attack.Position).normalized;
            movement.SetTargetDirection(direction);

            // Wait to face direction & reload
            if (movement.IsFacingTargetDirection() && attack.HasReloaded())
                PerformAttack();
        }
        
        protected override bool SetNewTarget(IHealth target)
        {
            followsTarget = false;

            if (!base.SetNewTarget(target)) return false;
            
            // Check if target entity can move
            if (CurrentTarget.Entity is not Unit unit) return true;
            
            targetMove = unit.Movement;
            followsTarget = targetMove != null;

            return true;
        }

        #region Convenience

        private void MoveToTarget()
        {   
            if (!commandedByOwner && attack.Stance == AttackStance.StandGround)
            {
                // Disable following for stand ground, without initial movement
                followsTarget = false;

                return;
            }
            
            Vector3 attackPosition = Module.GetAttackPosition(attack, attackTarget);
            movement.SetDestination(attackPosition);
            
            followsTarget = attack.Stance != AttackStance.StandGround
                && attackTarget != null
                && attackTarget.Entity is IUnit { Movement: not null };
        }

        private bool CheckEnemiesForStance(float deltaTime)
        {
            checkCloserEnemyTimer += deltaTime;

            if (checkCloserEnemyTimer < closerEnemyCheckInterval)
                // Do not check this update
                return false;

            // Reset timer
            checkCloserEnemyTimer = 0;

            switch (attack.Stance)
            {
                case AttackStance.Aggressive:
                    FindCloserTarget();
                    break;

                case AttackStance.StandGround:
                    if (movement.HasReachedDestination())
                        FindCloserTarget();
                    break;

                case AttackStance.Defensive:
                    // Checks if attacker is in defensive stance and makes sure it does
                    // not follow the current target for too far; by making sure attacker
                    // itself stays within range, but does not stop if target steps out
                    // while it is still in range.

                    if (attack is IDefendPosition defend)
                    {
                        // When target is out of range and no enemy is nearby.
                        if (!defend.IsInRange(attack.Position) && !FindCloserTarget(true))
                        {
                            // Moving back is done when task is finished, for some reason...
                            isComplete = true;

                            // Avoids performing attack right by finishing the update
                            return true;
                        }
                    }
                    else
                    {
                        // If no interface is implemented, behave as aggresive
                        FindCloserTarget();
                    }
                    break;

                case AttackStance.NoAttack:
                    break;
            }

            return false;
        }

        /// <summary>
        /// Looks for closer units to attack.
        /// </summary>
        /// <param name="searchByStance">Determins if the nearby search should be done based on stance.</param>
        /// <returns>Returns true if closer target was found.</returns>
        private bool FindCloserTarget(bool searchByStance = false)
        {
            if (FindCloserTarget(searchByStance, out var closestEnemy))
            {
                if (SetNewTarget(closestEnemy))
                {
                    MoveToTarget();
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// Finds closer target for specific stance if needed.
        /// </summary>
        /// <param name="searchByStance">Determins if the nearby search should be done based on stance.</param>
        /// <param name="health">Health of the closer target</param>
        /// <returns>Returns true if new target was found.</returns>
        private bool FindCloserTarget(bool searchByStance, out IHealth health)
        {
            if (searchByStance)
            {
                return Module.FindNearestEnemyByStance(attack, attackTarget, out health);
            }
            else if (Module.FindNearestEnemy(attack, closerEnemyRange, out health) && health != attackTarget)
            {
                return true;
            }

            health = null;
            return false;
        }

        #endregion
    }
}
