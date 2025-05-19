using UnityEngine;

namespace TRavljen.UnitSystem
{

    /// <summary>
    /// Implementation of the <see cref="IActiveProductionCapability"/> interface,
    /// defining the production actions a unit can perform.
    /// </summary>
    public struct ActiveProductionCapability : IActiveProductionCapability
    {

        /// <summary>
        /// The production actions available to the unit.
        /// Each action specifies a producible item or task that the unit can perform.
        /// </summary>
        [SerializeField]
        private ProductionAction[] productionActions;

        /// <inheritdoc />
        public readonly ProductionAction[] ProductionActions => productionActions;

        #region IEntityCapability

        /// <summary>
        /// Configures the unit to support production by attaching a <see cref="ActiveProduction"/> component.
        /// </summary>
        /// <param name="entity">
        /// The entity to which the production capability is being configured.
        /// </param>
        readonly void IEntityCapability.ConfigureEntity(IEntity entity)
        {
            entity.gameObject.AddComponentIfNotPresent<ActiveProduction>();

            IUnitSpawn existingSpawnPoint = entity.gameObject.GetComponentInChildren<IUnitSpawn>();
            if (existingSpawnPoint == null)
            {
                entity.gameObject.AddComponent<UnitSpawnPoint>();
            }
        }

        void IEntityCapability.SetDefaultValues()
        {
            productionActions = null;
        }

        #endregion

    }
}