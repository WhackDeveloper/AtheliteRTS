using UnityEngine;

namespace TRavljen.UnitSelection
{

    /// <summary>
    /// Interface used for interacting with a selectable game object within the
    /// scene. This component is a requirement for selection system to work
    /// on the game objects. See available implementation <see cref="SelectableUnit"/>.
    /// </summary>
    public interface ISelectable
    {

        /// <summary>
        /// Provides access to game object. Does not need additional changes
        /// when implemented by <see cref="MonoBehaviour"/>.
        /// </summary>
        public GameObject gameObject { get; }

        /// <summary>
        /// Returns true if the unit is marked as selected.
        /// Unit can be marked as highlighted and selected at the same time.
        /// </summary>
        public bool IsSelected { get; }

        /// <summary>
        /// Returns true if the unit is marked as highlighted.
        /// Unit can be marked as highlighted and selected at the same time.
        /// </summary>
        public bool IsHighlighted { get; }

        /// <summary>
        /// Implement this for selected action.
        /// </summary>
        public void Select();

        /// <summary>
        /// Implement this for deselect action.
        /// </summary>
        public void Deselect();

        /// <summary>
        /// Implement this for highlight action. This can also be invoked
        /// when unit is already selected.
        /// </summary>
        public void Highlight();

        /// <summary>
        /// Implement this for unhighlight action.
        /// </summary>
        public void Unhighlight();

    }

}