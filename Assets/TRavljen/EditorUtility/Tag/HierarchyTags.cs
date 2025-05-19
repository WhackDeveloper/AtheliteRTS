using UnityEngine;

namespace TRavljen.EditorUtility.Tag
{
    
    #if UNITY_EDITOR
    using UnityEditor;
    
    [InitializeOnLoad]
    public static class HierarchyTags
    {
        
        private static readonly Color DefaultBackground = new Color(0.2f, 0.6f, 0.3f);
            
        static HierarchyTags()
        {
            EditorApplication.hierarchyWindowItemOnGUI += DrawTag;
        }

        private static void DrawTag(int instanceID, Rect selectionRect)
        {
            var obj = EditorUtility.InstanceIDToObject(instanceID) as GameObject;
            if (obj == null) return;
            
            if (!obj.TryGetComponent(out IHierarchyTag tag))
            {
                DrawDefaultTag(obj, selectionRect);
                return;
            }
            
            tag.DrawTag(selectionRect);
        }

        private static void DrawDefaultTag(GameObject obj, Rect selectionRect)
        {
            var name = obj.name;
            if (!name.Contains("----")) return;

            name = name.Replace("--", "").Trim();
            
            DrawTag(selectionRect, name, Color.white, DefaultBackground);
        }
        
        #region Helper
        
        public static void DrawTag(
            Rect rect, 
            string label,
            Color textColor,
            Color backgroundColor,
            FontStyle fontStyle = FontStyle.Bold, 
            TextAnchor textAnchor = TextAnchor.MiddleCenter)
        {
            EditorGUI.DrawRect(rect, backgroundColor);

            EditorGUI.LabelField(rect, label, new GUIStyle
            {
                normal = new GUIStyleState { textColor = textColor },
                fontStyle = fontStyle,
                alignment = textAnchor
            });
        }
        
        #endregion
    }
    #endif

    /// <summary>
    /// Defines contract for rendering custom hierarchy window item.
    /// </summary>
    public interface IHierarchyTag
    {
        /// <summary>
        /// Method invoked for rendering custom hierarchy window item.
        /// </summary>
        /// <param name="rect"></param>
        public void DrawTag(Rect rect);
    }
}