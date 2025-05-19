using UnityEngine;

namespace TRavljen.Tooltip.Editor
{
  
    using UnityEditor;
    [CustomEditor(typeof(HoverTooltip))]
    [CanEditMultipleObjects]
    public class HoverTooltipEditor : Editor
    {
        private SerializedProperty delay;
        private SerializedProperty information;
        
        private void OnEnable()
        {
            delay = serializedObject.FindProperty("delay");
            information = serializedObject.FindProperty("information");
        }

        public override void OnInspectorGUI()
        {
            EditorGUILayout.HelpBox("Manages information and hover state of the object. You can set custom type of information in code or set plain text here.", MessageType.Info);

            var manager = Object.FindFirstObjectByType<TooltipManager>();
            
            GUI.enabled = !manager.useGlobalDelay;
            EditorGUILayout.PropertyField(delay);
            GUI.enabled = true;

            if (manager.useGlobalDelay)
            {
                EditorGUILayout.HelpBox("Delay is controlled by the TooltipManager with global delay enabled.", MessageType.None);
            }

            EditorGUILayout.PropertyField(information, true);

            if (serializedObject.ApplyModifiedProperties())
            {
                EditorUtility.SetDirty(target);
            }
        }
    }

}