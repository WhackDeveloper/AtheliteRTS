using TRavljen.UnitSystem.Task;
using UnityEngine;

namespace TRavljen.UnitSystem.Garrison
{

    /// <summary>
    /// Handles the execution of the <see cref="EnterGarrisonTask"/>, directing the unit
    /// to move to the garrison and enter it.
    /// </summary>
    public class EnterGarrisonTaskHandler : ITaskHandler
    {

        #region Properties

        private IGarrisonEntity garrison;
        private IUnitMovement garrisonMovement;

        private IGarrisonableUnit garrisonableUnit;
        private IUnitMovement move;

        private bool isTaskFinished = false;
        private bool followsTarget = false;

        /// <summary>
        /// The current garrison target that the unit should approach.
        /// </summary>
        public IGarrisonEntity CurrentTarget => garrison;

        private readonly float targetPositionUpdateInterval = 0.5f;
        private float positionUpdateTimer = 0;

        #endregion

        public EnterGarrisonTaskHandler(float targetPositionUpdateInterval = 0.5f)
        {
            this.targetPositionUpdateInterval = targetPositionUpdateInterval;
        }

        #region ITaskHandler

        public bool IsFinished() => isTaskFinished;

        public void StartTask(ITaskContext context, ITaskInput input)
        {
            UnitInteractionContext interactionContext = context as UnitInteractionContext;

            garrisonableUnit = ((EnterGarrisonInput)input).garrisonableUnit;
            move = garrisonableUnit.Unit.Movement;
            garrison = interactionContext.Target as IGarrisonEntity;

            bool canGarrisonMove = garrison.Entity.TryGetEntityComponent(out garrisonMovement);
            followsTarget = canGarrisonMove && garrisonableUnit.Unit.Movement != null;

            MoveTowardsGarrison();
        }

        public void UpdateTask(float deltaTime)
        {
            // Validate garrison status.
            if (garrison.IsNull())
            {
                move.StopInCurrentPosition();
                isTaskFinished = true;
                return;
            }

            // Check destination in case the entrance on invalid position & unit can only get close.
            if (garrison.IsInRangeToEnter(garrisonableUnit) || move.HasReachedDestination())
            {
                isTaskFinished = true;

                if (!garrison.AddUnit(garrisonableUnit))
                {
                    garrison = null;
                }
            }
            else if (followsTarget)
            {
                positionUpdateTimer += Time.deltaTime;

                if (!garrisonMovement.HasReachedDestination() &&
                    positionUpdateTimer > targetPositionUpdateInterval)
                {
                    positionUpdateTimer = 0;
                    MoveTowardsGarrison();
                }
            }
        }

        public void EndTask()
        {
            garrisonableUnit.Unit.InteractionMovement.RemoveInteractionTarget(garrisonableUnit, garrison);
            garrison = null;
        }

        #endregion

        private void MoveTowardsGarrison()
        {
            garrisonableUnit.Unit.InteractionMovement.SetInteractionTarget(garrisonableUnit, garrison);
        }

    }

}