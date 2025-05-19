namespace TRavljen.Tooltip
{
    using UnityEngine;
    
    /// <summary>
    /// Defines the base component for displaying tooltip when hovering an object.
    /// Attach this to a game object and <see cref="TooltipManager"/> will display
    /// the <see cref="information"/> present on this component.
    /// </summary>
    /// <remarks>
    /// To fully customise information on the tooltip implement your own using interface
    /// <see cref="ITooltipInformation"/> or <see cref="ITextTooltipInformation"/>.
    /// </remarks>
    public class HoverTooltip : HoverObject
    {
        
        /// <summary>
        /// Specifies the delay (in seconds) before showing the tooltip after the pointer enters the object's bounds.
        /// This value is ignored if the global delay is enabled in the <see cref="TooltipManager"/>.
        /// </summary>
        [SerializeField, Range(0, 30)]
        [Tooltip("Specifies the delay of tooltip before showing it. " +
                 "This value is ignored if global delay is enabled on the manager.")]
        private float delay = 0.25f;
    
        /// <summary>
        /// The information to display on tooltip once this object is hovered.
        /// </summary>
        [SerializeReference] public ITooltipInformation information = new TextTooltipInformation();
    
        /// <summary>
        /// Delay used for this specific tooltip, but only when global delay is disabled.
        /// </summary>
        public float Delay
        {
            get => delay;
            set => delay = Mathf.Max(0, value);
        }
        
        /// <summary>
        /// Updates the tooltip if it is currently active and showing.
        /// Call this method whenever the tooltip UI needs to change because
        /// the information displayed is outdated. 
        /// </summary>
        public void UpdateIfActive()
        {
            // Update info on the tooltip in case its hovering
            if (FocusedObject == this)
                SetFocusObject(this);
        }

    }

}
