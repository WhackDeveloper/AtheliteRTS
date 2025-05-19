namespace TRavljen.UnitSelection
{
    using UnityEngine;

    /// <summary>
    /// Selection filter for prioritizing player-owned units using the
    /// <see cref="UnitOwnership"/> component.
    /// Automatically filters out unowned units if both are selected.
    /// </summary>
    public class PlayerOwnedUnitFilter : APlayerSelectionFilter
    {
        protected override bool IsOwnedByPlayer(ISelectable selectable)
        {
            if (selectable == null) return false;

            // Try getting the ownership component
            if (selectable.gameObject.TryGetComponent(out UnitOwnership ownership))
                return ownership.State == UnitOwnershipState.Player;

            // Handle selectable groups
            if (selectable is ISelectableGroup group && group.GroupUnits.Count > 0)
            {
                return group.GroupUnits[0].gameObject.TryGetComponent(out ownership) &&
                       ownership.State == UnitOwnershipState.Player;
            }

            Debug.LogWarning($"[PlayerOwnedUnitFilter] {selectable} does not have a PlayerOwnedUnit component.");
            return false;
        }
    }

}