using UnityEngine;

namespace TRavljen.UnitSystem.Combat
{

    /// <summary>
    /// Defines the basic functionality of a projectile.
    /// Implementations specify how projectiles behave upon being launched.
    /// </summary>
    public interface IProjectile
    {
        /// <summary>
        /// Launches the projectile towards a specified target.
        /// </summary>
        /// <param name="owner">The unit initiating the projectile.</param>
        /// <param name="health">The target's health component.</param>
        /// <param name="startPosition">The position from which the projectile is launched.</param>
        /// <param name="damage">The damage dealt by the projectile upon hitting the target.</param>
        void LaunchTowards(UnitAttack owner, IHealth health, Vector3 startPosition, int damage);

        /// <summary>
        /// Destroys the projectile immediately.
        /// </summary>
        void Destroy();
    }

}