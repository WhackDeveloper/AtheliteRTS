using UnityEngine;

namespace TRavljen.UnitSystem.Combat
{
    using System;
    using Interactions;
    using Task;

    /// <summary>
    /// Abstract base class for handling attack tasks targeting specific units or entities.
    /// Implements the <see cref="ITaskHandler"/> interface to define behavior for initiating,
    /// updating, and completing attack-related tasks.
    /// </summary>
    public abstract class AAttackTargetTaskHandler : ITaskHandler
    {
        #region Fields

        /// <summary>
        /// The attacking unit's <see cref="UnitAttack"/> component used for managing attack logic.
        /// </summary>
        protected UnitAttack attack;

        /// <summary>
        /// The target's <see cref="Health"/> component, used for applying damage.
        /// </summary>
        protected IHealth attackTarget;

        /// <summary>
        /// Transform of the current target, cached for positioning purposes.
        /// </summary>
        protected Transform targetTransform;

        /// <summary>
        /// Indicates whether the unit is currently attacking.
        /// </summary>
        protected bool isAttacking = false;

        /// <summary>
        /// Indicates whether the attack task has completed.
        /// </summary>
        protected bool isComplete = false;

        protected CombatModule Module { get; private set; }

        #endregion

        #region Properties

        /// <summary>
        /// Determines if a new target should be requested when the current task is complete.
        /// </summary>
        public bool RequestsNewTarget = true;

        /// <summary>
        /// Gets the current attack target's <see cref="Health"/> component.
        /// </summary>
        public IHealth CurrentTarget => attackTarget;

        /// <summary>
        /// Indicates whether the unit is currently engaged in attacking.
        /// </summary>
        public bool IsAttacking => isAttacking;

        /// <summary>
        /// Checks if the task is finished and ready for cleanup or replacement.
        /// </summary>
        /// <returns>True if the task is complete; otherwise, false.</returns>
        public bool IsFinished() => isComplete;

        #endregion

        #region ITaskHandler Implementation

        /// <summary>
        /// Initializes the attack task with the provided context and input.
        /// </summary>
        /// <param name="context">The task context containing necessary interaction information.</param>
        /// <param name="input">The input data for the task, such as attack parameters.</param>
        public virtual void StartTask(ITaskContext context, ITaskInput input)
        {
            UnitInteractionContext interactionContext = context as UnitInteractionContext;
            Health target = interactionContext.Target as Health;

            attack = ((AttackTaskInput)input).attack;
            Module = attack.Owner.GetModule<CombatModule>();

            if (!SetNewTarget(target))
            {
                // Unit reached its interaction limit.
                isComplete = true;
            }
        }

        /// <summary>
        /// Updates the attack task logic each frame.
        /// </summary>
        /// <param name="deltaTime">The time elapsed since the last frame.</param>
        public virtual void UpdateTask(float deltaTime)
        {
            if (attackTarget.IsNotNull()
                && attackTarget.IsActive 
                && !attackTarget.IsDepleted)
            {
                UpdateAttack(deltaTime);
            }
            else
            {
                StopAttacking();
            }
        }

        /// <summary>
        /// Ends the attack task, cleaning up target references and resetting state if necessary.
        /// </summary>
        public virtual void EndTask()
        {
            RemoveTarget();

            if (Module.ResetAttackOnExit)
            {
                attack.ResetReload();
            }
        }

        protected virtual bool SetNewTarget(IHealth target)
        {
            if (target is not ILimitedUnitInteractionTarget limits ||
                limits.Assign(attack.Unit))
            {
                attackTarget = target;
                targetTransform = target.transform;
                return true;
            }
            else
            {
                return false;
            }
        }

        private void StopAttacking()
        {
            RemoveTarget();

            isAttacking = false;
            isComplete = true;

            if (RequestsNewTarget)
            {
                RequestNewTarget();
            }
        }

        protected virtual bool RequestNewTarget()
        {
            // Engages targets in sight or stays put.
            if (Module.FindNearestEnemy(attack, out IHealth nearbyTarget))
            {
                attack.GoAttackEntity(nearbyTarget);
                return true;
            }

            return false;
        }

        #endregion

        #region Abstract Methods

        /// <summary>
        /// Updates the attack logic. Must be implemented by derived classes.
        /// </summary>
        /// <param name="deltaTime">The time elapsed since the last frame.</param>
        protected abstract void UpdateAttack(float deltaTime);

        #endregion

        #region Helper Methods

        /// <summary>
        /// Executes the attack logic, calculating damage and applying it to the target.
        /// </summary>
        protected void PerformAttack()
        {
            int damage = attack.GetDamage(attackTarget.Entity);

            attack.StartReloading();

            if (attack.ManuallyTriggerAttack)
            {
                attack.OnAttack.Invoke(attack, attackTarget, damage);
            }
            else
            {
                // Automatically trigger interaction logic for damage application.
                attackTarget.Damage(attack, damage);
            }
        }

        /// <summary>
        /// Removes the current target and clears any associated data or assignments.
        /// </summary>
        protected void RemoveTarget()
        {
            if (attackTarget == null) return;

            if (attackTarget is ILimitedUnitInteractionTarget limits)
            {
                limits.Unassign(attack.Unit);
            }

            attackTarget = null;
        }

        #endregion
    }


}