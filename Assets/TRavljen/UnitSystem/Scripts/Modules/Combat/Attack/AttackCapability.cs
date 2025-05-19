using System.Collections.Generic;
using UnityEngine;

namespace TRavljen.UnitSystem.Combat
{

    /// <summary>
    /// Specifies the entity damage for specified unit type or entity itself.
    /// </summary>
    [System.Serializable]
    internal struct EntityTypeDamage
    {
        [SerializeField] private UnitTypeSO unitType;
        [SerializeField] private AEntitySO entity;
        [SerializeField] private int damage;

        /// <summary>
        /// The specific unit type that this damage configuration applies to.
        /// </summary>
        public UnitTypeSO UnitType => unitType;

        /// <summary>
        /// The specific entity that this damage configuration applies to.
        /// </summary>
        public AEntitySO Entity => entity;

        /// <summary>
        /// The damage value for this configuration.
        /// </summary>
        public int Damage => damage;
    }

    /// <summary>
    /// Represents a combat capability for a unit, defining attributes such as attack range, reload time,
    /// and damage. This capability enables a unit to engage in combat and provides damage customization
    /// based on the target's type or specific entity data.
    /// </summary>
    [System.Serializable]
    public struct AttackCapability : IUnitCapability, IAttackCapability
    {

        [Tooltip("The default attack stance.")]
        [SerializeField] private AttackStance defaultStance;

        [HideInInspector]
        [Tooltip("The minimum attack range.")]
        [SerializeField] private float minRange;

        [Tooltip("The maximum attack range.")]
        [SerializeField] private float range;

        [Tooltip("The time required to reload between attacks")]
        [SerializeField] private float reloadSpeed;

        [Tooltip("The default damage value if no type-specific configuration is found.")]
        [SerializeField] private int damage;

        [Tooltip("The line-of-sight distance for detecting and targeting enemies.")]
        [SerializeField] private float lineOfSight;

        [SerializeField] private EntityTypeDamage[] damagePerType;

        [Tooltip("Unit Types specified here are not valid targets for this attacker.")]
        [SerializeField] private UnitTypeSO[] invalidTargetTypes;

        public readonly AttackStance DefaultStance => defaultStance;

        public readonly float MinRange => minRange;

        public readonly float Range => range;

        public readonly float ReloadSpeed => reloadSpeed;

        public readonly int Damage => damage;

        public readonly float LineOfSight => lineOfSight;

        public readonly UnitTypeSO[] InvalidTargetTypes => invalidTargetTypes;

        /// <summary>
        /// Retrieves the damage value based on the target's entity or unit type.
        /// Falls back to the default damage if no specific configuration exists.
        /// </summary>
        /// <param name="target">The target entity.</param>
        /// <returns>The damage value to apply to the target.</returns>
        public readonly int GetDamage(IEntity target)
        {
            if (damagePerType.Length == 0)
                return damage;

            if (target is not Unit unit)
                return damage;

            List<UnitTypeSO> types = new(unit.Data.UnitTypes);

            foreach (var damageType in damagePerType)
            {
                if (damageType.Entity == unit.Data)
                    return damageType.Damage;

                if (types.Contains(damageType.UnitType))
                    return damageType.Damage;
            }

            // No specific damage configuration found for this target
            return damage;
        }

        #region IEntityCapability

        /// <summary>
        /// Configures the associated entity with the necessary components for attack capability.
        /// Ensures the entity has a <see cref="UnitAttack"/> component.
        /// </summary>
        /// <param name="entity">The entity to configure.</param>
        readonly void IEntityCapability.ConfigureEntity(IEntity entity)
        {
            entity.gameObject.AddComponentIfNotPresent<UnitAttack>();
        }

        void IEntityCapability.SetDefaultValues()
        {
            defaultStance = AttackStance.Aggressive;
            minRange = 0;
            range = 1;
            reloadSpeed = 0.5f;
            damage = 10;
            lineOfSight = 15;
            damagePerType = null;
        }

        #endregion

    }

}