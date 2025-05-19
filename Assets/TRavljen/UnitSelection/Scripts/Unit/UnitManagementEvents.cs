namespace TRavljen.UnitSelection
{
    using UnityEngine.Events;

    /// <summary>
    /// Provides events for managing unit lifecycle events within the game. This class is essential
    /// for cleaning up no longer valid unit references in the selection systems.
    /// <para>
    /// If <see cref="ManageUnitObject"/> component is used, this is handled internally.
    /// Read <see cref="OnUnitRemoved"/> documentation for further information.
    /// </para>
    /// </summary>
    public static class UnitManagementEvents
    {

        /// <summary>
        /// Event triggered to clean up references in the selection system when a unit is destroyed
        /// or deactivated for reuse, such as being returned to an object pool. Properly invoking this
        /// event prevents null reference exceptions by ensuring that all references to the selectable
        /// game object are correctly cleared.
        /// <para>
        /// Usage:
        /// <code>
        /// UnitManagementEvents.OnUnitRemoved.AddListener(unit => HandleUnitRemoval(unit));
        /// </code>
        /// </para>
        /// <para>
        /// Note: Failing to invoke this event after a unit is destroyed or deactivated can lead to
        /// null reference exceptions if other parts of the system attempt to access the now-invalid
        /// unit.
        /// </para>
        /// </summary>
        public readonly static UnityEvent<ISelectable> OnUnitRemoved = new UnityEvent<ISelectable>();

    }
}