namespace TRavljen.UnitSystem
{

    using UnityEngine;

    /// <summary>
    /// Core scriptable object for units, representing producibles that are spawned into the Scene.
    /// </summary>
    /// <remarks>
    /// Any producible that generates a game object should derive from this class. 
    /// Unit components and capabilities are primarily tied to this abstract class.
    /// For usage in custom implementations, see <see cref="UnitSO"/>.
    /// </remarks>
    public abstract class AUnitSO : AEntitySO
    {

        [Header("Unit Base")]

        [Tooltip("Defines the types associated with this unit.")]
        [SerializeField]
        private UnitTypeSO[] types = new UnitTypeSO[0];

        /// <summary>
        /// Gets the array of unit types associated with this unit.
        /// </summary>
        public UnitTypeSO[] UnitTypes => types;

        /// <summary>
        /// Determines if the unit matches any of the specified types.
        /// </summary>
        /// <param name="types">The types to check against.</param>
        /// <returns>
        /// Returns true if the unit matches any of the specified types; otherwise, false.
        /// </returns>
        public bool DoesMatchAnyType(UnitTypeSO[] types)
        {
            return UnitTypeSO.DoesMatchAnyType(types, UnitTypes);
        }

        #region IPrefabCreatable

        /// <summary>
        /// Creates a prefab instance of this unit with the specified name.
        /// </summary>
        /// <param name="name">The name of the prefab instance.</param>
        /// <returns>The created prefab game object.</returns>
        public override GameObject CreatePrefab(string name)
        {
            return CreatePrefab<Unit>(name);
        }

        #endregion

    }

}