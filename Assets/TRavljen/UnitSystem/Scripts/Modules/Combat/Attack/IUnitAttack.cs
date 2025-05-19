using TRavljen.UnitSystem.Interactions;
using UnityEngine;

namespace TRavljen.UnitSystem.Combat
{

    /// <summary>
    /// Defines the interface for components that manage a unit's attack capabilities.
    /// </summary>
    public interface IUnitAttack : IUnitInteractorComponent
    {
        /// <summary>
        /// Gets the current stance of the unit (e.g., Aggressive, Defensive).
        /// </summary>
        AttackStance Stance { get; }

        /// <summary>
        /// Gets the maximum range within which the unit can detect targets.
        /// </summary>
        float LineOfSight { get; }
        
        /// <summary>
        /// Excludes types specified here as valid targets for engaging.
        /// </summary>
        UnitTypeSO[] InvalidTargetTypes { get; }

        /// <summary>
        /// Commands the unit to attack a specified entity.
        /// </summary>
        /// <param name="entity">The target entity to attack.</param>
        /// <returns>True if the attack task was successfully scheduled; otherwise, false.</returns>
        bool GoAttackEntity(IEntity entity);

        /// <summary>
        /// Commands the unit to attack a specified health component.
        /// </summary>
        /// <param name="health">The target health component to attack.</param>
        /// <returns>True if the attack task was successfully scheduled; otherwise, false.</returns>
        bool GoAttackEntity(IHealth health);

        /// <summary>
        /// Sets the attack stance of the unit, changing its behavior even during combat.
        /// </summary>
        /// <param name="stance">The new attack stance to apply.</param>
        void SetStance(AttackStance stance);

    }

}