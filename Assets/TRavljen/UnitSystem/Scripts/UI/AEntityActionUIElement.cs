namespace TRavljen.UnitSystem
{

    /// <summary>
    /// Represents a UI element capable of performing an action.
    /// </summary>
    public abstract class AEntityActionUIElement : AEntityUIElement
    {
        /// <summary>
        /// Assigns an action to this UI element.
        /// </summary>
        /// <param name="action">The action to be executed when interacted with.</param>
        public abstract void SetAction(IEntityUIAction action);

        /// <summary>
        /// Checks if the assigned action can be triggered via a hotkey.
        /// </summary>
        /// <returns>True if a valid hotkey is pressed, otherwise false.</returns>
        public abstract bool CheckActionHotKey();
    }

    /// <summary>
    /// Defines an interface for an actionable entity UI element.
    /// </summary>
    public interface IEntityUIAction
    {
        /// <summary>
        /// Executes the assigned action on the specified entity.
        /// </summary>
        /// <param name="entity">The entity on which the action will be performed.</param>
        void Execute(IEntity entity);
    }

}