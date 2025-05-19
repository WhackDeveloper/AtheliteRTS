using UnityEngine;
using UnityEngine.UI;

namespace TRavljen.Tooltip
{
    
    /// <summary>
    /// Concrete implementation of <see cref="ATooltipUI"/> for displaying text-based tooltips.
    /// Uses a <see cref="Text"/> component to show tooltip content and expects information
    /// of type <see cref="ITextTooltipInformation"/>.
    /// </summary>
    public class TextTooltipUI : ATooltipUI
    {
        
        /// <summary>
        /// The <see cref="Text"/> component used to display the tooltip's content.
        /// </summary>
        [SerializeField]
        private Text infoText;
        
        /// <summary>
        /// Updates the tooltip's text content with the provided information.
        /// If the information is not of type <see cref="ITextTooltipInformation"/>, 
        /// logs a warning and does not update the UI.
        /// </summary>
        /// <param name="information">The tooltip information to display.</param>
        public override void SetInformation(ITooltipInformation information)
        {
            if (information is ITextTooltipInformation textInfo)
                infoText.text = textInfo.Text;
            else
                Debug.LogWarning(information + " cast to ITextTooltipInformation failed. Unexpected reference type was passed. Previous text will be displayed.");
        }
        
    }


}