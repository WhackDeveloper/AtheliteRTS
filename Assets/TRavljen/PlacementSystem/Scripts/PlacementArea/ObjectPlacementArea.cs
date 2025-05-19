using UnityEngine;

namespace TRavljen.PlacementSystem
{

#if UNITY_EDITOR
    using UnityEditor;
    using EditorUtility;
    
    [CustomEditor(typeof(ObjectPlacementArea), true)]
    public sealed class ObjectPlacementAreaEditor : HiddenScriptPropertyEditor
    {
        public override void OnInspectorGUI()
        {
            EditorGUILayout.HelpBox("This component manages adding and removing placement are on " + 
                                    "ObjectPlacementAreaManager when enabled or disabled. " +
                                    "\n \nTo manage when placement area is added, simply enable and disable this component.", MessageType.Info);
            
            base.OnInspectorGUI();
        }
    }
#endif
    
    /// <summary>
    /// Component which specifies a valid placement area for a game object.
    /// This component manages adding and removing placement are on
    /// <see cref="ObjectPlacementAreaManager"/> when enabled or disabled.
    /// To manage when placement area is added, simply enable and disable this component.
    /// </summary>
    [DisallowMultipleComponent]
    public class ObjectPlacementArea : MonoBehaviour
    {
        [Tooltip("Specifies area bounds provided by the game object. Position is relative.")]
        [SerializeField] private SphereBounds areaBounds;

        public SphereBounds WorldBounds
        {
            get
            {
                var position = transform.TransformPoint(areaBounds.center);
                return new SphereBounds(position, areaBounds.radius);
            }
        }
        
        private void Start()
        {
            if (TryGetManager(out var manager))
            {
                var bounds = WorldBounds;
                manager.AddObjectArea(gameObject, bounds.center, bounds.radius);
            }
            else
            {
                Debug.LogError("Object placement is missing ObjectPlacementAreaManager component as customArea for placement.");
            }
        }

        private void OnEnable()
        {
            if (!TryGetManager(out var manager)) return;

            var bounds = WorldBounds;
            manager.AddObjectArea(gameObject, bounds.center, bounds.radius);
        }
        
        private void OnDisable()
        {
            if (!TryGetManager(out var manager)) return;
            
            manager.RemoveObjectArea(gameObject);
        }

        private bool TryGetManager(out ObjectPlacementAreaManager manager)
        {
            if (ObjectPlacement.Instance != null &&
                ObjectPlacement.Instance.PlacementArea.customArea is ObjectPlacementAreaManager _manager)
            {
                manager = _manager;
                return true;
            }

            manager = null;
            return false;
        }
        
        #if UNITY_EDITOR

        [Header("Preview")]
        [SerializeField] private Color gizmoColor = Color.red;
        [SerializeField] private bool disableGizmo;
        
        private void OnDrawGizmos()
        {
            if (UnityEditor.Selection.activeObject != gameObject || disableGizmo) return;

            Gizmos.color = gizmoColor;
            var bounds = WorldBounds;
            Gizmos.DrawWireSphere(bounds.center, bounds.radius);
        }
        
        #endif
    }
}