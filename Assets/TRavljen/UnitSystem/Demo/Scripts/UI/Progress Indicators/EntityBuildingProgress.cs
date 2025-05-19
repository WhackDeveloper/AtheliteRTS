namespace TRavljen.UnitSystem.Demo
{

    using Build;
    
    /// <summary>
    /// A component for displaying a progress bar that visually represents the building progress of an entity.
    /// </summary>
    public class EntityBuildingProgress: AEntityProgressIndicator
    {

        /// <summary>
        /// The UnitBuilding component associated with this progress bar.
        /// </summary>
        private EntityBuilding _entityBuilding;

        protected override void Awake()
        {
            base.Awake();

            // Attempt to find the UnitBuilding component if not assigned
            if (_entityBuilding == null)
                _entityBuilding = entity.GetComponent<EntityBuilding>();
        }

        protected override void UpdateBar()
        {
            // Update progress internally
            Progress = _entityBuilding.Progress;

            base.UpdateBar();
        }

        protected override bool ShouldShowProgressIndicator()
        {
            // Ensure the UnitBuilding component is valid and active
            if (!(_entityBuilding != null || entity.TryGetComponent(out _entityBuilding)))
                return false;

            if (!_entityBuilding.IsActive || entity.Health.IsDepleted)
                return false;

            // Only show the progress bar if construction is incomplete
            return _entityBuilding.Progress < 1;
        }
    }

}