using UnityEngine;
using System.Collections.Generic;
using UnityEngine.Events;

namespace TRavljen.UnitSystem
{

    /// <summary>
    /// Module for managing production processes associated with a player.
    /// Tracks and updates components implementing <see cref="IProduce"/> on player units.
    /// </summary>
    /// <remarks>
    /// - Realtime automatic production may be paused by disabling this module.
    /// - Supports both real-time updates and manual production updates for flexible gameplay systems.
    /// </remarks>
    [DisallowMultipleComponent]
    public sealed class ProductionModule : APlayerModule, IActiveProductionDelegate
    {

        #region Properties

        /// <summary>
        /// Determines whether production updates should be applied manually.
        /// </summary>
        [SerializeField]
        [Tooltip("Enable manual updates for production, typically used in turn-based systems.")]
        private bool produceManually;

        /// <summary>
        /// The interval (in seconds) at which the production systems are updated.
        /// Defines how often production progresses and determines the tick rate
        /// for updating unit production components.
        /// </summary>
        [Tooltip("Specifies the interval at which production updates in real-time mode.")]
        [SerializeField, Range(0, 10)]
        private float tickInterval = 0.2f;

        [Tooltip("Specifies whether the production delegate for units is automatically " +
            "managed by the module using its default implementation. Disable to manually " +
            "managed production unit delegates; then set delegate to each " +
            "production unit individually.")]
        [SerializeField]
        private bool definesProductionDelegate = true;

        /// <summary>
        /// If production is requested for an entity or unit with building capabilities,
        /// they should be placed in world space instead of produced by unit itself.
        /// This event will be invoked to manage the placement of a production object.
        /// </summary>
        [Tooltip("This event will be invoked to manage the placement of a production object.")]
        public UnityEvent<PlacementRequiredInfo> OnStartPlacementRequest = new();

        /// <summary>
        /// List of active production components to update.
        /// </summary>
        private List<IProduce> producing = new();

        /// <summary>
        /// Enables or disables manual production. If enabled production will
        /// run in realtime using <see cref="tickInterval"/>.
        /// </summary>
        public bool ProduceManually
        {
            get => produceManually;
            set => produceManually = value;
        }

        private ResourceModule resourceModule;
        private PopulationModule populationModule;

        private bool hasPopulationModule;
        private bool hasResourceModule;
        private float productionTimer;

        /// <summary>
        /// Handles production request
        /// </summary>
        private IProductionRequestHandler requestHandler;

        #endregion

        #region Lifecycle

        protected override void Awake()
        {
            base.Awake();

            requestHandler ??= new DefaultProductionRequestHandler(player);

            producing = new List<IProduce>();

            // Get modules if present
            hasResourceModule = player.TryGetModule(out resourceModule);
            hasPopulationModule = player.TryGetModule(out populationModule);
        }

        private void OnEnable()
        {
            player.OnUnitAdded.AddListener(UnitAdded);
            player.OnUnitRemoved.AddListener(UnitRemoved);
        }

        private void OnDisable()
        {
            player.OnUnitAdded.RemoveListener(UnitAdded);
            player.OnUnitRemoved.RemoveListener(UnitRemoved);
        }

        private void Update()
        {
            if (produceManually) return;

            productionTimer += Time.deltaTime;

            if (productionTimer < tickInterval) return;

            productionTimer -= tickInterval;
            
            Produce(tickInterval);
        }

        #endregion

        #region Event Handlers

        /// <summary>
        /// Adds production components when a unit is added to the player.
        /// </summary>
        /// <param name="unit">The unit being added.</param>
        private void UnitAdded(IUnit unit)
        {
            IProduce[] newProductions = unit.transform.GetComponents<IProduce>();

            if (newProductions.Length > 0)
                producing.AddRange(newProductions);

            // Set delegate if enabled.
            if (definesProductionDelegate &&
                unit.ActiveProduction.IsNotNull() == true)
            {
                unit.ActiveProduction.Delegate = this;
            }
        }

        /// <summary>
        /// Removes production components when a unit is removed from the player.
        /// </summary>
        /// <param name="unit">The unit being removed.</param>
        private void UnitRemoved(IUnit unit)
        {
            if (unit.IsNull()) return;
            
            var newProductions = unit.gameObject.GetComponents<IProduce>();

            foreach (var produce in newProductions)
                producing.Remove(produce);

            // Should manage this reference only
            if (definesProductionDelegate &&
                unit.ActiveProduction.IsNotNull() &&
                unit.ActiveProduction.Delegate is ProductionModule _ == this)
            {
                unit.ActiveProduction.Delegate = null;
            }
        }

        #endregion

        #region IUnitProductionDelegate

        /// <summary>
        /// Checks if the producible can finish production by validating population
        /// consumption, if requirements of the producible are still fulfilled and
        /// for resource producible it also checks if storage will be full.
        /// </summary>
        /// <param name="producibleQuantity">Producible with quantity to check</param>
        /// <returns><c>true</c> if production may finish; otherwise <c>false</c>.</returns>
        public bool ShouldFinishProductionFor(ProducibleQuantity producibleQuantity)
        {
            // Check if population is valid for the producible
            if (hasPopulationModule && !populationModule.HasPopulationCapacity(producibleQuantity))
            {
                return false;
            }

            // Check requirements of the producible
            // Reason for this is that some things might have happened during production
            // and requirements might no longer be fulfilled or before production start
            // they were not checked as player can queue production before fulfilling
            // requirements.
            if (!player.FulfillsRequirements(producibleQuantity.Producible, producibleQuantity.Quantity))
            {
                return false;
            }

            // Finally in case of a resource production, check if it has storage for it.
            switch (producibleQuantity.Producible)
            {
                case ResourceSO resource:
                    // If there is no resource module, assume resources are managed differently.
                    if (!hasResourceModule) return true;
                    return resourceModule.HasEnoughStorage(resource, producibleQuantity.Quantity);
                default:
                    return true;
            }
        }

        #endregion

        #region Public Interface

        /// <summary>
        /// Applies production updates to all active production components.
        /// </summary>
        /// <param name="delta">
        /// The time or turn increment to apply to all production processes.
        /// </param>
        public void Produce(float delta)
        {
            foreach (var produce in producing)
                produce.Produce(delta);
        }

        /// <summary>
        /// Request production request for the entity. Generally if entity can produce the producible,
        /// it should be added to its production queue. If producible requires building it should be
        /// placed using <see cref="OnStartPlacementRequest"/> event. Placement is not managed by this module.
        /// If entity cannot produce or place the producible, it should ignore the request. 
        /// </summary>
        /// <param name="productionQuantity">Producible and its quantity</param>
        /// <param name="entity">Entity which will attempt to fulfill the request.</param>
        public void RequestProduction(ProducibleQuantity productionQuantity, IEntity entity)
        {
            requestHandler.HandleProductionRequest(productionQuantity, entity, (info) => OnStartPlacementRequest.Invoke(info));
        }

        /// <summary>
        /// Sets the new handler for production requests.
        /// </summary>
        /// <param name="customHandler">New custom handler</param>
        public void SetProductionRequestHandler(IProductionRequestHandler customHandler)
        {
            requestHandler = customHandler;
        }

        #endregion
    }

}