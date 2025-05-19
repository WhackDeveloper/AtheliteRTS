namespace TRavljen.UnitSystem.Combat
{

    using Task;

    /// <summary>
    /// Represents input data required for executing an attack task.
    /// </summary>
    public readonly struct AttackTaskInput : ITaskInput
    {
        /// <summary>
        /// The attack component providing data and behavior for the task.
        /// </summary>
        public readonly UnitAttack attack;

        /// <summary>
        /// Creates a new instance with the specified attack component.
        /// </summary>
        /// <param name="attack">The UnitAttack component initiating the task.</param>
        public AttackTaskInput(UnitAttack attack)
        {
            this.attack = attack;
        }

    }
}