using System;

namespace TRavljen.EditorUtility.Tag
{
    using UnityEngine;
    
    /// <summary>
    /// Simple customisation tag for Unity's Editor game object inspector visuals.
    /// </summary>
    [DisallowMultipleComponent]
    public class HierarchyTag : MonoBehaviour, IHierarchyTag
    {
#if UNITY_EDITOR
        [SerializeField] private string customName = "";
        [SerializeField] private Color textColor = Color.white;
        [SerializeField] private FontStyle fontStyle = FontStyle.Bold;
        [SerializeField] private TextAnchor textAnchor = TextAnchor.MiddleCenter;
        [SerializeField] private Color backgroundColor = new Color(0.2f, 0.6f, 0.3f);
#endif

#if !UNITY_EDITOR
        private void Awake()
        {
            Destroy(this);
        }
#endif

        public void DrawTag(Rect rect)
        {
#if UNITY_EDITOR
            var label = customName.Length > 0 ? customName : name.Trim();
            HierarchyTags.DrawTag(rect, label, textColor, backgroundColor, fontStyle, textAnchor);
#endif
        }
    }
}