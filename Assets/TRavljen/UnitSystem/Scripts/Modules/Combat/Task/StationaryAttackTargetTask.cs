using UnityEngine;

namespace TRavljen.UnitSystem.Combat
{
    using Task;
    using Interactions;

    /// <summary>
    /// Represents a task for a stationary unit to attack a specified target. 
    /// This task ensures the target is within the attack range and validates 
    /// conditions such as the attacker's state and the target's health.
    /// </summary>
    public class StationaryAttackTargetTask : ITask
    {

        /// <summary>
        /// The interval in seconds between distance checks to avoid constant frame updates.
        /// </summary>
        [Tooltip("The interval in seconds between distance checks to avoid constant frame updates.")]
        [SerializeField]
        private float distanceValidationCheckInterval = 0.3f;

        /// <inheritdoc/>
        public bool CanExecuteTask(ITaskContext context, ITaskInput input)
        {
            if (context is not UnitInteractionContext interactionContext ||
                input is not AttackTaskInput attackInput)
                return false;

            IUnitAttack attack = attackInput.attack;

            if (attack == null ||
                !attack.Unit.IsOperational ||
                !(interactionContext.Target is Health { IsActive: true } health))
                return false;

            // Check for invalid unit type
            if (health.Entity.Data is AUnitSO unit 
                && unit.DoesMatchAnyType(attack.InvalidTargetTypes))
                return false;

            // Check if target is too far or too close for attacking.
            float distance = Vector3.Distance(attack.Position, health.transform.position);
            if (distance < attack.MinInteractionRange || attack.MaxInteractionRange < distance)
                return false;

            if (health is ILimitedUnitInteractionTarget limits &&
                limits.HasReachedLimit())
                return false;

            Unit unitA = attack.Unit;

            if (unitA && health.TryGetComponent(out Entity entityB))
                return unitA.IsEnemy(entityB);

            // If its not unit or no owner, allow interaction.
            return true;
        }

        /// <inheritdoc/>
        public ITaskHandler CreateHandler()
        {
            return new StationaryAttackTargetTaskHandler(distanceValidationCheckInterval);
        }
    }

    /// <summary>
    /// Handles the execution of a stationary attack task. This handler ensures the target 
    /// remains within the attack range and performs attacks at appropriate intervals.
    /// </summary>
    public class StationaryAttackTargetTaskHandler : AAttackTargetTaskHandler
    {

        private readonly float distanceValidationCheckInterval;

        /// <summary>
        /// Timer to track when to perform the next distance validation.
        /// </summary>
        private float timer = 0;

        public StationaryAttackTargetTaskHandler(float distanceValidationCheckInterval)
        {
            this.distanceValidationCheckInterval = distanceValidationCheckInterval;
        }

        /// <summary>
        /// Initializes the task handler and assigns the target if possible.
        /// Marks the task as complete if the target cannot be assigned.
        /// </summary>
        /// <param name="context">The context containing interaction details, such as the attacker and target.</param>
        /// <param name="input">The input data for the task, including attack-related information.</param>
        public override void StartTask(ITaskContext context, ITaskInput input)
        {
            base.StartTask(context, input);

            if (attackTarget == null &&
                (attackTarget is ILimitedUnitInteractionTarget limits &&
                !limits.Assign(attack.Unit)))
            {
                isComplete = true;
                isAttacking = false;
            }    
        }

        /// <summary>
        /// Updates the attack logic. Validates the distance between the attacker and the target
        /// at regular intervals, performs the attack if in range, and completes the task if out of range.
        /// </summary>
        protected override void UpdateAttack(float deltaTime)
        {
            timer -= deltaTime;
            if (timer <= 0)
            {
                float distance = Vector3.Distance(attack.Position, targetTransform.position);
                isAttacking = attack.MinInteractionRange < distance && distance < attack.MaxInteractionRange;
                timer = distanceValidationCheckInterval;
            }

            if (!isAttacking)
                isComplete = true;
            else if (attack.HasReloaded())
                PerformAttack();
        }

    }

}