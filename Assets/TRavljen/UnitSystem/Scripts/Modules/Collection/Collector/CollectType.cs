using UnityEngine;

namespace TRavljen.UnitSystem.Collection
{

    /// <summary>
    /// Enum specifying different types of collection behaviors for resource collectors.
    /// </summary>
    public enum CollectType
    {
        /// <summary>
        /// Collects resources directly into the player's resource pool without requiring transport or capacity management.
        /// </summary>
        RealtimeCollect,

        /// <summary>
        /// Collects resources into the collector's storage until capacity is reached, then automatically adds them 
        /// to the player's resource pool without requiring transport.
        /// </summary>
        StackAndCollect,

        /// <summary>
        /// Collects resources into the collector's storage and requires transporting them to a resource depot for depositing.
        /// </summary>
        GatherAndDeposit
    }

}