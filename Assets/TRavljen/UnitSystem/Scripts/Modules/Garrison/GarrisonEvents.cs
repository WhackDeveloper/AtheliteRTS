using UnityEngine;
using UnityEngine.Events;

namespace TRavljen.UnitSystem.Garrison
{

    /// <summary>
    /// Manages events related to garrisons.
    /// </summary>
    public class GarrisonEvents
    {

        public static GarrisonEvents Instance { get; private set; } = new();

        [Tooltip("Event invoked when a unit enters the garrison.")]
        public UnityEvent<IGarrisonEntity, IGarrisonableUnit> OnUnitEnterGarrison = new();

        [Tooltip("Event invoked when a unit exits the garrison.")]
        public UnityEvent<IGarrisonEntity, IGarrisonableUnit> OnUnitExitGarrison = new();

    }

}