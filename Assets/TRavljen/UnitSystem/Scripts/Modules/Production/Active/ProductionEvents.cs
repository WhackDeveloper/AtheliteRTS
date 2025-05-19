using UnityEngine.Events;

namespace TRavljen.UnitSystem
{

    /// <summary>
    /// Singleton class responsible for handling high-level production-related events 
    /// in the system. This class acts as a centralized point for raising and observing
    /// production events, which can be utilized by various systems or modules.
    ///
    /// For example, you can use these events to notify modules, UI components, 
    /// or other listeners when a production process is completed.
    /// </summary>
    public class ProductionEvents
    {
        /// <summary>
        /// Singleton instance of the <see cref="ProductionEvents"/> class. 
        /// Use this instance to subscribe to or invoke production events.
        /// </summary>
        public static readonly ProductionEvents Instance = new();

        /// <summary>
        /// Event invoked when a new production is scheduled on a unit-
        /// </summary>
        public readonly UnityEvent<ActiveProduction, AProducibleSO, long> OnNewProductionScheduled = new();

        /// <summary>
        /// Event invoked when specific production order is cancelled on the unit.
        /// </summary>
        public readonly UnityEvent<ActiveProduction, AProducibleSO, long> OnProductionCancelled = new();

        /// <summary>
        /// Event invoked when all productions have been cancelled on the unit.
        /// </summary>
        public readonly UnityEvent<ActiveProduction> OnAllProductionCancelled = new();

        /// <summary>
        /// Event invoked when production order has been finished.
        /// </summary>
        public readonly UnityEvent<ActiveProduction, ProducibleQuantity> OnProductionFinished = new();
    }

}