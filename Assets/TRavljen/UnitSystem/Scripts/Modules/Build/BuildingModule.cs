using System.Collections;
using System.Collections.Generic;
using TRavljen.UnitSystem.Interactions;
using UnityEngine;
using UnityEngine.Serialization;

namespace TRavljen.UnitSystem.Build
{
    using Collection;
    using Task;

    /// <summary>
    /// A module for managing building operations within a player's system. 
    /// Handles assigning builders to building tasks, managing resource consumption, 
    /// and integrating with other modules such as CollectionModule.
    /// </summary>
    [DisallowMultipleComponent]
    public class BuildingModule : APlayerModule
    {

        /// <summary>
        /// Specifies when resources should be collected by builders.
        /// </summary>
        enum CollectResources
        {
            /// <summary>
            /// Builder should never attempt to collect resources.
            /// </summary>
            Never,
            /// <summary>
            /// Builder should attempt to find resources after building a resource depot.
            /// </summary>
            AfterBuilding,
            /// <summary>
            /// Builder should attempt to find resources after all nearby building units are completed
            /// and the last one is resource depot.
            /// </summary>
            WhenNoWork
        }

        #region Fields

        [SerializeField]
        [Tooltip("If enabled, builders are automatically discovered when added to the player's units.")]
        private bool findBuildersAutomatically = true;

        [SerializeField]
        [Tooltip("If enabled, builders are automatically assigned work upon being added (e.g. spawned).")]
        private bool assignBuildableWhenBuilderAdded = false;

        [SerializeField]
        [Tooltip("Specifies the behavior for resource collection by builders when building process finished.")]
        private CollectResources collectResources = CollectResources.Never;

        public IBuildableEntityFinder BuildingFinder;

        private readonly List<IBuilder> builders = new();
        private CollectionModule collectionModule;

        #endregion

        #region Lifecycle

        protected override void Awake()
        {
            base.Awake();
            
            BuildingFinder ??= new BuildableEntityFinder(player);
            collectionModule = player.GetModule<CollectionModule>();
        }

        private void OnEnable()
        {
            player.OnUnitAdded.AddListener(UnitAdded);
            player.OnUnitRemoved.AddListener(UnitRemoved);

            BuildEvents.Instance.OnBuilderFinished.AddListener(OnBuilderFinished);
        }

        private void OnDisable()
        {
            player.OnUnitAdded.RemoveListener(UnitAdded);
            player.OnUnitRemoved.RemoveListener(UnitRemoved);

            BuildEvents.Instance.OnBuilderFinished.RemoveListener(OnBuilderFinished);
        }

        #endregion

        #region Public interface

        /// <summary>
        /// Starts the building process of a new entity if eligible.
        /// </summary>
        /// <param name="newObject">The GameObject representing the new entity to build.</param>
        /// <returns>True if building started successfully; otherwise, false.</returns>
        public bool StartBuilding(GameObject newObject)
        {
            return newObject.TryGetComponent(out Entity entity) && StartBuilding(entity);
        }

        /// <summary>
        /// Starts the building process of a new entity if eligible.
        /// </summary>
        /// <param name="entity">The Entity to build.</param>
        /// <returns>True if building started successfully; otherwise, false.</returns>
        public bool StartBuilding(Entity entity)
        {
            if (entity.Data.RequiresBuilding())
            {
                // Enable component for build process (it is disabled during placement)
                if (entity.Buildable is MonoBehaviour mono)
                    mono.enabled = true;

                entity.SetOperational(false);
                return true;
            }

            return false;
        }

        /// <summary>
        /// Returns the count of currently available builders.
        /// </summary>
        public int GetAvailableBuilderCount()
        {
            var system = TaskSystem.GetOrCreate();
            var count = 0;
            
            foreach (var builder in builders)
            {
                // Units without tasks are considered as available.
                if (system.HasActiveTask(builder.transform.gameObject) == false)
                    count++;
            }

            return count;
        }

        /// <summary>
        /// Finds the closest entity with an active building process.
        /// </summary>
        /// <param name="builder">Builder to find the entity for building</param>
        /// <param name="filterFull">
        /// If enabled, filters out full units (limited by <see cref="Interactions.ILimitedUnitInteractionTarget"/>).
        /// </param>
        /// <param name="buildable">Closest entity in building process.</param>
        /// <returns>Returns true if entity was found; otherwise false.</returns>
        public bool FindClosestUnitBuilding(IBuilder builder, bool filterFull, out IBuildableEntity buildable)
        {
            return BuildingFinder.FindClosest(
                builder.Position,
                builder.AutoPickUpWorkRadius,
                filterFull,
                out buildable);
        }

        #endregion

        #region Building Management

