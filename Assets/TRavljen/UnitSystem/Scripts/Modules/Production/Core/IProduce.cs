using UnityEngine;

namespace TRavljen.UnitSystem
{

    /// <summary>
    /// Interface for managing production progress using a delta value, which can represent
    /// real-time updates (e.g., <see cref="Time.deltaTime"/>) or turn-based increments.
    /// </summary>
    public interface IProduce
    {
        /// <summary>
        /// Updates production progress by applying the given delta value.
        /// </summary>
        /// <param name="delta">
        /// The amount to progress production, typically a time value or turn increment.
        /// </param>
        void Produce(float delta);
    }


}