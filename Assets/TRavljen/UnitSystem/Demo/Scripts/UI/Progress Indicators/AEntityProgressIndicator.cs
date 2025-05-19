using UnityEngine;
using UnityEngine.UI;

namespace TRavljen.UnitSystem.Demo
{
    
    /// <summary>
    /// Abstract behaviour for progress indicator. When subclassing for specific use simply
    /// override the <see cref="ShouldShowProgressIndicator"/> and update <see cref="Progress"/>
    /// when the progress itself should update (e.g. on event) for updating the UI.
    /// </summary>
    public abstract class AEntityProgressIndicator : MonoBehaviour
    {
        
        [SerializeField, Tooltip("The Entity associated with the progress indicator.")]
        protected Entity entity;
        
        [SerializeField, Tooltip("The canvas that contains the progress bar UI.")]
        protected Canvas canvas;

        [SerializeField, Tooltip("The image used to represent the progress.")]
        protected Image progressImage;

        /// <summary>
        /// Reference to the main camera's transform, used for alignment.
        /// </summary>
        protected Transform cameraTransform;

        /// <summary>
        /// Cached reference to this object's transform.
        /// </summary>
        protected Transform _transform;

        /// <summary>
        /// The current progress value to be displayed, ranging from 0 to 1.
        /// </summary>
        public float Progress { get; protected set; }
        
        protected virtual void Awake()
        {
            if (entity == null)
                entity = GetComponentInParent<Entity>();

            // Find the main camera's transform
            // Note: Consider improving this to avoid repetitive Camera.main calls
            cameraTransform = Camera.main?.transform;

            // Cache the transform for better performance
            _transform = transform;
        }

        protected virtual void OnValidate()
        {
            if (entity == null)
                entity = GetComponentInParent<Unit>();
        }

        protected virtual void Update()
        {
            // Determine whether the progress bar should be shown
            var show = ShouldShowProgressIndicator();

            // Show or hide the canvas based on the condition
            canvas.enabled = show;

            // If the canvas is shown update it
            if (show)
            {
                UpdateBar();
            }
        }

        /// <summary>
        /// Determines whether the progress indicator should be displayed.
        /// </summary>
        /// <returns>True if the indicator should be shown; otherwise, false.</returns>
        protected abstract bool ShouldShowProgressIndicator();
        
        /// <summary>
        /// Updates the progress bar's appearance and alignment.
        /// </summary>
        protected virtual void UpdateBar()
        {
            // Updates fill & rotation
            progressImage.fillAmount = Progress;
            _transform.LookAt(cameraTransform);
        }
    }
    
}