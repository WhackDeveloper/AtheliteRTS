namespace TRavljen.UnitSystem.Build
{

    using UnityEngine.Events;

    /// <summary>
    /// Centralized event manager for handling the build process of units, 
    /// including notifications for starting, updating, canceling, and completing builds.
    /// </summary>
    public sealed class BuildEvents
    {
        /// <summary>
        /// Singleton instance of the BuildEvents class, 
        /// ensuring a single point for managing build-related events.
        /// </summary>
        public static readonly BuildEvents Instance = new();

        /// <summary>
        /// Private constructor to enforce the singleton pattern.
        /// </summary>
        private BuildEvents() { }

        /// <summary>
        /// Invoked when a builder finishes their assigned task for building an entity.
        /// This can be invoked duo to target built, destroyed or fails to start building process.
        /// </summary>
        /// <remarks>
        /// Parameters:
        /// - <see cref="Builder"/>: The builder that completed the task.
        /// - <see cref="Entity"/>: The entity that was successfully built.
        /// </remarks>
        public UnityEvent<Builder, IEntity> OnBuilderFinished = new();

        /// <summary>
        /// Invoked when the building process for an entity is fully completed.
        /// </summary>
        /// <remarks>
        /// Parameter:
        /// - <see cref="EntityBuilding"/>: The entity building that has been fully built.
        /// </remarks>
        public UnityEvent<EntityBuilding> OnBuildCompleted = new();

        /// <summary>
        /// Invoked when the building process for an entity begins.
        /// </summary>
        /// <remarks>
        /// Parameter:
        /// - <see cref="EntityBuilding"/>: The entity that has started its build process.
        /// </remarks>
        public UnityEvent<EntityBuilding> OnBuildStarted = new();

        /// <summary>
        /// Invoked when the build process for an entity is canceled.
        /// </summary>
        /// <remarks>
        /// Parameter:
        /// - <see cref="EntityBuilding"/>: The entity whose build process was canceled.
        /// </remarks>
        public UnityEvent<EntityBuilding> OnBuildCanceled = new();

        /// <summary>
        /// Invoked periodically to provide updates on the progress of an entity's build process.
        /// </summary>
        /// <remarks>
        /// Parameters:
        /// - <see cref="EntityBuilding"/>: The entity currently being built.
        /// - <see cref="float"/>: The progress percentage (0.0 to 1.0) of the build.
        /// </remarks>
        public UnityEvent<EntityBuilding, float> OnBuildUpdated = new();
    }


}