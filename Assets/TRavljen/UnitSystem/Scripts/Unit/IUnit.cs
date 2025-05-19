using TRavljen.UnitSystem.Build;
using TRavljen.UnitSystem.Collection;
using TRavljen.UnitSystem.Combat;
using TRavljen.UnitSystem.Garrison;
using TRavljen.UnitSystem.Interactions;

namespace TRavljen.UnitSystem
{

    /// <summary>
    /// Interface representing core unit functionality.
    /// </summary>
    /// <remarks>
    /// Extends <see cref="IEntity"/> to include unit-specific data and operational state.
    /// This interface allows for querying unit metadata and state, such as whether it is operational.
    /// </remarks>
    public interface IUnit : IEntity
    {
        
        /// <summary>
        /// Contract for movement to interact. This should not itself implement
        /// the movement so it should depend on <see cref="Movement"/> for movement.
        /// </summary>
        public IMoveUnitForInteraction InteractionMovement { get; }

        /// <summary>
        /// Contract for unit movement.
        /// </summary>
        public IUnitMovement Movement { get; }

        /// <summary>
        /// Contract for garrisonable unit which allows this unit to enter a garrison.
        /// </summary>
        public IGarrisonableUnit GarrisonableUnit { get; }

        /// <summary>
        /// Contract for unit which supports building other units.
        /// </summary>
        public IBuilder Builder { get; }

        /// <summary>
        /// Contract for unit attacks.
        /// </summary>
        public IUnitAttack UnitAttack { get; }
        
        /// <summary>
        /// Contract for collecting resources.
        /// </summary>
        public IResourceCollector ResourceCollector { get; }
        
        /// <summary>
        /// Contract for depositing resources.
        /// </summary>
        public IResourceDepot ResourceDepot { get; }

        /// <summary>
        /// Overrides data type from <see cref="AEntitySO"/> to <see cref="AUnitSO"/>.
        /// </summary>
        public new AUnitSO Data { get; }

    }

}