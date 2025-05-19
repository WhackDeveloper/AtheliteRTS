using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace TRavljen.PlacementSystem
{

    #if UNITY_EDITOR
    using UnityEditor;

    [CustomEditor(typeof(ObjectPlacementAreaManager), true)]
    class ObjectPlacementAreaManagerEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();
            
            EditorGUILayout.HelpBox("Manage objects manually by adding or removing them from the manager.\n" +
                                    "You can also attach ObjectPlacementArea component on those objects and they will manage themselves.", MessageType.Info);
        }
    }
    #endif
    
    /// <summary>
    /// Component which manages a list of objects and their placement areas.
    /// </summary>
    public class ObjectPlacementAreaManager : ASpherePlacementAreas
    {
        
        private readonly Dictionary<GameObject, SphereBounds> _objectAreas = new();
        private SphereBounds[] areas = new SphereBounds[0];

        [SerializeField] private ObjectPlacementArea[] predefinedAreas;

        protected override SphereBounds[] GetAreas() => areas;

        protected override void Awake()
        {
            base.Awake();

            if (predefinedAreas == null) return;
            
            foreach (var area in predefinedAreas)
            {
                var bounds = area.WorldBounds;
                AddObjectArea(area.gameObject, bounds.center, bounds.radius);
            }

            predefinedAreas = null;
        }

        /// <summary>
        /// Add area for the game object.
        /// </summary>
        /// <param name="obj">Area game object owner</param>
        /// <param name="position">Center world position.</param>
        /// <param name="radius">Radius</param>
        public void AddObjectArea(GameObject obj, Vector3 position, float radius)
        {
            if (_objectAreas.ContainsKey(obj))
                _objectAreas.Remove(obj);
            
            _objectAreas.Add(obj, new SphereBounds(position, radius));
            areas = _objectAreas.Values.ToArray();
        }

        /// <summary>
        /// Remove area for the game object.
        /// </summary>
        /// <param name="obj">Area game object owner.</param>
        public void RemoveObjectArea(GameObject obj)
        {
            _objectAreas.Remove(obj);
            areas = _objectAreas.Values.ToArray();
        }

        protected override void OnDrawGizmos()
        {
            base.OnDrawGizmos();

            if (predefinedAreas == null) return;

            foreach (var area in predefinedAreas)
            {
                var bounds = area.WorldBounds;
                Gizmos.color = gizmosColor;
                Gizmos.DrawWireSphere(bounds.center, bounds.radius);
            }
        }
    }
    
}