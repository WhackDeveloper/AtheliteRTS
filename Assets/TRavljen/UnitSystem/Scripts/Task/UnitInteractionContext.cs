namespace TRavljen.UnitSystem.Task
{
    using Interactions;

    /// <summary>
    /// A task context for unit interactions that require a specific interactee target.
    /// </summary>
    public class UnitInteractionContext : ITaskContext
    {
        /// <summary>
        /// Gets the target of the interaction context.
        /// </summary>
        public IUnitInteracteeComponent Target { get; }

        /// <summary>
        /// Specifies if the interaction was commanded by the owner.
        /// </summary>
        public bool Commanded { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="UnitInteractionContext"/> class
        /// with the specified interaction target.
        /// </summary>
        /// <param name="target">The target of the interaction task.</param>
        /// <param name="commanded">If the player/owner has commanded the movement.
        /// Some tasks might behave differently based on this.</param>
        public UnitInteractionContext(IUnitInteracteeComponent target, bool commanded = false)
        {
            Target = target;
            Commanded = commanded;
        }
    }

}