using UnityEditor;

namespace TRavljen.EditorUtility
{
     
    using UnityEngine;

    public class DisableIfAttribute : PropertyAttribute
    {
        public string propertyName;
        public bool value;
    
        public DisableIfAttribute(string propertyName, bool value)
        {
            this.propertyName = propertyName;
            this.value = value;
        }
    }
    
#if UNITY_EDITOR
    [CustomPropertyDrawer(typeof(DisableIfAttribute))]
    public class DisableIfDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            string propertyName = ((DisableIfAttribute)attribute).propertyName;
    
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
    
            var expected = ((DisableIfAttribute)attribute).value;
            var disable = boolProperty.boolValue == expected;
    
            if (disable)
            {
                GUI.enabled = false;
            }
    
            EditorGUI.PropertyField(position, property, label, true);
    
            GUI.enabled = true;
        }
    
    }
#endif

}