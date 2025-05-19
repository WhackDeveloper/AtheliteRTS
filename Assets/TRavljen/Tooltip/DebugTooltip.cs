using System.Collections;
using UnityEngine;

namespace TRavljen.Tooltip
{
    class DebugTooltip : MonoBehaviour
    {
        
        [SerializeField] private TextTooltipInformation info = 
            new("Manages tooltips in the UI, controlling their visibility, position, and timing based on user interactions.\n Supports customizable delays, update ");

        private void Start()
        {
            var manager = Object.FindFirstObjectByType<TooltipManager>();
            manager.ShowCustomTooltip(info, 0.1f);
            StartCoroutine(HideCustom());
        }

        private IEnumerator HideCustom()
        {
            yield return new WaitForSeconds(5);
            var manager = Object.FindFirstObjectByType<TooltipManager>();
            manager.RemoveCustomTooltip();
        }
    }
}