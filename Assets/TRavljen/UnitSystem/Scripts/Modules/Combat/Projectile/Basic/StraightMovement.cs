using UnityEngine;

namespace TRavljen.UnitSystem.Combat
{

    /// <summary>
    /// Direct movement with speed defined by a curve.
    /// </summary>
    [System.Serializable]
    public class StraightMovement : IProjectileMovement
    {

        #region Properties

        [Header("Speed Settings")]
        [SerializeField, Tooltip("Curve defining the projectile's velocity progression over time.")]
        private AnimationCurve speedCurve = AnimationCurve.Linear(0, 1, 1, 1);

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

        private float progress;
        private float totalDistance;
        private float currentSpeed;

        #endregion

        /// <inheritdoc/>
        public void Start(UnitAttack owner, Transform transform, Vector3 start, Vector3 target)
        {
            currentSpeed = GetCurrentSpeed();
            totalDistance = Vector3.Distance(start, target);
        }

        /// <inheritdoc/>
        public void Update(Transform transform, Vector3 targetPosition, float deltaTime)
        {
            //totalDistance = Vector3.Distance(startPosition, targetPosition);
            progress += currentSpeed * deltaTime / totalDistance;
            currentSpeed = GetCurrentSpeed();

            // Move towards the target position
            Vector3 direction = (targetPosition - transform.position).normalized;
            transform.Translate(currentSpeed * deltaTime * direction, Space.World);

            // Rotate the projectile to face the target
            Quaternion targetRotation = Quaternion.LookRotation(direction);
            transform.rotation = targetRotation;
        }

        public bool HasReachedTarget() => progress >= 1;

        private float GetCurrentSpeed()
            => Mathf.Lerp(minSpeed, maxSpeed, speedCurve.Evaluate(progress));

    }

}