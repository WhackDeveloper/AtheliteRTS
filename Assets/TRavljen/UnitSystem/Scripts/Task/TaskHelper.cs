using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TRavljen.UnitSystem.Task
{

    /// <summary>
    /// Provides helper methods for evaluating and running tasks based on a given context and input.
    /// </summary>
    public static class TaskHelper
    {

        /// <summary>
        /// Determines whether any task in the provided array can be executed based on the given context and input.
        /// </summary>
        /// <param name="context">The task context for evaluation. This typically specifies the situation in which the task might run.</param>
        /// <param name="input">The task input, containing data and configurations for the task.</param>
        /// <param name="tasks">An array of tasks to evaluate.</param>
        /// <param name="taskToRun">
        /// Outputs the first task from the array that can be executed, or <c>null</c> if no task is valid.
        /// </param>
        /// <returns>
        /// <c>true</c> if a valid task is found and assigned to <paramref name="taskToRun"/>; otherwise, <c>false</c>.
        /// </returns>
        public static bool CanRunTask(ITaskContext context, ITaskInput input, ITask[] tasks, out ITask taskToRun)
        {
            foreach (var task in tasks)
            {
                if (task.CanExecuteTask(context, input))
                {
                    taskToRun = task;
                    return true;
                }
            }

            taskToRun = null;
            return false;
        }

    }


}