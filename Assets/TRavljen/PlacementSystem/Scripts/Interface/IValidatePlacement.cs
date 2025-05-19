using UnityEngine;

namespace TRavljen.PlacementSystem
{
    /// <summary>
    /// Interface for additional custom validation for object's placement.
    /// You can implement this to verify if certain objects can be placed
    /// in certain locations, under specific rotation.
    /// </summary>
    public interface IValidatePlacement
    {
        /// <summary>
        /// Validates placement of a game object.
        /// </summary>
        /// <param name="gameObject">Game object to validate</param>
        /// <param name="bounds">Bounds of the game object</param>
        /// <param name="rotation">Rotation of bounds in world space</param>
        /// <returns>Returns true if object has been validated and can be placed,
        /// returns false when it cannot be placed.</returns>
        public bool IsPlacementValid(GameObject gameObject, Bounds bounds, Quaternion rotation);
    }
}