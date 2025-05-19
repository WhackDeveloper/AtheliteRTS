using TRavljen.EditorUtility;
using UnityEngine;

namespace TRavljen.UnitSystem
{

    /// <summary>
    /// Base class for ScriptableObjects that require a unique identifier (UUID). 
    /// Identifiers are used to efficiently match and compare items, avoiding reliance 
    /// on multiple property checks, object references, or other complex comparisons.
    /// </summary>
    /// <remarks>
    /// Unique IDs are useful in systems where objects are referenced frequently 
    /// or compared against each other, such as inventory management or production systems.
    /// By default, IDs are initialized to an invalid value and must be assigned explicitly.
    /// </remarks>
    public class ManagedSO : ScriptableObject
    {
        /// <summary>
        /// Represents an invalid ID value. Used to signify that an ID has not been assigned.
        /// </summary>
        public static readonly int invalidId = -1;

        /// <summary>
        /// Gets the unique identifier (UUID) for this object.
        /// </summary>
        /// <remarks>
        /// This value is private and serialized, ensuring it cannot be directly modified in the Unity Editor.
        /// It can only be assigned programmatically using <see cref="AssignUniqueID(int)"/>.
        /// </remarks>
        public int ID => uniqueID;

        /// <summary>
        /// The unique identifier for the object.
        /// </summary>
        /// <remarks>
        /// The ID is initially set to <see cref="invalidId"/> and must be explicitly assigned 
        /// using <see cref="AssignUniqueID(int)"/>. 
        /// Changing this value directly is not recommended.
        /// </remarks>
        [SerializeField, DisableInInspector, Tooltip("Unique ID for the ScriptableObject. Do not change this manually.")]
        private int uniqueID = invalidId;

        /// <summary>
        /// Invalidates the current ID, setting it back to <see cref="invalidId"/>.
        /// </summary>
        /// <remarks>
        /// This method is useful when an ID needs to be reset or marked as unassigned, 
        /// such as during testing or reassignment workflows.
        /// </remarks>
        public void InvalidateID() => uniqueID = invalidId;

        /// <summary>
        /// Assigns a new unique ID to this object.
        /// </summary>
        /// <param name="id">The unique ID to assign.</param>
        /// <remarks>
        /// This method ensures that a valid ID is assigned to the object. 
        /// IDs must be managed externally to guarantee uniqueness across objects.
        /// </remarks>
        public void AssignUniqueID(int id)
        {
            uniqueID = id;
        }
    }

}
