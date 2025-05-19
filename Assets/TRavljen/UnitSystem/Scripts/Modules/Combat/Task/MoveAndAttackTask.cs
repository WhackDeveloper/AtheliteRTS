using System;
using System.Collections.Generic;
using UnityEngine;

namespace TRavljen.UnitSystem.Combat
{

    using Interactions;
    using Task;

    /// <summary>
    /// A composite task that allows a unit to move towards a position and attack any encountered enemies
    /// within its sight range. If no enemies are present, the unit will proceed to the final destination.
    /// </summary>
    public class MoveAndAttackTask : ITask
    {
        
        /// <inheritdoc/>
        public bool CanExecuteTask(ITaskContext context, ITaskInput input)
        {
            if (context is not PositionContext)
                return false;

            if (input is not AttackTaskInput attackInput)
                return false;

            if (attackInput.attack == null)
                throw new ArgumentNullException("attack on input was null!");

            IUnit attackerUnit = attackInput.attack.Unit;
            IHealth health = attackerUnit.Health;

            if (health.IsNull() || health.IsDepleted) return false;
            
            // Check for invalid unit type
            if (health.Entity.Data is AUnitSO unit
                && unit.DoesMatchAnyType(attackInput.attack.InvalidTargetTypes))
                return false;

            if (health is ILimitedUnitInteractionTarget limits &&
                limits.HasReachedLimit())
                return false;

            return attackerUnit.IsOperational && health.IsInvulnerable;
        }

        /// <inheritdoc/>
        public ITaskHandler CreateHandler()
        {
            return new MoveAndAttackTaskHandler();
        }
    }

    /// <summary>
    /// Handles the movement and attack behavior for the MoveAndAttackTask. This handler
    /// transitions between states: moving to a destination, engaging enemies within range,
    /// and resuming movement if no enemies are present.
    /// </summary>
    public class MoveAndAttackTaskHandler : ITaskHandler
    {

        private enum State { Idle, Moving, Attacking, Finished }

        private ITaskHandler activeHandler = null;

        private UnitAttack attack;
        private IUnitMovement movement;
        private CombatModule module;

        private State state = State.Idle;
        private Vector3 finalDestination;

        /// <summary>
        /// Initializes a new instance of the <see cref="MoveAndAttackTaskHandler"/> class.
        /// </summary>
        public MoveAndAttackTaskHandler() { }

        /// <inheritdoc/>
        public bool IsFinished() => state == State.Finished;

        /// <summary>
        /// Begins the task, setting up the movement to the target position.
        /// </summary>
        /// <param name="context">The task context containing the target position.</param>
        /// <param name="input">The task input containing attack information.</param>
        public void StartTask(ITaskContext context, ITaskInput input)
        {
            AttackTaskInput attackInput = (AttackTaskInput)input;
            attack = attackInput.attack;
            movement = attack.Unit.Movement;
            module = attack.Unit.Owner.GetModule<CombatModule>();

            finalDestination = (context as PositionContext).TargetPosition;

            state = State.Moving;
            movement.SetDestination(finalDestination);
        }

        /// <summary>
        /// Updates the task, transitioning between states based on movement and combat conditions.
        /// </summary>
        /// <param name="deltaTime">The time elapsed since the last update.</param>
        public void UpdateTask(float deltaTime)
        {
            if (state == State.Moving && movement.HasReachedDestination())
            {
                state = State.Finished;
            }
            else
            {
                if (state == State.Attacking)
                {
                    activeHandler.UpdateTask(deltaTime);

                    if (activeHandler.IsFinished())
                    {
                        activeHandler.EndTask();
                        activeHandler = null;
                        state = State.Idle;
                    }
                    else
                    {
                        // Not finished attacking
                        return; 
                    }
                }

                // Handle finished or no longer attacking
                MoveOnward();
            }
        }

        /// <summary>
        /// Cleans up task-related components and stops movement.
        /// </summary>
        public void EndTask()
        {
            movement.StopInCurrentPosition();
        }

        private void MoveOnward()
        {
            if (module.FindNearestEnemy(attack, attack.LineOfSight, out var target))
            {
                state = State.Attacking;
                UnitInteractionContext context = new(target, true);

                // Get appropriate task from the unit for attacking the target
                if (attack is ITaskProvider taskProvider &&
                    taskProvider.CanProvideTaskForContext(context, out var task))
                {
                    activeHandler = task.CreateHandler();

                    // Prevent requesting for new targets when current one is
                    // no longer available.
                    if (activeHandler is AAttackTargetTaskHandler attackTask)
                        attackTask.RequestsNewTarget = false;

                    activeHandler.StartTask(context, new AttackTaskInput(attack));
                }
                else
                {
                    Debug.Log("Object using MoveAndAttackTask does not provide a task for attacking target: " + target);

                    state = State.Moving;
                    movement.SetDestination(finalDestination);
                }
            }
            else if (state == State.Idle)
            {
                state = State.Moving;
                movement.SetDestination(finalDestination);
            }
        }

    }

}