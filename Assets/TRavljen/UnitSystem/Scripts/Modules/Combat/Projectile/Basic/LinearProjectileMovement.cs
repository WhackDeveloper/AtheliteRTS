using UnityEngine;

namespace TRavljen.UnitSystem.Combat
{

    /// <summary>
    /// Handles linear movement for a projectile, ensuring it moves directly toward a target at a constant speed.
    /// </summary>
    [System.Serializable]
    public class LinearProjectileMovement : IProjectileMovement
    {
        /// <summary>
        /// The speed at which the projectile moves toward the target, in units per second.
        /// </summary>
        [SerializeField]
        private float speed = 10f;

        private bool reachedTarget;

        /// <inheritdoc/>
        public void Start(UnitAttack owner, Transform transform, Vector3 start, Vector3 target)
        {
            reachedTarget = false;
        }

        /// <summary>
        /// Updates the projectile's position, moving it toward the target.
        /// Ensures the projectile doesn't overshoot the target.
        /// </summary>
        /// <param name="transform">The transform of the projectile.</param>
        /// <param name="targetPosition">The current target position (if dynamic).</param>
        /// <param name="deltaTime">The time elapsed since the last update.</param>
        public void Update(Transform transform, Vector3 targetPosition, float deltaTime)
        {
            // Calculate direction toward the target.
            Vector3 direction = (targetPosition - transform.position).normalized;

            // Calculate the maximum distance the projectile can travel this frame.
            float maxDistanceThisFrame = speed * deltaTime;

            // Calculate the remaining distance to the target.
            float remainingDistance = Vector3.Distance(transform.position, targetPosition);

            // Move the projectile by the smaller of the two distances.
            float travelDistance = Mathf.Min(maxDistanceThisFrame, remainingDistance);
            transform.position += direction * travelDistance;

            // Mark as reached if the projectile is very close to the target.
            if (remainingDistance <= 0.1f)
            {
                reachedTarget = true;
            }

            transform.LookAt(targetPosition);
        }

        /// <summary>
        /// Checks if the projectile has reached the target.
        /// </summary>
        /// <returns>True if the projectile is close enough to the target; otherwise, false.</returns>
        public bool HasReachedTarget() => reachedTarget;
    }

}