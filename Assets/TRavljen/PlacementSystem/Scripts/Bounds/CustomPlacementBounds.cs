using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TRavljen.PlacementSystem
{

    /// <summary>
    /// Defines custom bounds for the game object, which is required for placing
    /// an object using <see cref="ObjectPlacement"/>.
    /// </summary>
    public class CustomPlacementBounds : APlacementBounds
    {

        [Tooltip("Specifies center (offset) of the custom bounds in local space.")]
        public Vector3 center;

        [Tooltip("Specifies size of the custom bounds.")]
        public Vector3 size = Vector3.one;

        private Transform _transform;

        public override Bounds PlacementBounds => new Bounds(_transform.position + _transform.rotation * center, size);
        public override Quaternion PlacementRotation => _transform.rotation;

        private void Awake() => _transform = transform;

        private void OnValidate() => _transform = transform;

    }

}