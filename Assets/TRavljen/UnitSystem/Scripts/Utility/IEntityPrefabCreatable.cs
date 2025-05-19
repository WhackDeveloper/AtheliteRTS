using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TRavljen.UnitSystem
{

    /// <summary>
    /// Provided interface for creating a prefab from a <see cref="AEntitySO"/>
    /// in the Unit System Manager Window.
    /// </summary>
    public interface IEntityPrefabCreatable
    {

        /// <summary>
        /// Creates a new template prefab instance with a specified name.
        /// </summary>
        /// <param name="name">Name of the prefab</param>
        /// <returns>Returns the newly instantiated prefab</returns>
        GameObject CreatePrefab(string name);

        /// <summary>
        /// Configure game object prefab.
        /// </summary>
        /// <param name="prefab">Prefab to configure</param>
        void ConfigurePrefab(GameObject prefab);

    }

}