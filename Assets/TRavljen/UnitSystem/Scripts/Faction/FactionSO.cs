using UnityEngine;

namespace TRavljen.UnitSystem
{

    /// <summary>
    /// Represents a concrete implementation of a faction, defining its starting configuration,
    /// default production actions, and resource setup.
    /// </summary>
    [CreateAssetMenu(fileName = "New faction", menuName = "Unit System/Faction")]
    public class FactionSO : AFactionSO
    {

        [Tooltip("Default actions for the faction. Generally these would be displayed " +
            "for builders in order to construct other units.")]
        [SerializeField]
        private ProductionAction[] defaultActions;

        [SerializeField] private FactionStartData startData = new()
        {
            PopulationHardLimit = 100,
            PopulationCapacity = 0
        };

        /// <summary>
        /// Gets the initial units available to the faction.
        /// </summary>
        public override UnitQuantity[] GetStartingUnits() => startData.Units;

        /// <summary>
        /// Gets the default production actions available for the faction.
        /// </summary>
        public ProductionAction[] DefaultActions => defaultActions;

        /// <summary>
        /// Sets new default actions for the faction.
        /// </summary>
        public void SetDefaultActions(ProductionAction[] actions)
        {
            defaultActions = actions;
        }

        /// <summary>
        /// Sets new start data for the faction.
        /// </summary>
        public void SetStartData(FactionStartData data)
        {
            startData = data;
        }

        /// <summary>
        /// Configures the player's faction-specific settings, such as resource limits, population,
        /// and starting research upgrades.
        /// </summary>
        /// <param name="player">The player entity to configure.</param>
        public override void ConfigurePlayer(APlayer player)
        {
            if (player.TryGetModule(out ResourceModule resourceModule))
            {
                resourceModule.RemoveAllResources();

                foreach (var max in startData.ResourceCapacities)
                    resourceModule.SetResourceCapacity(max);

                resourceModule.AddResources(startData.Resources);
            }

            if (player.TryGetModule(out PopulationModule populationModule))
            {
                populationModule.SetMaxPopulation(startData.PopulationCapacity);
                populationModule.SetPopulationHardCap(startData.PopulationHardLimit);
            }

            if (player.TryGetModule(out ResearchModule researchModule))
            {
                foreach (ResearchSO research in startData.Researches)
                    researchModule.AddFinishedResearch(research);
            }
        }

    }

}