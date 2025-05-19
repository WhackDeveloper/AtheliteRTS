using System.Collections.Generic;

namespace TRavljen.UnitSystem.Task
{
    using UnityEngine;

    /// <summary>
    /// Represents parameters for a task, including its context and input.
    /// </summary>
    public struct TaskParameters
    {
        /// <summary>
        /// The context in which the task is executed.
        /// </summary>
        public ITaskContext Context;

        /// <summary>
        /// The input required for the task.
        /// </summary>
        public ITaskInput Input;
    }

    /// <summary>
    /// Manages the lifecycle of tasks for various objects, including queuing, execution, and cleanup.
    /// </summary>
    public class TaskSystem : MonoBehaviour
    {

        #region Queued Task

        /// <summary>
        /// Represents a queued task with its parameters and handler.
        /// </summary>
        public struct QueuedTask
        {
            /// <summary>
            /// The parameters associated with the task.
            /// </summary>
            public TaskParameters param;

            /// <summary>
            /// The handler responsible for executing the task.
            /// </summary>
            public ITaskHandler handler;

            /// <summary>
            /// Initializes a new instance of the <see cref="QueuedTask"/> struct.
            /// </summary>
            /// <param name="param">The parameters for the task.</param>
            /// <param name="handler">The handler to execute the task.</param>
            public QueuedTask(TaskParameters param, ITaskHandler handler)
            {
                this.param = param;
                this.handler = handler;
            }
        }

        #endregion

        #region Properties

        /// <summary>
        /// The singleton instance of the <see cref="TaskSystem"/>.
        /// </summary>
        private static TaskSystem instance;

        /// <summary>
        /// A dictionary of pending tasks for each GameObject.
        /// </summary>
        private readonly Dictionary<GameObject, Queue<QueuedTask>> pendingTasks = new();

        /// <summary>
        /// A dictionary of active tasks for each GameObject.
        /// </summary>
        private readonly Dictionary<GameObject, ITaskHandler> activeTasks = new();

        #endregion

        #region Singleton

        /// <summary>
        /// Retrieves or creates the singleton instance of the <see cref="TaskSystem"/>.
        /// </summary>
        /// <returns>The singleton <see cref="TaskSystem"/> instance.</returns>
        public static TaskSystem GetOrCreate()
        {
            if (instance) return instance;

            instance = SingletonHandler.Create<TaskSystem>("Task System");
            return instance;
        }

        /// <summary>
        /// Attempts to retrieve or create the singleton <see cref="TaskSystem"/>.
        /// </summary>
        /// <param name="system">The retrieved or created <see cref="TaskSystem"/> instance.</param>
        /// <returns>True if the instance was successfully retrieved or created; otherwise, false.</returns>
        public static bool TryGetOrCreate(out TaskSystem system)
        {
            system = GetOrCreate();
            return system != null;
        }

        #endregion

        #region Update

        private void Update()
        {
            UpdateActiveTasks();

            foreach (var pending in pendingTasks)
            {
                if (!activeTasks.TryGetValue(pending.Key, out var handler) || handler == null)
                {
                    Debug.LogError("TaskSystem contains pending task for this object " + pending.Key.name + ", but no active Task!");
                }
            }
        }

        /// <summary>
        /// Updates all active tasks, processing their lifecycle.
        /// </summary>
        private void UpdateActiveTasks()
        {
            List<GameObject> activeTaskOwners = new(activeTasks.Keys);

            foreach (var owner in activeTaskOwners)
            {
                if (owner == null || !owner.activeSelf)
                {
                    // Clearing out tasks from no longer valid owner. They will still
                    // end, so it may throw an exception if improperly handled.
                    RemoveTasks(owner);
                    continue;
                }

                // Can happen if one of the tasks was removed duo to destroyed
                // objects; process of previously executed task.
                if (!activeTasks.TryGetValue(owner, out var task))
                    continue;

                if (!UpdateTask(task)) continue;
                
                // Start pending task if one exists, but it won't be updated
                // in this iteration yet (as it should not be).
                if (TryStartPendingTask(owner, out _)) continue;
                
                // Remove entry, it's finished and no pending task was started
                activeTasks.Remove(owner);

                // Clear target on owner if no task was started, if one was
                // present then this is updated by StartTask method.
                if (owner.TryGetComponent(out ITaskOwner taskOwner))
                    taskOwner.ActiveTask = null;
            }
        }

        /// <summary>
        /// Updates a task and determines if it has finished.
        /// </summary>
        /// <param name="task">The task handler to update.</param>
        /// <returns>True if the task has finished, otherwise false.</returns>
        private bool UpdateTask(ITaskHandler task)
        {
            if (!task.IsFinished()) 
                task.UpdateTask(Time.deltaTime);

            if (task.IsFinished())
            {
                task.EndTask();
                return true;
            }
            else
            {
                return false;
            }
        }

