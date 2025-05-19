namespace TRavljen.Tooltip
{
    
    /// <summary>
    /// Interface for providing tooltip information to the <see cref="TooltipManager"/> through
    /// <see cref="HoverTooltip"/> and its information value or directly by using <see cref="TooltipManager.ShowCustomTooltip"/>.
    /// </summary>
    public interface ITextTooltipInformation: ITooltipInformation
    {
        /// <summary>
        /// Specifies the text which will be displayed on tooltip.
        /// </summary>
        public string Text { get; }
    }
    
}