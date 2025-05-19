using UnityEngine.Events;

namespace TRavljen.UnitSystem.Garrison
{
    using UnityEngine;
    using Task;

    /// <summary>
    /// Implements behavior for units that can interact with and enter garrison units.
    /// </summary>
    [System.Serializable]
    [DisallowMultipleComponent]
    public class GarrisonableUnit : AUnitComponent, IGarrisonableUnit, ITaskProvider
    {

        #region Properties

        [SerializeField, Tooltip("Minimum range required for interaction with a garrison.")]
        private float minInteractionRange = 0;

        [SerializeField, Tooltip("Maximum range allowed for interaction with a garrison.")]
        private float maxInteractionRange = 1;

        [Tooltip("Specifies if the game object should be deactivated when unit enters a " +
            "garrison or activated when exits a garrison.")]
        [SerializeField]
        private bool toggleActive = true;

        private readonly EnterGarrisonTask garrisonTask = new();

        private bool isGarrisoned = false;

        #endregion

        #region Getters

        /// <inheritdoc/>
        public bool IsGarrisoned => isGarrisoned;

        /// <inheritdoc/>
        public Vector3 Position => transform.position;

        /// <inheritdoc/>
        public float MinInteractionRange => minInteractionRange;

        /// <inheritdoc/>
        public float MaxInteractionRange => maxInteractionRange;

        #endregion

        #region Configuration

        /// <summary>
        /// Sets the minimum range for garrison interaction.
        /// </summary>
        /// <param name="minRange">The minimum range.</param>
        public void SetMinRange(float minRange) => minInteractionRange = minRange;

        /// <summary>
        /// Sets the maximum range for garrison interaction.
        /// </summary>
        /// <param name="maxRange">The maximum range.</param>
        public void SetMaxRange(float maxRange) => maxInteractionRange = maxRange;

        #endregion

        #region ITaskProvider

        public bool GoEnterGarrison(IGarrisonEntity garrison)
        {
            UnitInteractionContext context = new(garrison);
            return RunTask(context, garrisonTask);
        }

        bool ITaskProvider.CanProvideTaskForContext(ITaskContext context, out ITask taskToRun)
        {
            bool canRun = garrisonTask.CanExecuteTask(context, new EnterGarrisonInput(this));
            taskToRun = canRun ? garrisonTask : null;
            return canRun;
        }

        bool ITaskProvider.ScheduleTask(ITaskContext context, ITask task)
        {
            return RunTask(context, task);
        }

        private bool CanRunTask(ITaskContext context, EnterGarrisonInput input)
        {
            return garrisonTask.CanExecuteTask(context, input);
        }

        private bool RunTask(ITaskContext context, ITask task)
        {
            EnterGarrisonInput input = new(this);
            if (CanRunTask(context, input))
            {
                Unit.ScheduleTask(context, input, task);
                return true;
            }
            return false;
        }

        #endregion

        #region Garrison

        public virtual void EnterGarrison()
        {
            // Disable the unit's game object. You may add an animation here before disabling.
            isGarrisoned = true;

            if (toggleActive)
                gameObject.SetActive(false);
        }

        public virtual void ExitGarrison()
        {
            isGarrisoned = false;

            if (toggleActive && gameObject != null)
                // Simply activate game object again
                gameObject.SetActive(true);
        }

        #endregion
    }

}