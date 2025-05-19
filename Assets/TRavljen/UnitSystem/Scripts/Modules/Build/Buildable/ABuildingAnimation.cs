using UnityEngine;

namespace TRavljen.UnitSystem.Build
{

    /// <summary>
    /// Base class for handling animations during the build process of a unit.
    /// Provides hooks for starting, updating, and completing animations,
    /// and integrates with the <see cref="EntityBuilding"/> events.
    ///<para>
    /// If this component is disabled by default and when building process starts,
    /// <see cref="StartAnimation"/> method should be invoked.
    /// </para>
    /// </summary>
    abstract class ABuildingAnimation : MonoBehaviour
    {
        #region Properties

        /// <summary>
        /// The transform that will be animated during the build process.
        /// </summary>
        [SerializeField]
        [Tooltip("The transform that will be animated during the build process.")]
        protected Transform animatedTransform;

        /// <summary>
        /// Indicates whether the animation has started.
        /// </summary>
        protected bool started = false;

        #endregion

        #region Lifecycle

        /// <summary>
        /// Subscribes to the <see cref="EntityBuilding"/> events when the component is enabled.
        /// </summary>
        protected virtual void OnEnable()
        {
            if (TryGetComponent(out EntityBuilding building))
            {
                building.OnBuildStarted.AddListener(HandleStart);
                building.OnBuildProgress.AddListener(HandleChange);
                building.OnBuildCompleted.AddListener(HandleComplete);
            }
        }

        /// <summary>
        /// Unsubscribes from the <see cref="EntityBuilding"/> events when the component is disabled.
        /// </summary>
        protected virtual void OnDisable()
        {
            if (TryGetComponent(out EntityBuilding building))
            {
                building.OnBuildStarted.RemoveListener(HandleStart);
                building.OnBuildProgress.RemoveListener(HandleChange);
                building.OnBuildCompleted.RemoveListener(HandleComplete);
            }
        }

        #endregion

        #region Public Interface

        /// <summary>
        /// Manually triggers the animation start. This can be used when the component is 
        /// disabled or the animation needs to be started programmatically.
        /// </summary>
        public void StartAnimation()
        {
            if (started) return;

            // Enable component if needed
            if (!enabled) enabled = true;

            HandleStart();
        }

        #endregion

        #region Events

        /// <summary>
        /// Handles the start of the animation process by invoking the abstract <see cref="AnimationStart"/> method.
        /// </summary>
        private void HandleStart()
        {
            started = true;
            AnimationStart();
        }

        /// <summary>
        /// Handles the animation update during the build process and invokes the abstract <see cref="AnimationUpdate"/> method.
        /// </summary>
        /// <param name="progress">The progress of the build process, ranging from 0 to 1.</param>
        private void HandleChange(float progress) => AnimationUpdate(progress);


        /// <summary>
        /// Handles the completion of the animation process and invokes the abstract <see cref="AnimationEnd"/> method.
        /// </summary>
        private void HandleComplete() => AnimationEnd();

        #endregion

        #region Abstract

        /// <summary>
        /// Called when the animation starts. Implement this to define the animation's starting behavior.
        /// </summary>
        protected abstract void AnimationStart();

        /// <summary>
        /// Called during the animation process to update the animated object.
        /// </summary>
        /// <param name="progress">The progress of the build process, ranging from 0 to 1.</param>
        protected abstract void AnimationUpdate(float progress);

        /// <summary>
        /// Called when the animation ends. Implement this to define the animation's final behavior.
        /// </summary>
        protected abstract void AnimationEnd();

        #endregion
    }

}