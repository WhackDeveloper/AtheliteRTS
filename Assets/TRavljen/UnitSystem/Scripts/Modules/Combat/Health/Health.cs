using System.Collections;
using TRavljen.EditorUtility;
using UnityEngine;
using UnityEngine.Events;

namespace TRavljen.UnitSystem.Combat
{

    /// <summary>
    /// Handles the health system of an entity, including damage, healing, and health events.
    /// Implements invulnerability, health depletion logic, and optional destruction upon depletion.
    /// This will also manage self regeneration when enabled.
    /// </summary>
    [DisallowMultipleComponent]
    public class Health : AEntityComponent, IHealth
    {
        
        #region Properties

        [SerializeField]
        [Tooltip("If true, destroys the entity when its health is depleted.")]
        private bool destroyWhenDepleted = true;

        [Tooltip("Specifies the delay in seconds before the unit starts regenerating it's health." +
            "This activity will only start if regeneration is set to a positive number")]
        [SerializeField]
        private float regenerationDelay = 20;

        [Tooltip("The interval between heals when regeneration is active (after initial delay).")]
        [SerializeField]
        private float regenerationInterval = 1;

        [Header("Events")]
        [Tooltip("Event invoked when health is decreased.")]
        [SerializeField]
        private UnityEvent<IHealth, UnitAttack, int> onHitpointDecreased = new();

        [Tooltip("Event invoked when health is increased.")]
        [SerializeField]
        private UnityEvent<IHealth, IEntity, int> onHitpointIncreased = new();

        [Tooltip("Event invoked when health is depleted.")]
        [SerializeField]
        private UnityEvent<IHealth, UnitAttack> onHealthDepleted = new();

        [Tooltip("Event invoked when health is set, not damaged or healed.")]
        [SerializeField]
        private UnityEvent<IHealth, int> onHealthSet = new();

        // For display primarily, can be changed in Editor Inspector when in Debug view.
        [SerializeField]
        [Tooltip("The current health of the entity.")]
        [DisableInInspector]
        private int currentHealth = 100;

        public UnityEvent<IHealth, UnitAttack, int> OnHitpointDecreased => onHitpointDecreased;
        public UnityEvent<IHealth, IEntity, int> OnHitpointIncreased => onHitpointIncreased;
        public UnityEvent<IHealth, UnitAttack> OnHealthDepleted => onHealthDepleted;
        public UnityEvent<IHealth, int> OnHealthSet => onHealthSet;

        private int maxHealth = 100;
        private bool canDecrease = true;
        private bool canIncrease = false;
        private bool isDepleted = false;
        private int regeneration;
        private Coroutine regenerationTask;
        
        public bool IsDepleted => isDepleted == true;
        public bool IsInvulnerable => !canDecrease;
        public int Regenerates => regeneration;

        public float CurrentHealthPercentage => currentHealth / (float)maxHealth;
        public int CurrentHealth => currentHealth;
        public int MaxHealth => maxHealth;

        public Vector3 Position => transform.position;

        #endregion

        #region Lifecycle

        protected override void OnInitialize()
        {
            base.OnInitialize();

            if (Entity.TryGetCapability(out IHealthCapability health))
            {
                currentHealth = maxHealth = health.HealthPoints;
                canDecrease = health.CanDecrease;
                regeneration = health.Regeneration;
                canIncrease = health.CanIncrease;
            }
            else
            {
                Debug.LogError($"Unable to set up Health, missing IHealthCapability on Entity: {gameObject.name}");
            }
        }

        /// <summary>
        /// Ensures current health is clamped to the maximum value during editing.
        /// </summary>
        private void OnValidate()
        {
            currentHealth = Mathf.Clamp(currentHealth, 0, maxHealth);
        }

        #endregion

        #region Destruction

        /// <summary>
        /// Destroys the entire entity GameObject.
        /// </summary>
        public void DestroyGameObject() => Destroy(gameObject);

        /// <summary>
        /// Removes the health component from the entity.
        /// </summary>
        public void DestroyScript() => Destroy(this);

        #endregion

        #region Health Modification

        public void SetCurrentHealth(int amount)
        {
            currentHealth = amount;
            onHealthSet.Invoke(this, amount);
        }

        /// <inheritdoc/>
        public void Damage(UnitAttack attacker, int damage)
        {
            // Check if health can receive take damage
            if (IsInvulnerable || IsDepleted) return;

            // Ensure damage is non-negative
            damage = Mathf.Max(damage, 0);
            currentHealth -= damage;

            bool healthDepleted = currentHealth <= 0;

            if (healthDepleted)
            {
                // Cap damage to remaining health
                damage += currentHealth;
                currentHealth = 0;
                // Update this before invoking any events
                isDepleted = true;
            }

            // Invoke damage events
            onHitpointDecreased.Invoke(this, attacker, damage);
            CombatEvents.Instance.OnHitpointDecreased.Invoke(this, attacker, damage);

            if (regenerationTask != null)
                StopCoroutine(regenerationTask);

            if (!isDepleted && regeneration > 0)
            {
                regenerationTask = StartCoroutine(StartRegeneration());
            }

            if (healthDepleted)
            {
                // Cache owner reference in case it's cleared after depletion
                APlayer owner = Entity.Owner;

                // Invoke depletion events
                onHealthDepleted.Invoke(this, attacker);
                CombatEvents.Instance.OnHealthDepleted.Invoke(this, owner, attacker);

                if (destroyWhenDepleted)
                {
                    Entity.DestroyEntity();
                }
            }
        }

        /// <inheritdoc/>
        public void Heal(IEntity entity, int amount)
        {
            if (!canIncrease) return;

            // Ensure healing amount is non-negative
            amount = Mathf.Max(amount, 0);

            if (amount == 0) return;

            currentHealth = Mathf.Clamp(currentHealth + amount, 0, maxHealth);
            onHitpointIncreased.Invoke(this, entity, amount);
        }

        #endregion

        #region Regeneration

        private IEnumerator StartRegeneration()
        {
            yield return new WaitForSeconds(regenerationDelay);

            while (currentHealth < maxHealth && !isDepleted)
            {
                currentHealth = Mathf.Clamp(currentHealth + regeneration, 0, maxHealth);

                onHitpointIncreased.Invoke(this, null, regeneration);
                CombatEvents.Instance.OnHitpointIncreased.Invoke(this, null, regeneration);

                yield return new WaitForSeconds(regenerationInterval);
            }
        }

        #endregion
    }

}