        /// <summary>
        /// Attempts to start the next pending task for a given owner.
        /// </summary>
        /// <param name="owner">The owner of the tasks.</param>
        /// <param name="handler">The handler for the started task, if successful.</param>
        /// <returns>True if a pending task was started, otherwise false.</returns>
        private bool TryStartPendingTask(GameObject owner, out ITaskHandler handler)
        {
            if (!pendingTasks.TryGetValue(owner, out var queue))
            {
                handler = null;
                return false;
            }

            if (!queue.TryDequeue(out QueuedTask task))
            {
                // No pending tasks, we can cleanup the entry.
                pendingTasks.Remove(owner);
                handler = null;
                return false;
            }

            if (!StartTask(owner, task.param, task.handler))
            {
                // If tasks fails to start it will be still removed by dequeue above.
                handler = null;
                return false;
            }

            // Remove entry if this was the last one
            if (queue.Count == 0)
                pendingTasks.Remove(owner);

            handler = task.handler;
            return true;
        }

        #endregion

        #region Task Management

        /// <summary>
        /// Adds a task to the system for the specified GameObject.
        /// </summary>
        /// <param name="gameObject">The owner of the task.</param>
        /// <param name="context">The context for the task.</param>
        /// <param name="input">The input data for the task.</param>
        /// <param name="task">The task to add.</param>
        public void ScheduleTask(GameObject gameObject, ITaskContext context, ITaskInput input, ITask task)
        {
            ScheduleTask(gameObject, new() { Context = context, Input = input }, task.CreateHandler());
        }

        private void ScheduleTask(GameObject owner, TaskParameters param, ITaskHandler handler)
        {
            if (activeTasks.ContainsKey(owner))
            {
                if (!pendingTasks.TryGetValue(owner, out Queue<QueuedTask> handlers))
                {
                    handlers = new();
                    pendingTasks[owner] = handlers;
                }

                handlers.Enqueue(new(param, handler));
            }
            else
            {
                StartTask(owner, param, handler);
            }
        }

        /// <summary>
        /// Removes all tasks associated with the specified owner.
        /// </summary>
        /// <param name="owner">The owner whose tasks will be removed.</param>
        public void RemoveTasks(GameObject owner)
        {
            pendingTasks.Remove(owner);

            if (activeTasks.TryGetValue(owner, out ITaskHandler handler))
                handler.EndTask();

            activeTasks.Remove(owner);

            if (owner.TryGetComponent(out ITaskOwner taskOwner))
                taskOwner.ActiveTask = null;
        }

        #endregion

        #region Getters

        /// <summary>
        /// Gets the active task for the specified owner.
        /// </summary>
        /// <param name="owner">The owner whose active task is retrieved.</param>
        /// <returns>The active task handler, or null if none is active.</returns>
        public ITaskHandler GetActiveTask(GameObject owner)
        {
            if (activeTasks.TryGetValue(owner, out ITaskHandler handler))
                return handler;

            return null;
        }

        /// <summary>
        /// Gets all pending tasks for the specified owner.
        /// </summary>
        /// <param name="owner">The owner whose pending tasks are retrieved.</param>
        /// <returns>An array of pending tasks.</returns>
        public QueuedTask[] GetPendingTasks(GameObject owner)
        {
            if (pendingTasks.TryGetValue(owner, out Queue<QueuedTask> queuedTasks))
            {
                return queuedTasks.ToArray();
            }

            return new QueuedTask[0];
        }

        /// <summary>
        /// Determines whether the specified owner has an active task.
        /// </summary>
        /// <param name="owner">The owner to check.</param>
        /// <returns>True if the owner has an active task, otherwise false.</returns>
        public bool HasActiveTask(GameObject owner)
        {
            return GetActiveTask(owner) != null;
        }

        #endregion

        #region Convenience

        private bool StartTask(GameObject owner, TaskParameters param, ITaskHandler handler)
        {
            try
            {
                // Set active task before starting it!
                activeTasks[owner] = handler;
                handler?.StartTask(param.Context, param.Input);

                // Finishing update
                if (owner.TryGetComponent(out ITaskOwner taskOwner))
                    taskOwner.ActiveTask = handler;
            }
            catch (System.Exception exception)
            {
                Debug.LogError("Failed to start new task: " + exception);
                // Revert active task because of failure.
                handler = null;
                activeTasks[owner] = null;
            }

            return handler != null;
        }

        #endregion

    }

}