using TRavljen.UnitSystem.Collection;

namespace TRavljen.UnitSystem
{

    using UnityEngine;
    using UnityEngine.Events;
    using Interactions;
    using Garrison;
    using Combat;
    using Build;
    using Task;

    /// <summary>
    /// Core component representing a unit in the game.
    /// </summary>
    /// <remarks>
    /// Units are entities that can participate in various interactions, such as movement, combat,
    /// production, or garrisoning. This class ties together unit-specific components, such as movement or control,
    /// and provides methods for managing unit state, such as operational readiness and destruction.
    /// </remarks>
    [DisallowMultipleComponent]
    public class Unit : Entity, IUnit, ITaskOwner
    {

        #region Properties

        // Overrides AEntitySO type.
        public new AUnitSO Data => data as AUnitSO;

        /// <summary>
        /// Currently active task.
        /// </summary>
        private ITaskHandler activeTask;
        
        /// <summary>
        /// Event invoked when the active task assigned to the unit changes.
        /// </summary>
        public UnityEvent<ITaskHandler, ITaskHandler> OnActiveTaskChange = new();

        /// <summary>
        /// Gets or sets the active task handler for this unit.
        /// </summary>
        public ITaskHandler ActiveTask
        {
            set
            {
                ITaskHandler old = activeTask;
                activeTask = value;
                OnActiveTaskChange.Invoke(old, activeTask);
            }

            get => activeTask;
        }

        /// <summary>
        /// This reference allows you to fully control where unit is positioned for interaction.
        /// This way you can improve obstacle avoidance, walk away, etc. This is where position
        /// and direction for interaction is computed and then passed to <see cref="Movement"/> behaviour.
        /// </summary>
        public IMoveUnitForInteraction InteractionMovement { private set; get; }

        public IUnitMovement Movement { private set; get; }
        public IGarrisonableUnit GarrisonableUnit { private set; get; }
        public IBuilder Builder { private set; get; }
        public IUnitAttack UnitAttack { private set; get; }

        public IResourceCollector ResourceCollector { private set; get; }
        public IResourceDepot ResourceDepot { private set; get; }

        #endregion

        #region Lifecycle

        public override void OnInitialize()
        {
            base.OnInitialize();

            InteractionMovement = GetComponent<IMoveUnitForInteraction>() ?? new DefaultMoveUnitForInteraction();

            Movement = GetComponent<IUnitMovement>();
            GarrisonableUnit = GetComponent<IGarrisonableUnit>();
            UnitAttack = GetComponent<IUnitAttack>();
            Builder = GetComponent<IBuilder>();
            ResourceCollector = GetComponent<IResourceCollector>();
            ResourceDepot = GetComponent<IResourceDepot>();
        }

        protected virtual void OnDisable()
        {
            // Clears target if component is disabled.
            if (TaskSystem.TryGetOrCreate(out TaskSystem system))
                system.RemoveTasks(gameObject);
        }

        /// <summary>
        /// Destroys the unit, optionally refunding the owner and applying a delay before destruction.
        /// </summary>
        /// <param name="refundPlayer">Whether the owner should be refunded for this unit.</param>
        /// <param name="delay">The delay in seconds before the unit is destroyed.</param>
        public virtual void DestroyUnit(bool refundPlayer = false, float delay = 0f)
        {
            if (refundPlayer && owner)
                owner.RefundPlayer(this);

            DestroyEntity(delay);
        }

        #endregion

    }

}