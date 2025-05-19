using System;
using UnityEngine;

namespace TRavljen.Tooltip
{
    
    /// <summary>
    /// Defines text information used for <see cref="HoverTooltip"/> or <see cref="TooltipManager"/> directly.
    /// </summary>
    [Serializable]
    public struct TextTooltipInformation : ITextTooltipInformation
    {
        
        /// <summary>
        /// The text content to display in the tooltip once object is hovered.
        /// </summary>
        [TextArea]
        public string text;

        public string Text
        {
            get => text; 
            set => text = value;
        }

        public TextTooltipInformation(string text = "")
        {
            this.text = text;
        }
    }

}