using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TRavljen.UnitSystem
{

    /// <summary>
    /// Represents an action that can be performed to produce a specified quantity of a producible item.
    /// Typically used for binding production actions to user input (e.g., keyboard shortcuts).
    /// </summary>
    [System.Serializable]
    public struct ProductionAction: IEntityUIAction
    {
        #region Fields

        /// <summary>
        /// The key associated with this production action.
        /// Pressing this key triggers the production of the associated item.
        /// </summary>
        [SerializeField]
        private KeyCode keyCode;

        /// <summary>
        /// The producible item and its quantity associated with this action.
        /// </summary>
        [SerializeField]
        private ProducibleQuantity producibleQuantity;

        #endregion

        #region Properties

        /// <summary>
        /// Gets the key associated with this production action.
        /// </summary>
        public readonly KeyCode KeyCode => keyCode;

        /// <summary>
        /// Gets the producible item and quantity associated with this action.
        /// </summary>
        public readonly ProducibleQuantity ProducibleQuantity => producibleQuantity;

        #endregion

        public readonly void Execute(IEntity entity)
        {
            if (entity.Owner.TryGetModule(out ProductionModule module))
                module.RequestProduction(ProducibleQuantity, entity);
        }

    }

}
