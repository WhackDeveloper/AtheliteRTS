namespace TRavljen.UnitSystem.Combat
{
    using UnityEngine.Events;

    /// <summary>
    /// Singleton class for handling combat-related events in the game.
    /// Provides event hooks for when health changes or is depleted during combat.
    /// </summary>
    public class CombatEvents
    {
        /// <summary>
        /// Singleton instance of the <see cref="CombatEvents"/> class.
        /// </summary>
        public static readonly CombatEvents Instance = new();

        /// <summary>
        /// Private constructor to enforce singleton pattern.
        /// </summary>
        private CombatEvents() { }

        /// <summary>
        /// Event triggered when a entity's health decreases.
        /// </summary>
        /// <remarks>
        /// Parameters:
        /// - <see cref="IHealth"/>: The health component of the affected entity.
        /// - <see cref="AUnitComponent"/>: The attacker responsible for dealing damage.
        /// - <c>int</c>: The amount of damage dealt.
        /// </remarks>
        public UnityEvent<IHealth, AUnitComponent, int> OnHitpointDecreased = new();

        /// <summary>
        /// Event triggered when a entity's health increases (e.g., from healing or regeneration).
        /// </summary>
        /// <remarks>
        /// Parameters:
        /// - <see cref="IHealth"/>: The health component of the affected entity.
        /// - <see cref="AUnitComponent"/>: The source responsible for increasing health.
        /// - <c>int</c>: The amount of health restored.
        /// </remarks>
        public UnityEvent<IHealth, AUnitComponent, int> OnHitpointIncreased = new();

        /// <summary>
        /// Event triggered when an entity's health is fully depleted,
        /// causing it to die or be destroyed.
        /// </summary>
        /// <remarks>
        /// Parameters:
        /// - <see cref="IHealth"/>: The health component of the affected entity.
        /// - <see cref="IHealth"/>: The owner of the affected entity.
        /// - <see cref="AUnitComponent"/>: The last attacker or responsible source for depleting health.
        /// </remarks>
        public UnityEvent<IHealth, APlayer, AUnitComponent> OnHealthDepleted = new();
    }

}