using TRavljen.EditorUtility;
using UnityEngine;
using UnityEngine.Events;

namespace TRavljen.UnitSystem.Build
{

    using Task;

    /// <summary>
    /// Represents a component that enables a unit to act as a builder, 
    /// managing building tasks and interactions with buildable entities.
    /// </summary>
    [DisallowMultipleComponent]
    public class Builder : AUnitComponent, ITaskInput, ITaskProvider, IBuilder
    {

        #region Properties

        [Tooltip("Determines whether the unit can pick up new tasks while moving to a destination.")]
        [SerializeField]
        private bool pickUpWorkWhenMoving = false;

        [Tooltip("The current build assignment the unit is working on.")]
        [SerializeField, DisableInInspector]
        private EntityBuilding currentTarget;

        [Tooltip("Event invoked when builder is in position and starts building process.")]
        public UnityEvent<IEntity> OnStartBuilding = new();
        
        [Tooltip("Event invoked when builder finishes building, for whatever reason.")]
        public UnityEvent<IEntity> OnFinishBuilding = new();

        private readonly BuildTargetTask buildTask = new();

        protected IBuilderCapability builder;

        public bool AutoPickup => builder.AutoPickup;

        #endregion

        #region Getters

        /// <summary>
        /// Gets whether the unit can pick up new work while moving.
        /// </summary>
        public bool PickUpWorkWhenMoving => pickUpWorkWhenMoving;

        /// <summary>
        /// Gets the build power of the unit.
        /// </summary>
        public int BuildPower => builder.Power;

        /// <summary>
        /// Gets whether the unit builds with intervals based on reload time.
        /// </summary>
        public bool BuildWithIntervals => BuildInterval > 0;

        /// <summary>
        /// Gets the reload time between build updates.
        /// </summary>
        public float BuildInterval => builder.BuildInterval;

        /// <summary>
        /// Gets the unit's current position in the world.
        /// </summary>
        public Vector3 Position => transform.position;

        /// <summary>
        /// Gets the unit's current build assignment.
        /// </summary>
        public EntityBuilding CurrentAssignment => currentTarget;

        /// <summary>
        /// Gets the radius for automatic task pickup.
        /// </summary>
        public float AutoPickUpWorkRadius => builder.PickupRadius;

        /// <summary>
        /// Gets the minimum range for interactions.
        /// </summary>
        public float MinInteractionRange => 0;

        /// <summary>
        /// Gets the maximum range for interactions.
        /// </summary>
        public float MaxInteractionRange => builder.Range;

        #endregion

        #region AUnitComponent

        protected override void OnInitialize()
        {
            base.OnInitialize();

            if (!Unit.Data.TryGetCapability(out builder))
            {
                Debug.LogError("Cannot use Builder component without specifying " +
                    "IBuilderCapability in the Unit's capabilities: " + Unit.Data.Name);
            }
        }

        #endregion

        #region ITaskProvider

        public bool GoBuildUnit(IBuildableEntity buildable)
        {
            return RunTask(new UnitInteractionContext(buildable), buildTask);
        }

        /// <summary>
        /// Determines if a task can be provided for the specified context.
        /// </summary>
        /// <param name="context">The task context to evaluate.</param>
        /// <param name="taskToRun">The task to execute, if valid.</param>
        /// <returns>True if a task can be provided, false otherwise.</returns>
        bool ITaskProvider.CanProvideTaskForContext(ITaskContext context, out ITask taskToRun)
        {
            bool canRun = buildTask.CanExecuteTask(context, new BuildTargetInput(this));
            taskToRun = canRun ? buildTask : null;
            return canRun;
        }

        /// <summary>
        /// Schedules a task for execution within the provided context.
        /// </summary>
        /// <param name="context">The task context.</param>
        /// <param name="task">The task to schedule.</param>
        /// <returns>True if the task was successfully scheduled, false otherwise.</returns>
        bool ITaskProvider.ScheduleTask(ITaskContext context, ITask task)
        {
            return RunTask(context, task);
        }

        private bool CanRunTask(ITaskContext context, BuildTargetInput input)
        {
            return buildTask.CanExecuteTask(context, input);
        }

        private bool RunTask(ITaskContext context, ITask task)
        {
            BuildTargetInput input = new(this);
            if (CanRunTask(context, input))
            {
                Unit.ScheduleTask(context, input, task);
                return true;
            }
            return false;
        }

        #endregion

    }

}