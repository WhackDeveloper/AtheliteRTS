using UnityEngine.Events;

namespace TRavljen.UnitSystem.Combat
{
    using Interactions;
    
    /// <summary>
    /// Defines the interface for health components in the combat system.
    /// </summary>
    public interface IHealth : IUnitInteracteeComponent
    {

        /// <summary>
        /// Gets the current health of the entity.
        /// </summary>
        int CurrentHealth { get; }

        /// <summary>
        /// Gets the maximum health the entity can have.
        /// </summary>
        int MaxHealth { get; }

        /// <summary>
        /// Determines the regeneration amount per tick.
        /// </summary>
        int Regenerates { get; }

        /// <summary>
        /// Determines if the entity's health has been fully depleted.
        /// </summary>
        bool IsDepleted { get; }

        /// <summary>
        /// Gets the current health as a percentage of the maximum health.
        /// </summary>
        float CurrentHealthPercentage { get; }

        /// <summary>
        /// Determines if the entity is invulnerable to damage.
        /// </summary>
        bool IsInvulnerable { get; }

        /// <summary>
        /// Event invoked when the health decreases.
        /// </summary>
        UnityEvent<IHealth, UnitAttack, int> OnHitpointDecreased { get; }

        /// <summary>
        /// Event invoked when the health increases. Entity provided is responsible
        /// for increasing health points. In cases of player ability or self regeneration
        /// this entity will be null.
        /// </summary>
        UnityEvent<IHealth, IEntity, int> OnHitpointIncreased { get; }

        /// <summary>
        /// Event invoked when the health is fully depleted (e.g., the entity dies or is destroyed).
        /// </summary>
        UnityEvent<IHealth, UnitAttack> OnHealthDepleted { get; }

        /// <summary>
        /// Event invoked when health is set but not damaged or healed.
        /// </summary>
        UnityEvent<IHealth, int> OnHealthSet { get; }

        /// <summary>
        /// Reduces the entity's health by the specified damage amount.
        /// </summary>
        /// <param name="attacker">The attacker dealing damage.</param>
        /// <param name="damage">The amount of damage dealt.</param>
        void Damage(UnitAttack attacker, int damage);

        /// <summary>
        /// Increases the entity's health by the specified amount.
        /// </summary>
        /// <param name="entity">
        /// The entity that is responsible for health points increase.
        /// This may also be null if it was done directly by player via some ability.
        /// </param>
        /// <param name="amount">The amount to increase (heal/repair).</param>
        void Heal(IEntity entity, int amount);

        /// <summary>
        /// Sets current health to the specified amount. This should called
        /// when entity is not damaged or healed.
        /// </summary>
        /// <param name="amount">New current health.</param>
        void SetCurrentHealth(int amount);

    }
}
