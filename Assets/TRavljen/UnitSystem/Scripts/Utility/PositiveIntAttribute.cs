using UnityEngine;

namespace TRavljen.UnitSystem
{

    /// <summary>
    /// Prevents negative value for <see cref="int"/> types.
    /// </summary>
    public class PositiveIntAttribute : PropertyAttribute
    {
        public PositiveIntAttribute() { }
    }

}

#if UNITY_EDITOR
namespace TRavljen.UnitSystem
{
    using UnityEditor;

    [CustomPropertyDrawer(typeof(PositiveIntAttribute))]
    public class PositiveIntAttributeDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            if (property.intValue < 0)
            {
                property.intValue = 0;
            }

            EditorGUI.PropertyField(position, property, label);
        }
    }

}
#endif
