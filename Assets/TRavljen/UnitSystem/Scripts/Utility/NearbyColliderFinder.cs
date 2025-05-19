using System.Collections.Generic;
using UnityEngine;

namespace TRavljen.UnitSystem.Utility
{
    
    /// <summary>
    /// Generic wrapper for using <see cref="Physics"/> sphere overlaps.
    /// Can use non allocating methods, based on configuration.
    /// </summary>
    [System.Serializable]
    public class NearbyColliderFinder
    {
        public delegate bool Validate<in TComponentType>(TComponentType component);

        [SerializeField]
        private LayerMask layerMask = ~0;

        private bool _useNonAllocating = false;
        private Collider[] _colliders;

        public NearbyColliderFinder() {}

        public NearbyColliderFinder(LayerMask layerMask, bool useNonAllocating = false, int allocationSize = 100)
        {
            this.layerMask = layerMask;
            _useNonAllocating = useNonAllocating;

            if (useNonAllocating)
            {
                _colliders = new Collider[allocationSize];   
            }
        }

        public NearbyColliderFinder(LayerMask layerMask)
        {
            this.layerMask = layerMask;
        }

        public TComponentType[] FindNearby<TComponentType>(Vector3 position, float range,
            Validate<TComponentType> validation)
        {
            return FindNearby(position, position, range, validation);
        }

        public TComponentType[] FindNearby<TComponentType>(Vector3 position, Vector3 distanceFromPosition, float range,
            Validate<TComponentType> validation)
        {
            Collider[] colliders;
            var size = 0;
            
            if (_useNonAllocating)
            {
                size = Physics.OverlapSphereNonAlloc(position, range, this._colliders, layerMask);
                colliders = this._colliders;
            }
            else
            {
                colliders = Physics.OverlapSphere(position, range, layerMask);
                size = colliders.Length;
            }
            
            List<(TComponentType node, float distance)> result = new();

            for (var index = 0; index < size; index++)
            {
                var collider = colliders[index];
                
                if (collider.TryGetComponent(out TComponentType component) &&
                    validation.Invoke(component))
                {
                    result.Add((component, Vector3.Distance(distanceFromPosition, collider.transform.position)));
                }
            }

            result.Sort((a, b) => a.distance.CompareTo(b.distance));

            return result.ConvertAll(item => item.node).ToArray();
        }
        
        public bool FindClosest<TComponentType>(Vector3 position, Vector3 distanceFromPosition, float range, out TComponentType result, Validate<TComponentType> validation)
        {
            var nearbyResults = FindNearby(position, distanceFromPosition, range, validation);

            if (nearbyResults.Length > 0)
            {
                result = nearbyResults[0];
                return true;
            }

            result = default;
            return false;
        }

        public bool FindClosest<TComponentType>(Vector3 position, float range, out TComponentType result, Validate<TComponentType> validation)
        {
            return FindClosest(position, position, range, out result, validation);
        }
    }
    
}