        /// <summary>
        /// Handles the event when a builder completes a building task.
        /// </summary>
        /// <param name="builder">The builder that completed the task.</param>
        /// <param name="builtEntity">The entity that was built.</param>
        private void OnBuilderFinished(Builder builder, IEntity builtEntity)
        {
            // Ignore this if collection module is missing.
            if (collectionModule == null) return;

            switch (collectResources)
            {
                case CollectResources.Never:
                    if (FindClosestUnitBuilding(builder, true, out IBuildableEntity buildable))
                    {
                        builder.GoBuildUnit(buildable);
                    }
                    break;

                case CollectResources.AfterBuilding:
                    if (!SendToNearbyResourceNode(builder, builtEntity))
                    {
                        if (FindClosestUnitBuilding(builder, true, out buildable))
                        {
                            builder.GoBuildUnit(buildable);
                        }
                    }
                    break;

                case CollectResources.WhenNoWork:
                    if (FindClosestUnitBuilding(builder, true, out buildable))
                    {
                        builder.GoBuildUnit(buildable);
                    }
                    else
                    {
                        SendToNearbyResourceNode(builder, builtEntity);
                    }
                    break;
            }
        }

        /// <summary>
        /// Directs a builder to collect resources from a nearby resource node.
        /// </summary>
        /// <param name="builder">The builder to assign to resource collection.</param>
        /// <param name="builtEntity">The unit that was built.</param>
        /// <returns>True if a resource node was successfully assigned; otherwise, false.</returns>
        private bool SendToNearbyResourceNode(Builder builder, IEntity builtEntity)
        {
            // If built unit was destroyed or is missing, we cannot proceed
            // to collect resources for it
            if (builtEntity.IsNull()) return false;

            if (!builder.Unit.TryGetComponent(out ResourceCollector collector) ||
                !builtEntity.TryGetEntityComponent(out IResourceDepot depot))
                return false;
            
            if (depot.SupportedResources == null || depot.SupportedResources.Length == 0) 
                return true;

            // Find first resource nearby that this depot supports.
            foreach (var resource in depot.SupportedResources)
            {
                if (!collector.CanCollect(resource))
                    continue;

                if (!collectionModule.FindNearbyNode(collector.Position, resource, out var node)) 
                    continue;
                
                collector.GoCollectResource(node);
                return true;
            }

            return false;
        }

        private void UnitRemoved(IUnit unit)
        {
            if (unit?.Builder?.IsNotNull() == true)
            {
                builders.Remove(unit.Builder);
            }
        }

        private void UnitAdded(IUnit unit)
        {
            // First assign builders
            if (findBuildersAutomatically &&
                unit?.Buildable.IsNotNull() == true &&
                !unit.Buildable.IsBuilt)
            {
                AssignBuildersInRange(unit.Buildable);
            }

            if (unit?.Builder.IsNotNull() == true)
            {
                var newBuilder = unit.Builder;
                builders.Add(newBuilder);

                // Assign work right away if supported & unit is operational.
                if (assignBuildableWhenBuilderAdded &&
                    unit.IsOperational &&
                    unit.HasActiveTask() == false &&
                    FindClosestUnitBuilding(newBuilder, true, out IBuildableEntity closestBuildable) &&
                    ShouldAutoPickupJob(newBuilder, closestBuildable.Position))
                {
                    newBuilder.GoBuildUnit(closestBuildable);
                }
            }
        }

        private void AssignBuildersInRange(IBuildableEntity buildableEntity)
        {
            Vector3 position = buildableEntity.transform.position;

            List<(IBuilder builder, float distance)> availableBuilders = new();

            // Send all available works for the job.
            foreach (var builder in builders)
            {
                if (builder.IsNotNull() &&
                    builder.IsActive &&
                    !builder.Unit.HasActiveTask())
                    availableBuilders.Add((builder, Vector3.Distance(position, builder.Position)));
            }

            if (availableBuilders.Count == 0) return;

            availableBuilders.Sort((a, b) => a.distance.CompareTo(b.distance));

            int unitsToAssign;

            if (buildableEntity is ILimitedUnitInteractionTarget limiting && limiting.IsLimitActive())
                unitsToAssign = limiting.AvailableInteractions;
            else
                unitsToAssign = availableBuilders.Count;

            foreach (var (builder, range) in availableBuilders)
            {
                // Checks range of the builder
                if (builder.AutoPickUpWorkRadius < range) continue;

                if (unitsToAssign == 0) break;

                if (builder.GoBuildUnit(buildableEntity))
                {
                    unitsToAssign--;
                }
            }
        }

        /// <summary>
        /// Determines whether the unit should automatically pick up tasks based on its position.
        /// </summary>
        /// <returns>True if the unit should pick up the task, false otherwise.</returns>
        private bool ShouldAutoPickupJob(IBuilder builder, Vector3 jobPosition)
            => builder.AutoPickup && IsWithinAssignmentRange(builder, jobPosition);

        private bool IsWithinAssignmentRange(IBuilder builder, Vector3 position)
            => Vector3.Distance(position, builder.Position) < builder.AutoPickUpWorkRadius;


        #endregion
    }

}