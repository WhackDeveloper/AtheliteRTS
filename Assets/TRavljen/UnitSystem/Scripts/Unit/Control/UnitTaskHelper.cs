using System;
using System.Collections.Generic;
using TRavljen.UnitSystem.Interactions;
using TRavljen.UnitSystem.Task;

namespace TRavljen.UnitSystem
{

    /// <summary>
    /// Static helper class for managing tasks on entities.
    /// </summary>
    public static class UnitTaskHelper
    {
        #region Task Management

        /// <summary>
        /// Schedules a task for the specified entity.
        /// </summary>
        /// <param name="unit">The unit to schedule the task for.</param>
        /// <param name="context">The task context.</param>
        /// <param name="input">The task input.</param>
        /// <param name="task">The task to schedule.</param>
        public static void ScheduleTask(this IUnit unit, ITaskContext context, ITaskInput input, ITask task)
        {
            TaskSystem.GetOrCreate().ScheduleTask(unit.gameObject, context, input, task);
        }

        /// <summary>
        /// Attempts to schedule a task for the specified entity using the provided context.
        /// </summary>
        /// <param name="unit">The unit to schedule the task for.</param>
        /// <param name="context">The task context.</param>
        /// <param name="cancelsExistingTasks">Cancels existing tasks before scheduling a new one.</param>
        /// <returns>True if a task was successfully scheduled; otherwise, false.</returns>
        public static bool TryScheduleTask(this IUnit unit, ITaskContext context, bool cancelsExistingTasks)
        {
            foreach (var provider in unit.transform.GetComponents<ITaskProvider>())
            {
                if (provider.CanProvideTaskForContext(context, out ITask task))
                {
                    if (cancelsExistingTasks)
                        RemoveAllTasks(unit);

                    provider.ScheduleTask(context, task);
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// Attempts to schedule a task for the specified entity targeting another entity.
        /// </summary>
        /// <param name="unit">The unit initiating the task.</param>
        /// <param name="target">The target entity.</param>
        /// <param name="command">
        /// Whether the interaction task is a command. If true this will also cancel previous tasks.
        /// </param>
        /// <returns>True if a task was successfully scheduled; otherwise, false.</returns>
        public static bool TryScheduleTask(this IUnit unit, IEntity target, bool command = false)
        {
            var interactions = target.transform.GetComponents<IUnitInteracteeComponent>();
            foreach (var interaction in interactions)
            {
                if (unit.TryScheduleTask(interaction, command))
                    return true;
            }

            return false;
        }

        /// <summary>
        /// Attempts to schedule a task for the specified entity interacting with an interactee.
        /// </summary>
        /// <param name="unit">The unit initiating the task.</param>
        /// <param name="interactee">The interactee.</param>
        /// <param name="command">
        /// Whether the interaction task is a command. If true this will also cancel previous tasks.
        /// </param>/// <returns>True if a task was successfully scheduled; otherwise, false.</returns>
        public static bool TryScheduleTask(this IUnit unit, IUnitInteracteeComponent interactee, bool command)
        {
            return TryScheduleTask(unit, new UnitInteractionContext(interactee, command), command);
        }

        /// <summary>
        /// Attempts to schedule a task for the specified entity targeting a list of potential targets.
        /// </summary>
        /// <param name="unit">The unit initiating the task.</param>
        /// <param name="command">
        /// Whether the interaction task is a command. If true this will also cancel previous tasks.
        /// </param>
        /// <param name="potentialTargets">The list of potential target entities.</param>
        /// <returns>True if a task was successfully scheduled; otherwise, false.</returns>
        public static bool TryScheduleTask(this IUnit unit, bool command, List<IEntity> potentialTargets, out IEntity target)
        {
            if (potentialTargets == null) throw new ArgumentNullException(nameof(potentialTargets));

            foreach (var potentialTarget in potentialTargets)
            {
                if (TryScheduleTask(unit, potentialTarget, command))
                {
                    target = potentialTarget;
                    return true;
                }
            }

            target = null;
            return false;
        }

        /// <summary>
        /// Removes all tasks associated with the specified entity.
        /// </summary>
        /// <param name="entity">The entity to clear tasks from.</param>
        public static void RemoveAllTasks(this IUnit entity)
        {
            TaskSystem.GetOrCreate().RemoveTasks(entity.gameObject);
        }

        /// <summary>
        /// Determines whether the specified unit has an active task.
        /// </summary>
        /// <param name="unit">The unit to check.</param>
        /// <returns>True if the unit has an active task; otherwise, false.</returns>
        public static bool HasActiveTask(this IUnit unit)
        {
            return TaskSystem.GetOrCreate().HasActiveTask(unit.gameObject);
        }

        #endregion
    }


}