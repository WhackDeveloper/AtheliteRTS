using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TRavljen.UnitSystem.Combat
{

    using UnityEngine;

    /// <summary>
    /// Handles arc-based projectile movement, where the projectile reaches a peak height at the midpoint
    /// before descending toward the target. The height and speed adjust dynamically based on distance.
    /// </summary>
    [System.Serializable]
    public class BallisticMovement : IProjectileMovement
    {

        #region Properties

        /// <summary>
        /// The minimum speed the projectile can have.
        /// </summary>
        [SerializeField]
        protected float minSpeed = 5f;

        /// <summary>
        /// The maximum speed the projectile can achieve.
        /// </summary>
        [SerializeField]
        protected float maxSpeed = 10f;

        [Header("Height Settings")]
        [SerializeField, Tooltip("The maximum height the projectile will reach at the midpoint of its trajectory.")]
        private float maxHeight = 5f;

        [Header("Speed Settings")]
        [SerializeField, Tooltip("Curve defining the projectile's velocity progression over time.")]
        private AnimationCurve speedCurve = AnimationCurve.Linear(0, 1, 1, 1);

        private float maxDistance = 50f;
        private Vector3 startPosition;
        private Vector3 targetPosition;
        private float totalDistance;
        private float progress = 0f;
        private float currentSpeed = 0f;

        #endregion

        /// <summary>
        /// Initializes the projectile's trajectory based on the start and target positions.
        /// Here the initial rotation will also be set for the projectile.
        /// </summary>
        /// <param name="transform">Transform of the projectile</param>
        /// <param name="start">The starting position of the projectile.</param>
        /// <param name="target">The target position the projectile will aim for.</param>
        public void Start(UnitAttack owner, Transform transform, Vector3 start, Vector3 target)
        {
            startPosition = start;
            targetPosition = target;
            totalDistance = Vector3.Distance(startPosition, targetPosition);
            maxDistance = owner.MaxInteractionRange;
            // Don't set 0 here, just start of with small percentage for better initial angle calculation.
            progress = 0.05f;

            UpdatePosition(transform, target, progress);
        }

        /// <summary>
        /// Updates the projectile's position, height, and rotation based on its trajectory progress.
        /// </summary>
        /// <param name="transform">The transform of the projectile.</param>
        /// <param name="targetPosition">The current target position (can move over time).</param>
        /// <param name="deltaTime">The time elapsed since the last update.</param>
        public void Update(Transform transform, Vector3 targetPosition, float deltaTime)
        {
            totalDistance = Vector3.Distance(startPosition, targetPosition);

            // Dynamically update speed
            UpdateSpeed(deltaTime);

            UpdatePosition(transform, targetPosition, progress);
        }

        /// <summary>
        /// Updates the projectile's speed based on the configured speed curve and trajectory progress.
        /// </summary>
        /// <param name="deltaTime">The time elapsed since the last update.</param>
        public void UpdateSpeed(float deltaTime)
        {
            currentSpeed = Mathf.Lerp(minSpeed, maxSpeed, speedCurve.Evaluate(progress));
            // Update trajectory progress
            progress += currentSpeed * deltaTime / totalDistance;
        }

        /// <summary>
        /// Determines whether the projectile has reached its target.
        /// </summary>
        /// <returns>True if the projectile has completed its trajectory, otherwise false.</returns>
        public bool HasReachedTarget() => progress >= 1f;

        #region Convenience

        private void UpdatePosition(Transform transform, Vector3 targetPosition, float progress)
        {
            // Get new position for the progress
            Vector3 newPosition = GetPosition(progress, targetPosition);

            // Rotate to face the new direction
            Quaternion targetRotation = GetRotation(transform.position, newPosition);

            // Apply position and rotation
            transform.SetPositionAndRotation(newPosition, targetRotation);
        }

        private Vector3 GetPosition(float progress, Vector3 targetPosition)
        {
            // Calculate horizontal position
            Vector3 horizontalPosition = startPosition + (targetPosition - startPosition) * progress;

            // Calculate height using a parabolic curve
            float heightFactor = 4 * AdjustedMaxHeight() * (progress - progress * progress);
            float baseHeight = Mathf.Lerp(startPosition.y, targetPosition.y, progress);
            float arcHeight = baseHeight + heightFactor;

            // Combine horizontal and height positions
            return new Vector3(horizontalPosition.x, arcHeight, horizontalPosition.z);
        }

        private Quaternion GetRotation(Vector3 start, Vector3 end)
        {
            return Quaternion.LookRotation((end - start).normalized);
        }

        /// <summary>
        /// Dynamically adjusts the maximum height based on the distance between the start and target positions.
        /// </summary>
        private float AdjustedMaxHeight()
        {
            float distanceFactor = Mathf.Clamp01(totalDistance / maxDistance);
            return maxHeight * distanceFactor; // Scale height proportionally to distance
        }

        #endregion
    }

}