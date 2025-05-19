using UnityEngine;

namespace TRavljen.UnitSystem.Demo
{

    /// <summary>
    /// Component for custom actions defined on unit
    /// </summary>
    [DisallowMultipleComponent]
    public class CustomUnitUIActions : AUnitComponent, IEntityUIHandler
    {

        [SerializeField] private AEntityActionUIElement elementPrefab;
        [SerializeField] private int sectionIndex = 0;
        [SerializeField] private ProductionAction[] actions;

        public bool BuildUIElements(EntityUILayoutData layoutData)
        {
            if (layoutData.SectionIndex != sectionIndex)
                return false;
            
            foreach (var action in actions)
            {
                var element = Instantiate(elementPrefab, layoutData.Container);
                element.Configure(unit);
                element.SetAction(action);
            }

            return true;
        }

    }
    
}