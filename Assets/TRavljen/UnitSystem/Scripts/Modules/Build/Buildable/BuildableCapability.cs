using TRavljen.UnitSystem.Navigation;
using UnityEngine;

namespace TRavljen.UnitSystem.Build
{
    
    /// <summary>
    /// Defines the implementation for the <see cref="IBuildableCapability"/> interface.
    /// Encapsulates the configuration required for enabling the building functionality on an entity.
    /// </summary>
    public struct BuildableCapability : IBuildableCapability
    {

        [Tooltip("Determines whether the building process starts automatically.")]
        [SerializeField]
        private bool autoBuild;

        [Tooltip("Determines whether the building process uses health for progress.")]
        [SerializeField]
        private bool useHealth;

        public readonly bool AutoBuild => autoBuild;

        public readonly bool UsesHealth => useHealth;

        #region IEntityCapability

        /// <summary>
        /// Adds the <see cref="EntityBuilding"/> component to the provided entity's <see cref="GameObject"/>
        /// if it is not already present.
        /// </summary>
        /// <param name="entity">The entity to configure with the building capability.</param>
        readonly void IEntityCapability.ConfigureEntity(IEntity entity)
        {
            entity.gameObject.AddComponentIfNotPresent<EntityBuilding>().enabled = false;
        }

        void IEntityCapability.SetDefaultValues()
        {
            autoBuild = false;
            useHealth = true;
        }

        #endregion

    }

}