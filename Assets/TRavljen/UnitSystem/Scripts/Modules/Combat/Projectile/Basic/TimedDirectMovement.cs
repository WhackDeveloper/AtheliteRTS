using UnityEngine;

namespace TRavljen.UnitSystem.Combat
{

    /// <summary>
    /// Implements projectile movement over a fixed duration, 
    /// interpolating between the start and target positions.
    /// This movement type ensures the projectile reaches its destination 
    /// at a consistent pace regardless of the distance to the target.
    /// </summary>
    public class TimedDirectMovement : IProjectileMovement
    {

        [SerializeField]
        [Tooltip("The total time (in seconds) it takes for the projectile to reach its target.")]
        private float duration = 3;

        private Vector3 startPos;
        private float elapsedTime = 0;
        private bool reached = false;

        public bool HasReachedTarget() => reached;

        public void Start(UnitAttack owner, Transform transform, Vector3 start, Vector3 target)
        {
            startPos = start;
            transform.SetPositionAndRotation(start, Quaternion.Euler(start - target));
        }

        public void Update(Transform transform, Vector3 targetPosition, float deltaTime)
        {
            elapsedTime += deltaTime;

            if (elapsedTime >= duration)
            {
                elapsedTime = duration;
                reached = true;
            }

            float progress = elapsedTime / duration; // Normalized progress (0 to 1)
            Vector3 position = Vector3.Lerp(startPos, targetPosition, progress);

            Quaternion rotation = Quaternion.LookRotation(targetPosition - transform.position);
            transform.SetPositionAndRotation(position, rotation);
        }
    }

}