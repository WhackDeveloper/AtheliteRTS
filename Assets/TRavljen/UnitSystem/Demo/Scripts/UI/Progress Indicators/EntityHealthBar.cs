using TRavljen.UnitSystem.Combat;

namespace TRavljen.UnitSystem.Demo
{
   
    /// <summary>
    /// Component for handling progress indicator for health. When health is set and below max health
    /// it will show the progress canvas and update its state.
    /// </summary>
    public class EntityHealthBar : AEntityProgressIndicator
    {
        /// <summary>
        /// The health component associated with this health bar.
        /// </summary>
        private Health health;

        protected override void Awake()
        {
            base.Awake();

            // Attempt to find the Health component if not assigned
            if (health == null)
                health = entity.GetComponent<Health>();
        }

        protected override void UpdateBar()
        {
            // Update progress internally
            Progress = health.CurrentHealthPercentage;

            base.UpdateBar();
        }

        protected override bool ShouldShowProgressIndicator()
        {
            // Hide if health is missing
            if (health == null) return false;
            return health.CurrentHealthPercentage < 1;
        }
    }
    
}