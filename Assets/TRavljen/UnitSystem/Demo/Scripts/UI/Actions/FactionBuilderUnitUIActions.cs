using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TRavljen.UnitSystem.Demo
{
    
    /// <summary>
    /// Component for builder unit using faction default actions.
    /// Represents <see cref="IEntityUIHandler"/> for production actions defined
    /// by unit with <see cref="FactionSO"/> and its default actions.
    /// <para>
    /// If custom <see cref="AFactionSO"/> is used, it will fall back on units
    /// owner if it implements <see cref="IEntityUIHandler"/> interface.
    /// </para>
    /// </summary>
    [DisallowMultipleComponent]
    public class FactionBuilderUnitUIActions : AUnitComponent, IEntityUIHandler
    {

        [SerializeField]
        private AEntityActionUIElement elementPrefab;

        [SerializeField] private int sectionIndex = 0;

        public bool BuildUIElements(EntityUILayoutData layoutData)
        {
            if (layoutData.SectionIndex != sectionIndex)
                return false;
            
            // Get default actions from faction - if correct type is used
            if (unit.Owner.Faction is FactionSO faction)
            {
                foreach (var action in faction.DefaultActions)
                {
                    var element = GameObject.Instantiate(elementPrefab, layoutData.Container);
                    element.Configure(unit);
                    element.SetAction(action);
                }

                return true;
            }

            // Fallback to any default actions player itself might provide.
            if (unit.Owner is IEntityUIHandler handler && handler.IsNotNull())
                return handler.BuildUIElements(layoutData);

            // No default actions.
            return false;
        }

    }

}