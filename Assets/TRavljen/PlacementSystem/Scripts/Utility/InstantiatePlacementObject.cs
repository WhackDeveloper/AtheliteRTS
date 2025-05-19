using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TRavljen.PlacementSystem
{

    /// <summary>
    /// Helper methods for instantiatin a new object template used for visual
    /// placement.
    /// </summary>
    public static class InstantiatePlacementObject
    {
        public static GameObject TryInstantiate(GameObject prefab, Vector3 position, Quaternion rotation, Transform parent, out IPlacementBounds bounds)
        {
            if (prefab == null)
                throw new System.NullReferenceException("Object to place was not set yet or was null! " +
                    "If not set in editor, use the method that receives a game object to place");

            GameObject newInstance = GameObject.Instantiate(
                prefab,
                position,
                rotation,
                parent);

            // Disables all colliders on the placing object, they should not be part of detection.
            foreach (var collider in newInstance.GetComponentsInChildren<Collider>())
                collider.enabled = false;

            // In case it's inactive
            newInstance.SetActive(true);

            if (!TryCreatePlacementBounds(newInstance, out bounds))
            {
                GameObject.Destroy(newInstance);
                return null;
            }

            return newInstance;
        }

        public static bool TryCreatePlacementBounds(GameObject newInstance, out IPlacementBounds placementBounds)
        {
            if (newInstance.TryGetComponent(out IPlacementBounds _bounds))
            {
                placementBounds = _bounds;
            }
            // Attempt to fallback on other options
            else if (newInstance.TryGetComponent(out BoxCollider mainCollider))
            {
                ColliderPlacementBounds bounds = newInstance.gameObject.AddComponent<ColliderPlacementBounds>();
                bounds.SetCollider(mainCollider);
                placementBounds = bounds;
            }
            else if (newInstance.TryGetComponent(out Renderer mainRenderer))
            {
                RendererPlacementBounds bounds = newInstance.gameObject.AddComponent<RendererPlacementBounds>();
                bounds.SetRenderer(mainRenderer);
                placementBounds = bounds;
            }
            else
            {
                placementBounds = default;
                return false;
            }

            return true;
        }
    }

}