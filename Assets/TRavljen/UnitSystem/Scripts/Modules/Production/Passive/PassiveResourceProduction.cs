using System.Collections.Generic;
using UnityEngine;

namespace TRavljen.UnitSystem
{

    /// <summary>
    /// Unit component for producing resources passively.
    /// Resources to produce are retrieved from units <see cref="Entity.data"/> and
    /// its capability defined by IPassiveResourceProductionCapability interface.
    /// </summary>
    [DisallowMultipleComponent]
    public class PassiveResourceProduction : AEntityComponent, IProduce
    {

        #region Properties

        private IPassiveResourceProductionCapability passiveProduction;

        private float[] currentResourceProduced = new float[0];

        /// <summary>
        /// Constructs resource quantity data for currently producing resource
        /// that has not yet been deposited to the player.
        /// </summary>
        public ResourceQuantity[] CurrentResourceProduced
        {
            get
            {
                ResourceQuantity[] modifable = new ResourceQuantity[passiveProduction.ProducesResource.Length];
                for (int index = 0; index < passiveProduction.ProducesResource.Length; index++)
                {
                    ResourceQuantity resourceQuantity = passiveProduction.ProducesResource[index];
                    resourceQuantity.Quantity = (long)currentResourceProduced[index];
                    modifable[index] = resourceQuantity;
                }

                return modifable;
            }
        }

        #endregion

        protected override void OnInitialize()
        {
            base.OnInitialize();

            if (!Entity.TryGetCapability(out passiveProduction))
            {
                Debug.LogError("IPassiveResourceProductionCapability is expected to be on Entity for this component.");
                return;
            }

            // Create data set for each resource produced by the unit
            currentResourceProduced = new float[passiveProduction.ProducesResource.Length];
        }

        public void Produce(float delta)
        {
            // Ignore if unit is not yet operational or no owner
            if (!Entity.IsOperational || Entity.Owner == null) return;

            for (int index = 0; index < passiveProduction.ProducesResource.Length; index++)
            {
                ResourceQuantity resourceQuantity = passiveProduction.ProducesResource[index];
                float quantity = resourceQuantity.Quantity * delta;
                float produced = currentResourceProduced[index];
                produced += quantity;

                if (produced >= passiveProduction.DepositResourceQuantity)
                {
                    // Deposit the quantity desired/set
                    Entity.Owner.AddResource(new(resourceQuantity.Resource, (long)produced));
                    produced = 0;
                }

                currentResourceProduced[index] = produced;
            }
        }

    }

}