using UnityEngine;
using UnityEditor;

namespace TRavljen.EditorUtility
{

    /// <summary>
    /// Disables property editing in Unity Editor Inspector,
    /// but still allows it to be visible or editable in
    /// DEBUG Inspector view.
    /// </summary>
    public class DisableInInspectorAttribute : PropertyAttribute { }

#if UNITY_EDITOR
    [CustomPropertyDrawer(typeof(DisableInInspectorAttribute))]
    public class DisableInInspectorDrawer : PropertyDrawer
    {

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            // Disable GUI while rendering this property and it's children.
            GUI.enabled = false;
            EditorGUI.PropertyField(position, property, label, true);
            GUI.enabled = true;
        }

    }
#endif

}
