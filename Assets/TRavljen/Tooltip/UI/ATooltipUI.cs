using UnityEngine;

namespace TRavljen.Tooltip
{

    /// <summary>
    /// Abstract base class for UI components that display tooltips.
    /// Implementations of this class are responsible for defining how tooltip information 
    /// is visually presented to the user.
    /// </summary>
    public abstract class ATooltipUI : MonoBehaviour
    {
        
        /// <summary>
        /// Updates the tooltip UI with the provided information.
        /// Implementations must handle the specific type of <see cref="ITooltipInformation"/> they support.
        /// </summary>
        /// <param name="information">The tooltip information to display.</param>
        public abstract void SetInformation(ITooltipInformation information);

        public virtual void Show()
        {
            gameObject.SetActive(true);
        }

        public virtual void Hide()
        {
            gameObject.SetActive(false);
        }
        
    }

}