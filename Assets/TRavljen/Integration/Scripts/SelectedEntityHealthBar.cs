using TRavljen.UnitSelection;
using TRavljen.UnitSystem;
using TRavljen.UnitSystem.Demo;
using UnityEngine;

namespace IntegrationDemo
{

    /// <summary>
    /// A component for displaying a health bar that visually represents the health of selected or highlighted entity.
    /// </summary>
    public class SelectedEntityHealthBar : EntityHealthBar
    {

        [SerializeField, Tooltip("The selectable object associated with this unit.")]
        [RequiresType(typeof(ISelectable))]
        private GameObject selectableObject;

        private ISelectable selectable;

        protected override void Awake()
        {
            base.Awake();
        
            // Attempt to find the selectable component from the provided object or unit component
            if (selectableObject.IsNotNull())
            {
                selectable = selectableObject.GetComponent<ISelectable>();
            }
            else
            {
                selectable = entity.GetComponentInParent<ISelectable>() ?? entity.GetComponentInChildren<ISelectable>();
            }
        }
    
        protected override bool ShouldShowProgressIndicator()
        {
            // Ignore base conditions, do it only for selection/highlight.
            return selectable.IsSelected || selectable.IsHighlighted;
        }
    }

}