using UnityEngine.Events;

namespace TRavljen.UnitSystem.Build
{
    using UnityEngine;

    /// <summary>
    /// An animation that raises a building from a specified start offset 
    /// to its final position during the build process.
    /// </summary>
    [DisallowMultipleComponent]
    class RaiseBuildingAnimation : ABuildingAnimation
    {

        #region Properties

        /// <summary>
        /// The vertical offset from which the building starts rising.
        /// </summary>
        [SerializeField]
        private float startOffset = -5;

        /// <summary>
        /// The final position of the animated transform.
        /// </summary>
        private Vector3 endPosition;

        /// <summary>
        /// The initial position of the animated transform.
        /// </summary>
        private Vector3 startPosition;

        #endregion

        #region Animation

        /// <inheritdoc/>
        protected override void AnimationStart()
        {
            // Record the final position.
            endPosition = transform.InverseTransformPoint(animatedTransform.position);
            // Calculate the start position by applying the offset.
            startPosition = endPosition + Vector3.up * startOffset;

            // Set the transform to the start position.
            animatedTransform.position = transform.TransformPoint(startPosition);
        }

        /// <inheritdoc/>
        protected override void AnimationUpdate(float progress)
        {
            // Interpolate the position between start and end based on progress.
            animatedTransform.position = transform.TransformPoint(Vector3.Lerp(startPosition, endPosition, progress));
        }

        /// <inheritdoc/>
        protected override void AnimationEnd()
        {
            // Ensure the transform reaches the final position.
            animatedTransform.position = transform.TransformPoint(endPosition);

            // Destroy this component as the animation is complete.
            Destroy(this);
        }

        #endregion

    }

}
