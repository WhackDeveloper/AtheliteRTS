using UnityEngine;

namespace TRavljen.PlacementSystem
{
    
    [RequireComponent(typeof(ObjectPlacement))]
    public abstract class ASpherePlacementAreas : MonoBehaviour, IPlacementArea
    {

        #region Properties
        
        [HideInInspector]
        [SerializeField] protected ObjectPlacement placement;
        
        [Tooltip("Specifies if the boundsType should be overridden. Leave this disabled and " +
                 "manually control the type of the placement area.")]
        [SerializeField] protected bool overrideType;

        [Header("Preview")]
        [SerializeField] protected Color gizmosColor = Color.red;
        [SerializeField] protected bool alwaysShowGizmos;

        protected abstract SphereBounds[] GetAreas();
        
        #endregion
        
        #region Lifecycle

        protected virtual void Awake()
        {
            var area = placement.PlacementArea;
            if (overrideType)
                area.boundsType = PlacementAreaType.Custom;
            
            area.customArea = this;
            placement.PlacementArea = area;
        }
        
        protected virtual void OnValidate()
        {
            placement ??= GetComponent<ObjectPlacement>();
        }
        
        #endregion
        
        #region IPlacementArea
        
        Vector3 IPlacementArea.ClosestPosition(Vector3 position)
        {
            var areas = GetAreas();
            if (areas.Length <= 0) return position;
    
            SphereBounds closestArea = default;
            var closestDistance = float.MaxValue;
    
            foreach (var area in areas)
            {
                // Either its inside or get the closest position
                if (area.Contains(position))
                {
                    closestArea = area;
                    var closestPosition = area.ClosestPoint(position);
                    closestDistance = Vector3.Distance(position, closestPosition);
                }
                else
                {
                    var closestPosition = area.ClosestPoint(position);
                    var distance = Vector3.Distance(position, closestPosition);
                    
                    if (closestDistance > distance)
                    {
                        closestDistance = distance;
                        closestArea = area;
                    }
                }
            }
            
            // Clamps position if bounding box is enabled.
            return closestArea.ClosestPoint(position);
        }
        
        bool IPlacementArea.IsInside(Vector3 position)
        {
            var areas = GetAreas();
            if (areas.Length == 0) return false;
            
            var isInsideAnyBounds = false;
            foreach (var bounds in GetAreas())
            {
                if (bounds.Contains(position))
                {
                    isInsideAnyBounds = true;
                }
            }
    
            return isInsideAnyBounds;
        }
        
        #endregion

        protected virtual void OnDrawGizmos()
        {
#if UNITY_EDITOR
            if (!alwaysShowGizmos && UnityEditor.Selection.activeObject != gameObject) return;
#endif
            Gizmos.color = gizmosColor;
            
            foreach (var area in GetAreas())
            {
                Gizmos.DrawWireSphere(area.center, area.radius);
            }
        }
    }
}