using TRavljen.EditorUtility;
using UnityEngine;
using UnityEngine.Events;

namespace TRavljen.UnitSystem.Combat
{

    using Task;

    [System.Serializable]
    struct DefendingData
    {
        public Vector3 position;
        public Vector3 direction;

        [Tooltip("Specifies the range allowed for units in defensive stance to " +
            "follow their targets before returning to their positions.")]
        public float range;
    }

    /// <summary>
    /// Represents a unit's attack capability, providing mechanisms for managing attack tasks, 
    /// reloading, and interactions with targets. Supports dynamic behaviors such as 
    /// switching between attack modes and managing attack-specific settings.
    /// </summary>
    [DisallowMultipleComponent]
    public class UnitAttack : AUnitComponent, IUnitAttack, IDefendPosition, ITaskProvider
    {

        #region Properties

        /// <summary>
        /// Attack stance, defines engaging behaviour for the unit.
        /// </summary>
        [SerializeField]
        private AttackStance stance = AttackStance.Aggressive;

        /// <summary>
        /// Allows attacks to be manually triggered, overriding automatic attack behavior.
        /// </summary>
        [Tooltip("Allows attacks to be manually triggered, overriding automatic attack behavior.\n" +
            "Essentially this means that only the attack event will be invoked, but no damage applied, this then must be handled manually.")]
        [SerializeField]
        private bool manuallyTriggerAttack;

        /// <summary>
        /// The time of the last attack.
        /// </summary>
        [SerializeField, DisableInInspector]
        private float lastAttackTime = -1;

        /// <summary>
        /// Event triggered when the unit attacks, providing details such as damage dealt.
        /// </summary>
        public UnityEvent<UnitAttack, IHealth, int> OnAttack = new();

        /// <summary>
        /// Event triggered when the unit stance has changed.
        /// </summary>
        public UnityEvent<IUnitAttack, AttackStance> OnStanceChanged = new();

        /// <summary>
        /// The current stance of the unit, determining its behavior in combat.
        /// </summary>
        public AttackStance Stance => stance;

        /// <summary>
        /// Whether attacks are triggered manually instead of automatically.
        /// </summary>
        public bool ManuallyTriggerAttack
        {
            get => manuallyTriggerAttack;
            set => manuallyTriggerAttack = value;
        }

        private IAttackCapability data;
        private ITask[] attackTasks;
        private DefendingData defendingData = new() {  range = 15 };

        #endregion

        #region Getters

        /// <summary>
        /// The reload time between consecutive attacks.
        /// </summary>
        public float ReloadSpeed => data.ReloadSpeed;

        public float LineOfSight => data.LineOfSight;

        /// <summary>
        /// Retrieves the damage dealt to a specific target.
        /// </summary>
        public int GetDamage(IEntity entity) => data.GetDamage(entity);

        public Vector3 Position => transform.position;

        public float MinInteractionRange => data.MinRange;

        public float MaxInteractionRange => data.Range;

        public UnitTypeSO[] InvalidTargetTypes => data.InvalidTargetTypes;

        /// <summary>
        /// Determines whether the unit has reloaded and is ready to attack again.
        /// </summary>
        public bool HasReloaded() => lastAttackTime + ReloadSpeed < Time.time;

        /// <summary>
        /// Starts the reload timer, marking the beginning of the reload period.
        /// </summary>
        public void StartReloading() => lastAttackTime = Time.time;

        /// <summary>
        /// Resets the reload timer, making the unit ready to attack immediately.
        /// </summary>
        public void ResetReload() => lastAttackTime = -1;

        /// <summary>
        /// The position unit defends if <see cref="AttackStance.Defensive"/> stance
        /// is active. If stance is not active this position may be stale.
        /// </summary>
        public Vector3 DefendPosition => defendingData.position;

        /// <summary>
        /// The direction in which unit defends if <see cref="AttackStance.Defensive"/>
        /// stance is active. If stance is not active this direction may be stale.
        /// </summary>
        public Vector3 DefendDirection => defendingData.direction;

        /// <summary>
        /// The range unit may follow when defending position in
        /// <see cref="AttackStance.Defensive"/> stance.
        /// </summary>
        public float DefensiveRange => defendingData.range;

        #endregion

