using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TRavljen.UnitSystem
{

    /// <summary>
    /// Provides extension methods for managing components and child GameObjects in Unity.
    /// </summary>
    public static class GameObjectHelper
    {

        /// <summary>
        /// Adds a component of the specified type to the GameObject if it does not already exist.
        /// </summary>
        /// <typeparam name="Component">The type of the component to add.</typeparam>
        /// <param name="gameObject">The GameObject to which the component will be added.</param>
        /// <returns>The existing component if present, otherwise the newly added component.</returns>
        public static Component AddComponentIfNotPresent<Component>(this GameObject gameObject) where Component : MonoBehaviour
        {
            if (gameObject.TryGetComponent(out Component existingComponent))
                return existingComponent;
            return gameObject.AddComponent<Component>();
        }

        /// <summary>
        /// Adds a component of the specified type to the GameObject or its children if it does not already exist.
        /// </summary>
        /// <typeparam name="Component">The type of the component to add.</typeparam>
        /// <param name="gameObject">The GameObject to which the component will be added.</param>
        /// <param name="includesChildren">If true, checks the GameObject and all its children for the component.</param>
        /// <returns>
        /// The existing component if present in the GameObject or its children, otherwise the newly added component.
        /// </returns>
        public static Component AddComponentIfNotPresent<Component>(this GameObject gameObject, bool includesChildren) where Component : MonoBehaviour
        {
            if (includesChildren)
            {
                var components = gameObject.GetComponentsInChildren<Component>();
                if (components.Length > 0) return components[0];
            }
            else
            {
                if (gameObject.TryGetComponent(out Component existingComponent))
                    return existingComponent;
            }

            return gameObject.AddComponent<Component>();
        }

        /// <summary>
        /// Creates a new child GameObject under the specified parent GameObject.
        /// </summary>
        /// <param name="parent">The parent GameObject to which the child will be added.</param>
        /// <param name="childName">The name to assign to the new child GameObject.</param>
        /// <returns>The newly created child GameObject.</returns>
        public static GameObject AddChildGameObject(this GameObject parent, string childName)
        {
            GameObject child = new GameObject(childName);
            child.transform.SetParent(parent.transform);
            child.transform.localPosition = Vector3.zero; // Or set as needed
            return child;
        }

    }

}