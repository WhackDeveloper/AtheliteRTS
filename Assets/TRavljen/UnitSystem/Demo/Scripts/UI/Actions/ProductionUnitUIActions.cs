using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TRavljen.UnitSystem.Demo
{

    /// <summary>
    /// Component for production unit and its actions.
    /// <para>
    /// Represents <see cref="IEntityUIHandler"/> for production actions defined
    /// by unit with <see cref="ActiveProductionCapability"/>.
    /// </para>
    /// </summary>
    [DisallowMultipleComponent]
    public class ProductionUnitUIActions : AUnitComponent, IEntityUIHandler
    {
        [SerializeField]
        private AEntityActionUIElement elementPrefab;

        [SerializeField]
        private int sectionIndex = 0;

        public bool BuildUIElements(EntityUILayoutData layoutData)
        {
            if (!Unit.TryGetCapability(out ActiveProductionCapability capability) ||
                layoutData.SectionIndex != sectionIndex)
                return false;
            
            foreach (var action in capability.ProductionActions)
            {
                var element = Instantiate(elementPrefab, layoutData.Container);
                element.Configure(Unit);
                element.SetAction(action);
            }

            return true;
        }
    }
}