namespace TRavljen.UnitSystem.Build
{
    using UnityEngine;
    using UnityEngine.Events;
    using Interactions;
    using TRavljen.UnitSystem.Combat;

    /// <summary>
    /// Manages the building process for a unit. Tracks build progress, handles build events,
    /// and integrates with unit capabilities to determine build behavior.
    /// </summary>
    /// <remarks>
    /// Building process can start once this component Starts.
    /// </remarks>
    [DisallowMultipleComponent]
    public class EntityBuilding : ALimitedInteractionTarget, IBuildableEntity, IUnitInteractingPosition
    {

        #region Fields and Properties

        [Tooltip("Specifies if the building process should start automatically on " +
            "component Start or will it be manually invoked.")]
        [SerializeField]
        private bool manuallyStartBuild = false;

        [Tooltip("Value used when building process is using Health for construction. " +
            "This will be its initial value.")]
        [SerializeField]
        private int startingHealth = 1;

        /// <summary>
        /// Event invoked when the building progress updates.
        /// Parameter: Current build progress as a float between 0 and 1.
        /// </summary>
        [Header("Events")]
        public UnityEvent<float> OnBuildProgress = new();

        /// <summary>
        /// Event invoked when the building process starts.
        /// </summary>
        public UnityEvent OnBuildStarted = new();

        /// <summary>
        /// Event invoked when the building process is completed.
        /// </summary>
        public UnityEvent OnBuildCompleted = new();

        /// <summary>
        /// Event invoked when the building process is canceled before completion.
        /// </summary>
        public UnityEvent OnBuildCanceled = new();

        /// <inheritdoc />
        public float Progress { get; private set; } = 0;

        public bool BuildAutomatically => buildable.AutoBuild;

        /// <summary>
        /// Indicates whether the unit is fully built and operational.
        /// </summary>
        public bool IsBuilt => Entity.IsOperational;

        /// <summary>
        /// Position provider reference. This is an optional provider.
        /// </summary>
        public IPredefinedInteractionPositionProvider PredefinedPositionProvider;

        private IBuildableCapability buildable;
        private bool buildStarted = false;
        private float currentTime = 0;
        private float durationTime = 0;

        private bool useHealth = false;
        private int constructedHealth = 0;
        private int maxHealth = 0;
        private IHealth health;

        #endregion

        #region Lifecycle

        /// <summary>
        /// Initializes the component. Verifies that the associated entity is a valid <see cref="Unit"/>
        /// and that the unit supports building capabilities.
        /// </summary>
        protected override void OnInitialize()
        {
            base.OnInitialize();

            if (!Entity.TryGetCapability(out buildable))
            {
                Debug.LogError($"Missing IBuildableCapability capability on the Entity {name}.");
                return;
            }

            useHealth = buildable.UsesHealth;

            PredefinedPositionProvider = GetComponent<IPredefinedInteractionPositionProvider>();
        }

        private void Start()
        {
            // Start building based on configuration
            if (!manuallyStartBuild)
            {
                StartBuilding();
            }
        }

        private void OnDisable()
        {
            if (IsBuilt) return;

            // Notify about cancelation when the building process has not been completed.
            OnBuildCanceled.Invoke();
            BuildEvents.Instance.OnBuildCanceled.Invoke(this);
        }

        private void Update()
        {
            if (BuildAutomatically)
            {
                UpdateBuildProcess(Time.deltaTime);
            }
        }

        #endregion

        #region IUnitInteractingPosition

        public virtual Vector3 GetAvailableInteractionPosition(IUnitInteractorComponent interactor, bool reserve)
        {
            // Check for position provider on unit itself
            if (PredefinedPositionProvider != null)
                return PredefinedPositionProvider.GetAvailableInteractionPosition(interactor, this, reserve);

            // Calculate fit position
            var (position, _) = InteractorPositionHelper.CalculateInteractionPositionAndDirection(interactor, this);
            return position;
        }

        public virtual bool ReleaseInteractionPosition(IUnitInteractorComponent interactor)
        {
            // If position provider is set, remove interaction position
            return PredefinedPositionProvider?.ReleaseInteractionPosition(interactor, this) ?? true;
        }

        #endregion

        #region Building Process

        /// <summary>
        /// Starts the building process for the unit. This is called automatically if
        /// <see cref="manuallyStartBuild"/> is set to false.
        /// </summary>
        public void StartBuilding()
        {
            // Check if production already started.
            if (buildStarted) return;

            buildStarted = true;
            durationTime = Entity.Data.ProductionDuration;

            if (useHealth && Entity.TryGetEntityComponent(out health))
            {
                health.SetCurrentHealth(startingHealth);
                constructedHealth = startingHealth;
                maxHealth = Mathf.Max(health.MaxHealth, 1);
            }
            else
            {
                Debug.LogWarning("Use health is enabled on Unit Building but no IHealth component is present on the Unit! Disabling this feature.");
                // Disable if no health
                useHealth = false;
            }

            OnBuildStarted.Invoke();
            BuildEvents.Instance.OnBuildStarted.Invoke(this);
        }

        public void BuildWithPower(float power)
        {
            UpdateBuildProcess(power / 100);
        }

        private void UpdateBuildProcess(float power)
        {
            // Convert power into time progression
            currentTime += power;

            if (useHealth)
            {
                UpdateHealth();
            }

            // Progress is now time-based, not just health-based
            Progress = Mathf.Clamp01(currentTime / durationTime);

            ProgressDidUpdate();
        }

        private void UpdateHealth()
        {
            // Calculate target health based on duration
            var targetHealth = Mathf.Lerp(startingHealth, maxHealth, currentTime / durationTime);

            // Ensure we only apply the difference in health since last frame
            var newHealth = Mathf.FloorToInt(targetHealth);
            var healthToAdd = newHealth - constructedHealth;

            if (healthToAdd <= 0) return;
            
            health.Heal(null, healthToAdd);
            constructedHealth = newHealth;
        }

        private void ProgressDidUpdate()
        {
            OnBuildProgress.Invoke(Progress);
            BuildEvents.Instance.OnBuildUpdated.Invoke(this, Progress);

            if (Progress >= 1)
            {
                FinishBuilding();
            }
        }

        /// <summary>
        /// Instantly finishes the building process and marks the unit as operational.
        /// </summary>
        public void FinishBuilding()
        {
            Entity.SetOperational(true);
            OnBuildCompleted.Invoke();
            BuildEvents.Instance.OnBuildCompleted.Invoke(this);

            // Destroy the component as the building process is complete.
            Destroy(this);
        }

        #endregion
    }

}