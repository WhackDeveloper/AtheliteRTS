using JetBrains.Annotations;
using TRavljen.EditorUtility;
using UnityEngine.Serialization;

namespace TRavljen.Tooltip
{
    using UnityEngine;
    using Input;
    using UnityEngine.Events;
    
    /// <summary>
    /// Manages tooltips in the UI, controlling their visibility, position, and timing based on user interactions.
    /// Supports customizable delays, update modes, and manual control of tooltip positions.
    /// Use interface like <see cref="ShowTooltip"/> to show tooltip at any time,
    /// <see cref="customPosition"/> to define custom positions of tooltips or use hover features with
    /// <see cref="HoverTooltip"/> which manages hovering state and information read by this manager. 
    /// </summary>
    /// <remarks>
    /// The <see cref="tooltipUI"/> elements should have disabled interactions (e.g., mask) on their contents. 
    /// Enabling interactions on tooltip elements may cause flickering due to hover state transitions.
    /// This happens because the tooltip appearing under the cursor triggers a cursor exit event,
    /// clearing the hovered state and hiding the tooltip. Then the cycle repeats.
    /// </remarks>
    public sealed class TooltipManager : MonoBehaviour
    {

        #region Properties

        /// <summary>
        /// Specifies the UI component used to display tooltips.
        /// Assign a tooltip UI prefab or object in the Inspector.
        /// </summary>
        [Tooltip("Specifies UI for displaying object's tooltip for the user.")]
        [SerializeField] private ATooltipUI tooltipUI;

        /// <summary>
        /// Determines if the delay is controlled globally or by individual tooltip objects.
        /// When true, the delay of individual tooltips is ignored.
        /// </summary>
        [Tooltip("Specifies if the delay is controlled globally or by individual tooltips.")]
        public bool useGlobalDelay = true;
        
        /// <summary>
        /// The delay in seconds before displaying a tooltip after hovering.
        /// If <see cref="useGlobalDelay"/> is enabled, this overrides individual object delays.
        /// Value is clamped to a minimum of 0.
        /// </summary>
        [SerializeField]
        [Tooltip("Specifies the delay in seconds before showing a tooltip after cursor enters its bounds.")]
        [DisableIf("useGlobalDelay", false)]
        private float globalDelay = 0.1f;

        [SerializeField, Tooltip("Determines if positions are updated manually or automatically. " +
                                 "When disabled use TooltipUpdate method to invoke updates.")]
        private bool updateManually;

        /// <summary>
        /// Determines if cursor or custom position is used to show the tooltip.
        /// </summary>
        public bool useCustomPosition;
        
        /// <summary>
        /// Determines custom position of the tooltip. Used when
        /// <see cref="useCustomPosition"/> is enabled.
        /// </summary>
        [DisableIf("useCustomPosition", false)]
        public Vector2 customPosition;

        [Tooltip("Determines if canvases will be force update when tooltip is shown. With resizing tooltips " +
                 "and overflow alignment changes, the tooltip may flicker for 1 frame with this disabled.\n" +
                 "This is disabled by default duo to potential performance impact.")]
        [SerializeField] private bool updateForceCanvasUpdate;
        
        [SerializeField]
        private TooltipLayout layout = new();
        
        [Header("Events")]
        public UnityEvent<ITooltipInformation> OnShowTooltip = new();
        public UnityEvent<ITooltipInformation> OnHideTooltip = new();

        /// <summary>
        /// Stores the currently selected object for displaying its tooltip.
        /// </summary>
        private ITooltipInformation _currentlyDisplayedInformation;

        private ITooltipInformation _customInfo;

        /// <summary>
        /// Current delay, set based on delay configuration.
        /// </summary>
        private float _currentDelay;

        /// <summary>
        /// Tracks whether the tooltip is currently being displayed.
        /// </summary>
        private bool _showingTooltip;

        /// <summary>
        /// Gets or sets the global delay for showing tooltips.
        /// Clamped to non-negative values.
        /// </summary>
        public float GlobalDelay
        {
            get => globalDelay;
            set => globalDelay = Mathf.Max(value, 0);
        }

