using System;
using UnityEngine;

namespace TRavljen.UnitSystem.Navigation
{
    using Build;
    using UnityEngine.AI;

    /// <summary>
    /// Manages the activation and deactivation of Unity's NavMesh components 
    /// (NavMeshAgent and NavMeshObstacle) for buildings during their construction lifecycle.
    /// </summary>
    /// <remarks>
    /// This component observes the <see cref="EntityBuilding"/> events to update 
    /// navigation-related components as construction progresses. It will
    /// disable and enable <see cref="NavMeshAgent"/> and <see cref="NavMeshObstacle"/>
    /// respectively.
    /// </remarks>
    [RequireComponent(typeof(EntityBuilding))]
    public class BuildableEntityNavMeshManager : MonoBehaviour
    {
        /// <summary>
        /// Reference to the NavMeshAgent component on the building, if any.
        /// </summary>
        [SerializeField]
        private NavMeshAgent agent;

        /// <summary>
        /// Reference to the NavMeshObstacle component on the building, if any.
        /// </summary>
        [SerializeField]
        private NavMeshObstacle obstacle;

        /// <summary>
        /// Reference to the EntityBuilding component associated with this object.
        /// </summary>
        private EntityBuilding entityBuilding;

        private bool hasObstacle;
        private bool hasAgent;

        /// <summary>
        /// Initializes component references for the building and NavMesh components.
        /// </summary>
        private void Awake()
        {
            // Retrieve references to the EntityBuilding and optional NavMesh components.
            entityBuilding = GetComponent<EntityBuilding>();
            SetupComponents();
        }

        private void OnValidate()
        {
            SetupComponents();
        }

        private void SetupComponents()
        {
            hasObstacle = obstacle != null || TryGetComponent(out obstacle);
            hasAgent = agent != null || TryGetComponent(out agent);
        }

        /// <summary>
        /// Subscribes to the <see cref="EntityBuilding"/> events to manage NavMesh component states.
        /// </summary>
        private void OnEnable()
        {            
            entityBuilding.OnBuildStarted.AddListener(BuildingStarted);
            entityBuilding.OnBuildCompleted.AddListener(BuildingComplete);
            entityBuilding.OnBuildCanceled.AddListener(BuildingCancelled);
            entityBuilding.OnBuildProgress.AddListener(BuildingUpdated);
        }

        /// <summary>
        /// Unsubscribes from the <see cref="EntityBuilding"/> events to prevent memory leaks.
        /// </summary>
        private void OnDisable()
        {
            if (entityBuilding != null)
            {
                entityBuilding.OnBuildStarted.RemoveListener(BuildingStarted);
                entityBuilding.OnBuildCompleted.RemoveListener(BuildingComplete);
                entityBuilding.OnBuildCanceled.RemoveListener(BuildingCancelled);
                entityBuilding.OnBuildProgress.RemoveListener(BuildingUpdated);
            }
        }

        /// <summary>
        /// Handles the logic when building construction starts.
        /// </summary>
        private void BuildingStarted()
        {
            SetObstacleEnabled(entityBuilding.Progress > 0);
            SetNavigationAgentEnabled(false);
        }

        /// <summary>
        /// Handles the logic when the building construction progress is updated.
        /// </summary>
        /// <param name="progress">The current progress of the construction.</param>
        private void BuildingUpdated(float progress)
        {
            SetObstacleEnabled(progress > 0);
        }

        /// <summary>
        /// Handles the logic when building construction is completed.
        /// </summary>
        private void BuildingComplete()
        {
            SetObstacleEnabled(true);
            SetNavigationAgentEnabled(true);

            // Cleanup as this manager is no longer needed.
            Destroy(this);
        }

        /// <summary>
        /// Handles the logic when building construction is canceled.
        /// </summary>
        private void BuildingCancelled()
        {
            SetObstacleEnabled(false);
            SetNavigationAgentEnabled(false);
        }

        /// <summary>
        /// Enables or disables the NavMeshObstacle component.
        /// </summary>
        /// <param name="enabled">Whether to enable or disable the component.</param>
        private void SetObstacleEnabled(bool enabled)
        {
            if (hasObstacle)
            {
                obstacle.enabled = enabled;
            }
        }

        /// <summary>
        /// Enables or disables the NavMeshAgent component.
        /// </summary>
        /// <param name="enabled">Whether to enable or disable the component.</param>
        private void SetNavigationAgentEnabled(bool enabled)
        {
            if (hasAgent)
            {
                agent.enabled = enabled;
            }
        }
    }

}