namespace TRavljen.UnitSystem.Combat
{

    /// <summary>
    /// Defines the capabilities of a unit or entity for performing attacks.
    /// </summary>
    public interface IAttackCapability : IEntityCapability
    {

        /// <summary>
        /// The default attack stance.
        /// </summary>
        AttackStance DefaultStance { get; }

        /// <summary>
        /// The minimum range required to initiate an attack.
        /// </summary>
        float MinRange { get; }

        /// <summary>
        /// The maximum range within which the attack is effective.
        /// </summary>
        float Range { get; }

        /// <summary>
        /// The cooldown time between consecutive attacks.
        /// </summary>
        float ReloadSpeed { get; }

        /// <summary>
        /// The base damage inflicted by the attack.
        /// </summary>
        int Damage { get; }

        /// <summary>
        /// The line of sight required for detecting and targeting enemies.
        /// </summary>
        float LineOfSight { get; }
        
        /// <summary>
        /// Unit Types specified here are not valid targets for this attacker.
        /// </summary>
        UnitTypeSO[] InvalidTargetTypes { get; }

        /// <summary>
        /// Calculates the damage to be dealt to the specified target.
        /// </summary>
        /// <param name="target">The target entity receiving the attack.</param>
        /// <returns>The damage value to apply.</returns>
        int GetDamage(IEntity target);

    }

}