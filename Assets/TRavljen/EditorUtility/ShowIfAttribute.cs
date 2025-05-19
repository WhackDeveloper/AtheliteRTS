namespace TRavljen.EditorUtility
{
    
    using UnityEngine;
    using UnityEditor;
    
    public class ShowIfAttribute : PropertyAttribute
    {
        public string propertyName;
        public bool value;
    
        public ShowIfAttribute(string propertyName, bool value)
        {
            this.propertyName = propertyName;
            this.value = value;
        }
    }
    
#if UNITY_EDITOR
    [CustomPropertyDrawer(typeof(ShowIfAttribute))]
    public class ShowIfDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            var showIfAttribute = (ShowIfAttribute)attribute;
            var propertyName = showIfAttribute.propertyName;
    
            SerializedObject serializedObject = property.serializedObject;
            SerializedProperty boolProperty = serializedObject.FindProperty(propertyName);
    
            if (boolProperty == null)
            {
                Debug.LogError($"Property '{propertyName}' not found on the parent object.");
                GUI.enabled = false;
                EditorGUI.PropertyField(position, property, label, true);
                GUI.enabled = true;
                return;
            }
    
            var expected = showIfAttribute.value;
            var hide = boolProperty.boolValue != expected;

            if (hide) return;
    
            EditorGUI.PropertyField(position, property, label, true);
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            var showIfAttribute = (ShowIfAttribute)attribute;
            var boolProperty = property.serializedObject.FindProperty(showIfAttribute.propertyName);
            var hide = boolProperty.boolValue != showIfAttribute.value;
            
            if (hide) return 0;
            
            return property.isExpanded ? EditorGUI.GetPropertyHeight(property, label) : EditorGUIUtility.singleLineHeight;
        }
    }
#endif
}