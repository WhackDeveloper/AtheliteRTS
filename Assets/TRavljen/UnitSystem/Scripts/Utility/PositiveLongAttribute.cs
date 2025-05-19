using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TRavljen.UnitSystem
{

    /// <summary>
    /// Prevents negative value for <see cref="long"/> types.
    /// </summary>
    public class PositiveLongAttribute : PropertyAttribute
    {
        public PositiveLongAttribute() { }
    }

}

#if UNITY_EDITOR
namespace TRavljen.UnitSystem
{
    using UnityEditor;

    [CustomPropertyDrawer(typeof(PositiveLongAttribute))]
    public class PositiveLongAttributeDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            if (property.longValue < 0)
            {
                property.longValue = 0;
            }

            EditorGUI.PropertyField(position, property, label);
        }
    }

}
#endif
