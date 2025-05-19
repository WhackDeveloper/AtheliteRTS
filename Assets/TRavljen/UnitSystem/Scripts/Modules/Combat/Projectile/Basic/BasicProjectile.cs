using TRavljen.EditorUtility;
using UnityEngine;

namespace TRavljen.UnitSystem.Combat
{

    /// <summary>
    /// A basic implementation of the <see cref="IProjectile"/> interface.
    /// Handles launching, movement, and collision behavior for a projectile.
    /// </summary>
    public class BasicProjectile : MonoBehaviour, IProjectile
    {

        /// <summary>
        /// Defines behaviors when the target becomes invalid during flight.
        /// </summary>
        private enum TargetInvalidBehaviour
        {
            /// <summary>
            /// Ignores the invalid target and waits for the projectile's lifetime to expire.
            /// </summary>
            Ignore,

            /// <summary>
            /// Destroys the projectile immediately if the target is invalid.
            /// </summary>
            DestroyProjectileImmediately,

            /// <summary>
            /// Continues to the target's last known position, then destroys the projectile.
            /// </summary>
            DestroyOnLastKnownTargetPosition
        }

        /// <summary>
        /// Defines the types of projectile movements.
        /// </summary>
        private enum ProjectileType
        {
            /// <summary>
            /// Movement in a straight line towards the target.
            /// </summary>
            Straight,

            /// <summary>
            /// Ballistic movement with an arc.
            /// </summary>
            Ballistic,

            /// <summary>
            /// Linear movement at a constant speed, regardless of acceleration or other forces.
            /// </summary>
            Linear,

            /// <summary>
            /// Movement that completes its trajectory within a fixed duration, 
            /// independent of the distance to the target.
            /// </summary>
            Timed

        }

        #region Fields

        [Tooltip("Specifies if the gizmo for direction of the projectile will be rendered.")]
        [SerializeField]
        private bool showDirectionalGizmo = false;

        /// <summary>
        /// Whether the projectile should actively follow the target during its flight.
        /// </summary>
        [Tooltip("If true, the projectile will actively follow its target during flight.")]
        [SerializeField]
        private bool followTarget = false;

        /// <summary>
        /// Indicates whether the projectile has been launched.
        /// </summary>
        [Tooltip("Tracks whether the projectile has been launched.")]
        [SerializeField, DisableInInspector]
        private bool launched = false;

        /// <summary>
        /// The maximum lifetime of the projectile in seconds.
        /// </summary>
        [Tooltip("The duration (in seconds) before the projectile is automatically destroyed.")]
        [SerializeField]
        private float lifeDuration = 5;

        /// <summary>
        /// Whether the projectile ignores obstacles when checking for collisions.
        /// </summary>
        [Tooltip("If true, the projectile ignores obstacles when performing collision checks.")]
        [SerializeField]
        private bool ignoreObstacles = true;

        /// <summary>
        /// Specifies which layers are considered obstacles for collision detection.
        /// </summary>
        [Tooltip("Defines the layers that the projectile considers obstacles during collision checks.")]
        [SerializeField]
        private LayerMask obstacleLayer;

        /// <summary>
        /// Whether the projectile should use the target's layer for collision detection.
        /// This does not include obstacle collision detection.
        /// </summary>
        [Tooltip("If true, the projectile will use the target's layer for collision checks.")]
        [SerializeField]
        private bool readLayerFromTarget = false;

        /// <summary>
        /// The layer mask used to identify valid targets for the projectile.
        /// </summary>
        [Tooltip("Specifies the layers that define valid targets for the projectile.")]
        [SerializeField]
        private LayerMask targetLayer;

        /// <summary>
        /// Specifies if damage is applied on projectile start (launch) or impact (target position).
        /// </summary>
        [Tooltip("Specifies if damage is applied on projectile start (launch) or impact (target position).")]
        [SerializeField]
        private bool damagesOnStart = false;

        /// <summary>
        /// Behavior to apply when the target becomes invalid (e.g., destroyed or out of range).
        /// </summary>
        [Tooltip("Specifies how the projectile should behave when its target becomes invalid.")]
        [SerializeField]
        TargetInvalidBehaviour invalidTargetBehaviour = TargetInvalidBehaviour.Ignore;

        /// <summary>
        /// Specifies the distance threshold at which the projectile considers itself to have reached the 
        /// last known target position after the target becomes invalid (e.g., destroyed or missing). 
        /// This is only applicable when <see cref="invalidTargetBehaviour"/> is set to 
        /// <see cref="TargetInvalidBehaviour.DestroyOnLastKnownTargetPosition"/>.
        /// </summary>
        [SerializeField]
        [Tooltip("Specifies the distance threshold at which the projectile considers " +
            "itself to have reached the last known target position after the target " +
            "becomes invalid (e.g., destroyed or missing). ")]
        private float targetLostProximityThreshold = 1f;

        /// <summary>
        /// Defines the movement type of the projectile (e.g., straight or ballistic).
        /// </summary>
        [Tooltip("The type of movement for the projectile (e.g., straight or ballistic).")]
        [SerializeField]
        private ProjectileType type = ProjectileType.Straight;

        /// <summary>
        /// Specifies the vertical offset of target position.
        /// </summary>
        [Tooltip("Specifies the vertical offset of target position.")]
        [SerializeField]
        private float targetVerticalOffset = 1;