        /// <summary>
        /// Specifies the provider for cursor position. By default, mouse position will be used.
        /// This provider is used to determine where the tooltip should be displayed when
        /// <see cref="useCustomPosition"/> is disabled, by following the cursor.
        /// </summary>
        public ICursorPositionProvider cursorPositionProvider = new DefaultMousePositionProvider();
        
        #endregion

        #region Lifecycle

        private void Awake()
        {
            layout.Setup();
        }

        private void OnValidate()
        {
            globalDelay = Mathf.Max(0, globalDelay);
        }

        private void OnDisable()
        {
            // Hide current tooltip on disable
            ShowTooltip(null, 0f);
        }

        private void Update()
        {
            if (!updateManually)
                TooltipUpdate();
        }

        private void LateUpdate()
        {
            // Use late update to fixup positions and overflow.
            if (!updateManually && _showingTooltip)
                UpdateTooltipPosition();
        }
        
        #if UNITY_EDITOR
        private void OnGUI()
        {
            if (_showingTooltip)
                layout.RenderDebugGUI();
            
            if (!updateManually && _showingTooltip)
                UpdateTooltipPosition();
        }
        #endif

        #endregion

        /// <summary>
        /// Call this method to manually update tooltip.
        /// </summary>
        public void TooltipUpdate()
        {
            // Check if static reference was updated and if it was update information UI.
            var hoveringObject = GetHoveringTooltip();
            if (_customInfo == null && _currentlyDisplayedInformation != hoveringObject?.information)
                ShowTooltip(hoveringObject?.information, hoveringObject?.Delay ?? 0);
            else
                PerformTooltipUpdate();
        }

        public void ShowCustomTooltip(ITooltipInformation info, float delay)
        {
            _customInfo = info;
            ShowTooltip(info, delay);
        }

        public void RemoveCustomTooltip()
        {
            // Simply clear, if object is hovering the Update will pick that up.
            _customInfo = null;
            ShowTooltip(null, 0f);
        }

        /// <summary>
        /// Hides the current tooltip and prepares the specified object for tooltip display.
        /// </summary>
        /// <param name="info">The tooltip information to display.</param>
        /// <param name="delay">Delay for showing the new tooltip</param>
        private void ShowTooltip(ITooltipInformation info, float delay)
        {
            delay = Mathf.Max(0, delay);
            if (_showingTooltip)
            {
                _showingTooltip = false;
                tooltipUI.Hide();
                OnHideTooltip.Invoke(_currentlyDisplayedInformation);
            }

            _currentlyDisplayedInformation = info;

            if (useGlobalDelay)
                _currentDelay = GlobalDelay;
            else if (info != null)
                _currentDelay = delay;
            else
                _currentDelay = 0f;

            PerformTooltipUpdate();
        }
        
        #region Convenience

        /// <summary>
        /// Updates the tooltip's position to follow the current cursor location or
        /// uses specified custom position.
        /// </summary>
        private void UpdateTooltipPosition()
        {
            Vector3 position;
            
            if (useCustomPosition)
            {
                position = customPosition;
            }
            else
            { 
                position = cursorPositionProvider.CursorPosition;
                position = layout.FixOverflow(position);
            }
            
            tooltipUI.transform.position = position;
        }

        // ReSharper disable Unity.PerformanceAnalysis - invoked once when delay runs out and that is it.
        /// <summary>
        /// Updates tooltip visibility and position.
        /// Handles the delay logic and determines when to show or hide the tooltip.
        /// </summary>
        private void PerformTooltipUpdate()
        {
            if (_currentDelay < 0 ||
                _currentlyDisplayedInformation == null ||
                _showingTooltip) 
                return;
            
            _currentDelay -= Time.deltaTime;

            if (_currentDelay > 0) return;

            tooltipUI.SetInformation(_currentlyDisplayedInformation);
            tooltipUI.Show();
            _showingTooltip = true;
            
            OnShowTooltip.Invoke(_currentlyDisplayedInformation);

            if (updateForceCanvasUpdate)
                Canvas.ForceUpdateCanvases();

            // Update position right away, in case it's done after the Late Update.
            UpdateTooltipPosition();
        }

        [CanBeNull]
        private static HoverTooltip GetHoveringTooltip()
        {
            return HoverObject.FocusedObject as HoverTooltip;
        }
        
        #endregion
    }
    
}

