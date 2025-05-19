using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TRavljen.UnitSystem.Combat
{

    /// <summary>
    /// Handles launching projectiles from a unit, integrating with the UnitAttack component.
    /// </summary>
    public class ProjectileLauncher : AUnitComponent
    {
        #region Fields

        [Header("Projectile Settings")]

        /// <summary>
        /// The local starting position offset for launched projectiles.
        /// </summary>
        [Tooltip("The local position offset from which projectiles will be launched.")]
        [SerializeField]
        private Vector3 projectileStart;

        /// <summary>
        /// Whether to use a specific Transform as the starting position for projectiles.
        /// </summary>
        [Tooltip("If true, uses the specified Transform's position as the launch point.")]
        [SerializeField]
        private bool useTransformForStart = false;

        /// <summary>
        /// The Transform to use as the starting position for projectiles, if enabled.
        /// </summary>
        [Tooltip("The Transform used as the starting position for projectiles.")]
        [SerializeField]
        private Transform startTransform;

        /// <summary>
        /// The prefab for the projectile to be instantiated.
        /// Must implement <see cref="IProjectile"/>.
        /// </summary>
        [Tooltip("The prefab to instantiate for each projectile. Must implement IProjectile.")]
        [SerializeField, RequiresType(typeof(IProjectile))]
        private GameObject projectilePrefab;

        [Tooltip("By default this is disabled and projectile will be launched when " +
            "unit attacks. If disabled, you can manually control when and where the " +
            "projectile is launched.")]
        [SerializeField]
        private bool launchManually = false;

        /// <summary>
        /// Whether to destroy active projectiles when the unit is destroyed.
        /// </summary>
        [Tooltip("If true, all active projectiles will be destroyed when the unit is destroyed.")]
        [SerializeField]
        private bool destroyProjectilesWithUnit = false;

        /// <summary>
        /// The UnitAttack component for triggering projectile launches.
        /// </summary>
        private UnitAttack attack;

        public UnitAttack Attack => attack;

        /// <summary>
        /// Tracks currently active projectiles for cleanup or additional control.
        /// </summary>
        private readonly List<IProjectile> activeProjectiles = new();

        #endregion

        #region Initialization

        protected override void OnInitialize()
        {
            base.OnInitialize();

            attack = Unit.GetComponent<UnitAttack>();
            attack.ManuallyTriggerAttack = true;
        }

        private void Start()
        {
            if (destroyProjectilesWithUnit)
            {
                Unit.OnDestroy.AddListener(DestroyProjectiles);
            }
        }

        #endregion

        #region Event Handling

        private void OnEnable()
        {
            if (Attack && !launchManually)
                Attack.OnAttack.AddListener(OnAttack);
        }

        private void OnDisable()
        {
            if (Attack)
                Attack.OnAttack.RemoveListener(OnAttack);
        }

        private void OnAttack(UnitAttack unit, IHealth target, int damage)
        {
            if (launchManually)
            {
                if (attack != null)
                    attack.OnAttack.RemoveListener(OnAttack);
                return;
            }

            LaunchProjectile(unit, target, damage);
        }

        public void LaunchProjectile(UnitAttack attacker, IHealth target, int damage)
        {
            // Instantiate and configure the projectile
            GameObject newProjectileObject = Instantiate(projectilePrefab);

            if (newProjectileObject.TryGetComponent(out IProjectile projectile))
            {
                projectile.LaunchTowards(attacker, target, GetStartingPosition(), damage);
                activeProjectiles.Add(projectile);
            }
            else
            {
                Debug.LogError("Projectile prefab does not implement IProjectile.");
            }
        }

        #endregion

        #region Utility Methods

        /// <summary>
        /// Cleans up all active projectiles.
        /// </summary>
        public void DestroyProjectiles()
        {
            foreach (IProjectile projectile in activeProjectiles)
            {
                projectile.Destroy();
            }

            activeProjectiles.Clear();
        }

        private void DestroyProjectiles(Entity _) => DestroyProjectiles();

        /// <summary>
        /// Calculates the starting position for launching projectiles.
        /// </summary>
        /// <returns>The world-space position for the projectile start.</returns>
        private Vector3 GetStartingPosition()
        {
            if (useTransformForStart && startTransform != null)
            {
                return startTransform.position;
            }

            return transform.position + transform.rotation * projectileStart;
        }

        #endregion

        #region Editor Gizmos

        private void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.green;
            Gizmos.DrawSphere(GetStartingPosition(), 0.5f);
        }

        #endregion
    }


}