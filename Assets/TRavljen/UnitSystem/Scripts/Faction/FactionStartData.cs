using UnityEngine;
using System.Collections;
using UnityEngine.Serialization;

namespace TRavljen.UnitSystem
{

    /// <summary>
    /// Represents the initial data configuration for a faction, 
    /// including starting population, resources, and unit limits.
    /// </summary>
    [System.Serializable]
    public struct FactionStartData
    {
        [Header("Population")]

        [Tooltip("The maximum population capacity of the faction. This is a hard limit of the faction.")]
        public int PopulationHardLimit;

        [Tooltip("The additional starting population capacity of the faction.\n" +
                 "This does not include the capacity contributed by starting units.")]
        public int PopulationCapacity;

        [Header("Units & Resources")]

        [Tooltip("The initial units available to the faction.")]
        [SerializeField]
        private UnitQuantity[] units;

        [Tooltip("The initial resources available to the faction.")]
        [SerializeField]
        private ResourceQuantity[] resources;

        [Tooltip("The maximum starting resources allowed for the faction.")]
        [SerializeField]
        private ResourceQuantity[] resourceCapacities;

        [Tooltip("The initial researches already done by the faction.")]
        [SerializeField]
        private ResearchSO[] researches;

        /// <summary>
        /// Gets the initial units available to the faction.
        /// </summary>
        public readonly UnitQuantity[] Units => units;

        /// <summary>
        /// Gets the initial resources available to the faction.
        /// </summary>
        public readonly ResourceQuantity[] Resources => resources;

        /// <summary>
        /// Gets the maximum starting resources allowed for the faction.
        /// </summary>
        public readonly ResourceQuantity[] ResourceCapacities => resourceCapacities;

        /// <summary>
        /// The initial researches already done by the faction.
        /// </summary>
        public readonly ResearchSO[] Researches => researches;

    }

}
