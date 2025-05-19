using UnityEngine;

namespace IntegrationDemo
{

    using TRavljen.UnitSelection;
    using TRavljen.UnitSystem;

    /// <summary>
    /// Filters selectable entities to include only those owned by the assigned player.
    /// This is an integration demo implementation of <see cref="APlayerSelectionFilter"/>.
    /// </summary>
    /// <remarks>
    /// This filter ensures that only entities owned by the specified player are considered 
    /// during selection. Extend or modify this class to accommodate additional filtering 
    /// requirements specific to your game logic.
    /// </remarks>
    public class MultiSelectionFilter : APlayerSelectionFilter
    {

        [Tooltip("Specifies the player whose entities will pass the selection filter.")]
        [SerializeField]
        private APlayer player;

        private void OnValidate()
        {
            if (player == null)
                player = GetComponentInParent<APlayer>();
        }

        protected override bool IsOwnedByPlayer(ISelectable selectable)
        {
            return selectable.gameObject.TryGetComponent(out IEntity entity) &&
                entity.Owner == player;
        }

    }
}