        /// <summary>
        /// The GameObject that defines the movement behavior for the projectile.
        /// Must implement <see cref="IProjectileMovement"/>.
        /// </summary>
        [Tooltip("The GameObject that implements the movement logic for the projectile. Must implement IProjectileMovement.")]
        [SerializeField, RequiresType(typeof(IProjectileMovement))]
        private GameObject movementObject;

        /// <summary>
        /// The movement logic used to control the projectile's behavior.
        /// </summary>
        [Tooltip("The movement logic controlling the projectile's behavior. Defaults to a ballistic movement.")]
        [SerializeReference]
        private IProjectileMovement movement = new BallisticMovement();

        #endregion

        #region Properties

        private int damage;
        private IHealth targetHealth;
        private Transform myTransform;
        private Transform healthTransform;
        private UnitAttack owner;
        private Vector3 targetPosition;

        #endregion

        #region IProjectile Implementation

         /// <inheritdoc />
        void IProjectile.LaunchTowards(UnitAttack owner, IHealth health, Vector3 startPosition, int damage)
        {
            this.owner = owner;
            this.damage = damage;
            targetHealth = health;

            launched = true;

            if (readLayerFromTarget)
                targetLayer = 1 << health.transform.gameObject.layer;

            myTransform = transform;
            healthTransform = health.transform;

            myTransform.position = startPosition;
            targetPosition = healthTransform.position + Vector3.up * targetVerticalOffset;
            
            Vector3 direction = targetPosition - startPosition;
            myTransform.SetPositionAndRotation(startPosition, Quaternion.LookRotation(direction));

            movement.Start(owner, myTransform, startPosition, targetPosition);

            if (damagesOnStart)
            {
                health.Damage(owner, damage);
                GameObject.Destroy(gameObject);
            }
        }

        /// <inheritdoc />
        void IProjectile.Destroy()
        {
            GameObject.Destroy(gameObject);
        }

        #endregion

        #region Lifecycle

        private void OnValidate()
        {
            switch (type)
            {
                case ProjectileType.Straight:
                    if (movement is not StraightMovement)
                        movement = new StraightMovement();
                    break;

                case ProjectileType.Ballistic:
                    if (movement is not BallisticMovement)
                        movement = new BallisticMovement();
                    break;

                case ProjectileType.Linear:
                    if (movement is not LinearProjectileMovement)
                        movement = new LinearProjectileMovement();
                    break;

                case ProjectileType.Timed:
                    if (movement is not TimedDirectMovement)
                        movement = new TimedDirectMovement();
                    break;
            }
        }

        private void Update()
        {
            PerformUpdate(Time.deltaTime);
        }

        private void FixedUpdate()
        {
            bool isTargetValid = !targetHealth.IsNull() && targetHealth.IsActive;

            // Check for collision only when target is still valid
            if (isTargetValid && lifeDuration > 0)
            {
                // If ignoring obstacles, check collision directly
                if (ignoreObstacles)
                    CollisionCheck();
                // Check collisions and only if no hit, check obstacles.
                else if (!CollisionCheck())
                {
                    // When there was no collision, continue with obstacle check
                    ObstacleCheck();
                }
            }
        }

        #endregion

        #region Update

        private void PerformUpdate(float time)
        {
            if (!launched) return;

            bool isTargetValid = !targetHealth.IsNull() && targetHealth.IsActive;

            // When following & target dies, continue to its last position.
            if (followTarget && isTargetValid)
            {
                targetPosition = healthTransform.position + Vector3.up * targetVerticalOffset;
            }

            movement.Update(myTransform, targetPosition, time);
            lifeDuration -= time;

            if (!isTargetValid)
            {
                switch (invalidTargetBehaviour)
                {
                    case TargetInvalidBehaviour.Ignore: break;
                    case TargetInvalidBehaviour.DestroyProjectileImmediately:
                        lifeDuration = 0;
                        break;
                    case TargetInvalidBehaviour.DestroyOnLastKnownTargetPosition:
                        if (movement.HasReachedTarget() || Vector3.Distance(myTransform.position, targetPosition) < targetLostProximityThreshold)
                            lifeDuration = 0;
                        break;
                }
            }

            // Destroy game object once life expires.
            if (lifeDuration <= 0)
            {
                enabled = false;
                GameObject.Destroy(gameObject);
            }
        }

        #endregion

        #region Collision

        private bool CollisionCheck()
        {
            Collider[] hits = Physics.OverlapSphere(transform.position, 0.5f, targetLayer);
            foreach (var collider in hits)
            {
                // Check if collider has correct health component
                if (!collider.TryGetComponent(out IHealth health) ||
                    health != targetHealth)
                    continue;

                health.Damage(owner, damage);
                GameObject.Destroy(gameObject);
                return true;
            }

            return false;
        }

        private bool ObstacleCheck()
        {
            Collider[] hits = Physics.OverlapSphere(transform.position, 0.5f, obstacleLayer);

            foreach (var collider in hits)
            {
                if (collider.gameObject == gameObject)
                    continue;

                enabled = false;
                GameObject.Destroy(gameObject);
                return true;
            }

            return false;
        }

        #endregion

        private void OnDrawGizmos()
        {
            if (showDirectionalGizmo)
            {
                Gizmos.color = Color.red;
                Gizmos.DrawLine(transform.position, transform.position + transform.forward * 2);
            }
        }

    }

}