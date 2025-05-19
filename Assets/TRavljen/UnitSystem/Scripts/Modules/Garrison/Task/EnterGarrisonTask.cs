namespace TRavljen.UnitSystem.Garrison
{
    using System;
    using UnityEngine;
    using Task;

    /// <summary>
    /// Task for directing a unit to enter a garrison. The task ensures that both the unit
    /// and the garrison meet eligibility requirements before execution.
    /// </summary>
    [Serializable]
    public class EnterGarrisonTask : ITask
    {

        [Tooltip("Specifies interval for updating target position when target garrison " +
            "can move and unit needs to follow the garrison in order to enter it.")]
        [SerializeField, Range(0, 60)]
        private float targetPositionUpdateInterval = 0.5f;

        public bool CanExecuteTask(ITaskContext context, ITaskInput input)
        {
            // Validate context and input
            if (context is not UnitInteractionContext interactionContext ||
                interactionContext.Target is not IGarrisonEntity garrison ||
                input is not EnterGarrisonInput garrisonInput)
            {
                return false;
            }

            // Proceed to evaluate execution possibility
            var garrisonableUnit = garrisonInput.garrisonableUnit;

            // Check for enemy
            if (EntityRelationshipHelper.IsEnemy(garrisonableUnit.Entity, garrison.Entity))
                return false;

            // Check task execution conditions
            return garrison.IsActive &&
                garrisonableUnit.IsActive &&
                garrison.Entity.IsOperational &&
                garrison.IsEligibleToEnter(garrisonableUnit) &&
                garrisonableUnit.Unit.HasActiveTask() == false;
        }

        public ITaskHandler CreateHandler()
        {
            return new EnterGarrisonTaskHandler(targetPositionUpdateInterval);
        }
    }

}