using UnityEngine;

namespace TRavljen.UnitSystem.Combat
{

    public interface IProjectileMovement
    {
        /// <summary>
        /// Initializes the movement logic with starting and target positions.
        /// </summary>
        /// <param name="owner">The owner of the projectile.</param>
        /// <param name="transform">The projectile's transform used for movement updates.</param>
        /// <param name="start">The starting position of the projectile.</param>
        /// <param name="target">The target position the projectile should try to reach.</param>
        void Start(UnitAttack owner, Transform transform, Vector3 start, Vector3 target);

        /// <summary>
        /// Updates the projectile's position and checks if it has reached the target.
        /// </summary>
        /// <param name="transform">The projectile's transform used for movement updates.</param>
        /// <param name="targetPosition">The current position of the target.</param>
        /// <param name="deltaTime">The time elapsed since the last update.</param>
        void Update(Transform transform, Vector3 targetPosition, float deltaTime);

        /// <summary>
        /// Determines if the projectile has reached its target.
        /// </summary>
        /// <returns>True if the target has been reached; otherwise, false.</returns>
        bool HasReachedTarget();
    }


}