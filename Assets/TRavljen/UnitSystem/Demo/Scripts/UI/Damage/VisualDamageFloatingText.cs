using System;
using System.Collections;
using UnityEngine;

namespace TRavljen.UnitSystem.Demo
{
    using Combat;

    /// <summary>
    /// A visual element for displaying floating damage numbers on the screen or in world space.
    /// </summary>
    /// <remarks>
    /// Designed for demo purposes, this component visually represents damage dealt to a target. 
    /// It can operate in either screen space or world space, and optionally updates its position 
    /// dynamically if configured to follow the target.
    /// </remarks>
    public class VisualDamageFloatingText : MonoBehaviour, IDamageVisualElement
    {

        #region Properties

        [SerializeField]
        [Tooltip("The UI Text component used to display the damage value.")]
        private UnityEngine.UI.Text text;

        [SerializeField, Range(0.1f, 30)]
        [Tooltip("The duration for which the floating text remains visible.")]
        private float duration = 4;

        [SerializeField]
        [Tooltip("The initial offset from the target's position where the text starts.")]
        private Vector3 startOffset;

        [SerializeField]
        [Tooltip("The final offset from the target's position where the text ends.")]
        private Vector3 endOffset;

        [SerializeField]
        [Tooltip("Determines whether the text should be displayed in screen space.")]
        private bool useScreenSpace = true;

        [SerializeField]
        [Tooltip("If true, the text will update its position to follow the target.")]
        private bool followTarget = false;

        // A delegate for determining the starting position of the text
        private Func<Vector3> getStartPosition;

        // The transform of the target being damaged
        private Transform targetTransform;

        // The last valid position of the target, used when not following the target
        private Vector3 lastValidPosition;

        // Has animation finished
        private bool finished;
        
        public Camera PlayerCamera { get; set; }

        #endregion

        /// <summary>
        /// Displays the damage value as floating text.
        /// </summary>
        /// <param name="health">The health component of the target being damaged.</param>
        /// <param name="damage">The amount of damage dealt.</param>
        public void ShowDamage(IHealth health, int damage)
        {
            gameObject.SetActive(true);
            finished = false;

            string text = String.Format("-{0}", damage);
            this.text.text = text;
            targetTransform = health.transform;
            UpdateLastPosition();

            StartCoroutine(Animate());
        }

        /// <summary>
        /// Checks if text object can be reused.
        /// </summary>
        public bool IsReadyForPooling() => finished;

        /// <summary>
        /// Animates the floating text over time, including fade-out and position adjustments.
        /// </summary>
        private IEnumerator Animate()
        {
            float timer = 0;
            Vector3 offset = startOffset;
            Vector3 endOffset = this.endOffset;

            // Determine the position update strategy
            if (followTarget || useScreenSpace)
            {
                getStartPosition = () =>
                {
                    UpdateLastPosition();
                    return lastValidPosition;
                };
            }
            else
            {
                getStartPosition = () => lastValidPosition;
            }

            transform.position = getStartPosition() + offset;
            Color fadeColor = text.color;

            while (timer < duration)
            {
                float progress = timer / duration;
                fadeColor.a = 1 - progress;
                text.color = fadeColor;
                offset = Vector3.Lerp(startOffset, endOffset, progress);

                transform.position = getStartPosition() + offset;

                // When using world space, look at camera.
                if (!useScreenSpace)
                    transform.LookAt(PlayerCamera.transform);

                yield return new WaitForEndOfFrame();
                timer += Time.deltaTime;
            }

            finished = true;
            gameObject.SetActive(false);
        }

        /// <summary>
        /// Updates the last valid position of the target, converting it to screen space if required.
        /// </summary>
        private void UpdateLastPosition()
        {
            if (targetTransform == null) return;

            lastValidPosition = targetTransform.position;

            if (useScreenSpace)
            {
                lastValidPosition = PlayerCamera.WorldToScreenPoint(lastValidPosition);
            }
        }
    }

}