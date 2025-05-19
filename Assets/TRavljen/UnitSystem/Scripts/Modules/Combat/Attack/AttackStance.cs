namespace TRavljen.UnitSystem.Combat
{

    /// <summary>
    /// Determines the unit behaviour managed by attack tasks.
    /// </summary>
    public enum AttackStance
    {
        /// <summary>
        /// Aggressive and default stance. Unit will engage any enemy within Line of Sight
        /// and follow until they are killed.
        /// </summary>
        Aggressive,
        /// <summary>
        /// Unit will attack enemies within range, will follow certain distance
        /// before returning to their position.
        /// </summary>
        Defensive,
        /// <summary>
        /// Units do not move, but will attack any enemy in their attack range.
        /// </summary>
        StandGround,
        /// <summary>
        /// Units do not attack on their own.
        /// </summary>
        NoAttack
    }

}