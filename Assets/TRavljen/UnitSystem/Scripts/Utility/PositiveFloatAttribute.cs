using UnityEngine;

namespace TRavljen.UnitSystem
{

    /// <summary>
    /// Prevents negative value for <see cref="float"/> types.
    /// </summary>
    public class PositiveFloatAttribute : PropertyAttribute
    {
        public PositiveFloatAttribute() { }
    }

}

#if UNITY_EDITOR
namespace TRavljen.UnitSystem
{
    using UnityEditor;

    [CustomPropertyDrawer(typeof(PositiveFloatAttribute))]
    public class PositiveFloatAttributeDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            if (property.floatValue < 0)
            {
                property.floatValue = 0;
            }

            EditorGUI.PropertyField(position, property, label);
        }
    }

}
#endif