        #region Lifecycle

        /// <summary>
        /// Initializes the component and configures attack tasks based on unit capabilities.
        /// </summary>
        protected override void OnInitialize()
        {
            base.OnInitialize();

            if (!unit.Data.TryGetCapability(out data))
            {
                Debug.LogError("Cannot use UnitAttack without specifying Attack Stats in the Unit ScriptableObject: " + unit.Data.Name);
            }

            // Sets default stance
            stance = data.DefaultStance;

            // Sets the appropriate tasks
            if (Unit.TryGetComponent(out IUnitMovement _))
            {
                attackTasks = new ITask[] { new MobileAttackTargetTask(), new MoveAndAttackTask() };
            }
            else
            {
                attackTasks = new ITask[] { new StationaryAttackTargetTask() };
            }
        }

        private void Start()
        {
            // Save position for the defensive state.
            if (stance == AttackStance.Defensive)
            {
                if (unit.Movement != null)
                    defendingData.position = unit.Movement.Destination;
                else
                    defendingData.position = transform.position;

                defendingData.direction = transform.forward;
            }
        }

        #endregion

        public virtual void SetStance(AttackStance stance)
        {
            this.stance = stance;
            OnStanceChanged.Invoke(this, stance);
        }

        /// <summary>
        /// Configures the unit to adopt a defensive stance and defend a specific position
        /// and direction. Optionally moves the unit to the specified position.
        /// </summary>
        /// <param name="position">The position the unit should defend.</param>
        /// <param name="direction">The direction the unit should face while defending.</param>
        /// <param name="move">Whether the unit should move to the defensive position.</param>
        public virtual void Defend(Vector3 position, Vector3 direction, bool move)
        {
            if (stance != AttackStance.Defensive)
                SetStance(AttackStance.Defensive);

            defendingData.position = position;
            defendingData.direction = direction;

            if (move)
            {
                Unit.Movement?.SetDestinationAndDirection(position, direction);
            }
        }

        /// <inheritdoc/>
        bool IDefendPosition.IsInRange(Vector3 position)
        {
            return Vector3.Distance(DefendPosition, position) <= DefensiveRange;
        }

        /// <summary>
        /// Initiates an attack on a specific entity if it is a valid target.
        /// </summary>
        /// <param name="entity">The target entity to attack.</param>
        /// <returns>True if the attack was successfully initiated; otherwise, false.</returns>
        public bool GoAttackEntity(IEntity entity)
        {
            if (entity.Health != null)
            {
                return GoAttackEntity(entity.Health);
            }

            return false;
        }

        /// <summary>
        /// Initiates an attack on a specific health component.
        /// </summary>
        /// <param name="health">The target health component to attack.</param>
        /// <returns>True if the attack was successfully initiated; otherwise, false.</returns>
        public bool GoAttackEntity(IHealth health)
        {
            UnitInteractionContext context = new(health);
            if (CanRunTask(context, out ITask task))
            {
                Unit.ScheduleTask(context, new AttackTaskInput(this), task);
                return true;
            }

            return false;
        }

        #region ITaskProvider

        private bool CanRunTask(ITaskContext context, out ITask taskToRun)
        {
            return TaskHelper.CanRunTask(context, new AttackTaskInput(this), attackTasks, out taskToRun);
        }

        bool ITaskProvider.CanProvideTaskForContext(ITaskContext context, out ITask taskToRun)
        {
            return CanRunTask(context, out taskToRun);
        }

        bool ITaskProvider.ScheduleTask(ITaskContext context, ITask task)
        {
            return RunTask(context, task);
        }

        private bool RunTask(ITaskContext context, ITask task)
        {
            AttackTaskInput input = new(this);
            if (task.CanExecuteTask(context, input))
            {
                Unit.ScheduleTask(context, input, task);
                return true;
            }

            return false;
        }

        #endregion

        private void OnDrawGizmos()
        {

            if (stance == AttackStance.Defensive)
            {
                Gizmos.color = Color.blue;
                Gizmos.DrawWireSphere(defendingData.position, defendingData.range);
            }
            else if (stance == AttackStance.StandGround)
            {
                Gizmos.color = Color.green;
                Gizmos.DrawWireSphere(transform.position, 2);
            }
        }
    }

}
