namespace TRavljen.Tooltip
{

    /// <summary>
    /// Interface for providing tooltip information to the <see cref="TooltipManager"/> either through
    /// <see cref="HoverTooltip"/> and its information value or directly by using <see cref="TooltipManager.ShowCustomTooltip"/>.
    /// Any class implementing this interface can be used as a source of data for tooltips.
    /// Most commonly used is text, therefore <see cref="TextTooltipInformation"/> is provided.
    /// </summary>
    public interface ITooltipInformation { }

}