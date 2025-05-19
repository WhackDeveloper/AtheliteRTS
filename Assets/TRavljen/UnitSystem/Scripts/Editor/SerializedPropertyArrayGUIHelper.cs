using System;
using System.Collections.Generic;
using UnityEngine;

namespace TRavljen.UnitSystem.Editor
{
    using UnityEditor;

    /// <summary>
    /// Hides the script name and reference.
    /// </summary>
    internal class HideScriptNameEditor : Editor
    {
        private readonly List<string> filter = new() { "mScript" };

        public override void OnInspectorGUI()
        {
            EditorHelper.RenderProperties(serializedObject, filter);
        }
    }

    /// <summary>
    /// A utility class for rendering and managing GUI elements for serialized arrays in Unity Editor.
    /// </summary>
    public static class SerializedPropertyArrayGUIHelper
    {

        private static Dictionary<object, Editor> editors = new();

        #region Type Name Helpers

        /// <summary>
        /// Extracts the simple name of a type (without namespace).
        /// </summary>
        /// <param name="type">The type to process.</param>
        /// <returns>The simple type name.</returns>
        private static string GetTypeName(Type type)
        {
            string typeString = type.ToString();
            string[] splitName = typeString.Split(".");
            return splitName.Length > 1 ? splitName[^1] : typeString;
        }

        /// <summary>
        /// Modifies a string by inserting spaces before camel-case words.
        /// </summary>
        /// <param name="currentName">The original name.</param>
        /// <returns>The modified name with spaces.</returns>
        private static string InsertSpaceBeforeCamelCase(string currentName)
        {
            string name = currentName;
            for (int index = 0; index < name.Length; index++)
            {
                if (index > 0 && char.IsUpper(name[index]) && char.IsLower(name[index - 1]))
                {
                    name = name.Insert(index, " ");
                    index++;
                }
            }
            return name;
        }

        #endregion

        #region Array Rendering

        /// <summary>
        /// Renders objects in a serialized array with remove buttons and type-specific headers.
        /// </summary>
        /// <param name="objects">The array of objects to render.</param>
        /// <param name="property">The serialized array property.</param>
        /// <param name="target">The target Unity object.</param>
        public static void RenderObjects(object[] objects, SerializedProperty property, UnityEngine.Object target)
        {
            GUIStyle style = new(EditorStyles.foldout);

            for (int index = 0; index < property.arraySize; index++)
            {
                SerializedProperty element = property.GetArrayElementAtIndex(index);
                var obj = objects[index];
                var type = obj.GetType();
                string name = GetTypeName(type);
                bool shouldUseEditor = obj is MonoBehaviour || obj is ScriptableObject;

                EditorGUILayout.BeginHorizontal();
                style.font = element.isExpanded ? EditorStyles.boldFont : EditorStyles.standardFont;

                element.isExpanded = EditorGUILayout.Foldout(element.isExpanded, name, true, style);

                bool itemRemoved = RenderRemoveButton();

                if (itemRemoved)
                {
                    property.DeleteArrayElementAtIndex(index);

                    if (shouldUseEditor)
                    {
                        Undo.DestroyObjectImmediate(obj as UnityEngine.Object);
                    }

                    EditorUtility.SetDirty(target);
                }

                EditorGUILayout.EndHorizontal();

                if (!itemRemoved && element.isExpanded)
                {
                    EditorGUILayout.BeginVertical("box");
                    EditorGUILayout.Space(8);

                    if (shouldUseEditor)
                    {
                        EditorGUI.indentLevel++;
                        if (editors.TryGetValue(obj, out var editor))
                        {
                            editor.OnInspectorGUI();

                            if (editor.serializedObject.ApplyModifiedProperties())
                            {
                                EditorUtility.SetDirty(editor.serializedObject.targetObject);
                                Debug.Log("Setting editor dirty");
                            }
                        }
                        else
                        {
                            editor = Editor.CreateEditor(obj as UnityEngine.Object, typeof(HideScriptNameEditor));
                            editor.OnInspectorGUI();

                            editors.Add(obj, editor);
                        }
                        EditorGUI.indentLevel--;
                    }
                    else
                    {
                        EditorGUI.indentLevel++;

                        // Handle structs and serializable classes
                        SerializedProperty child = element.Copy();
                        SerializedProperty endProperty = child.GetEndProperty();

                        // Iterate over children of the property
                        if (child.NextVisible(true))
                        {
                            do
                            {
                                EditorGUILayout.PropertyField(child, true);
                            }
                            while (child.NextVisible(false) && !SerializedProperty.EqualContents(child, endProperty));
                        }

                        EditorGUI.indentLevel--;
                    }

                    EditorGUILayout.Space(8);
                    EditorGUILayout.EndVertical();
                    EditorGUILayout.Space();
                }
                else
                {
                    EditorGUILayout.Space(24);
                }

            }
        }

        /// <summary>
        /// Renders a remove button for array elements.
        /// </summary>
        /// <param name="property">The serialized array property.</param>
        /// <param name="index">The index of the element to remove.</param>
        /// <param name="target">The target Unity object.</param>
        private static bool RenderRemoveButton()
        {
            bool clicked = false;

            if (GUILayout.Button(new GUIContent("Remove"), GUILayout.Width(80), GUILayout.Height(16)))
            {
                clicked = true;
            }

            return clicked;
        }

        /// <summary>
        /// Cleans up null references in a serialized array property.
        /// </summary>
        /// <param name="property">The serialized array property.</param>
        /// <returns>True if any null references were removed; otherwise, false.</returns>
        public static bool CleanupNulls(SerializedProperty property)
        {
            if (!property.isArray) return false;

            bool setDirty = false;
            for (int index = property.arraySize - 1; index >= 0; index--)
            {
                var element = property.GetArrayElementAtIndex(index);

                if (element.managedReferenceValue == null)
                {
                    property.DeleteArrayElementAtIndex(index);
                    setDirty = true;
                }
            }

            if (setDirty)
            {
                var serializedObject = property.serializedObject;
                serializedObject.ApplyModifiedProperties();
                if (serializedObject.targetObject != null)
                {
                    EditorUtility.SetDirty(serializedObject.targetObject);
                }
            }

            return setDirty;
        }

        #endregion

        #region Add Object Menu

        /// <summary>
        /// Displays a context menu for adding new objects to the serialized array.
        /// </summary>
        /// <typeparam name="ObjectType">The base type of objects to add.</typeparam>
        /// <param name="actionTitle">The title of the action/button.</param>
        /// <param name="existingObjects">The current objects in the array.</param>
        /// <param name="createCallback">The callback to invoke when an object is selected.</param>
        /// <returns>True if the menu was shown; otherwise, false.</returns>
        public static bool ShowAddObjectMenu<ObjectType>(
            string actionTitle,
            object[] existingObjects,
            Action<Type> createCallback,
            Action<List<Type>> createAll)
        {
            return ShowAddObjectMenu<ObjectType>(actionTitle, existingObjects, new(), createCallback, createAll);
        }

        public static bool ShowAddObjectMenu<ObjectType, FilterObjectType>(
          string actionTitle,
          object[] existingObjects,
          Action<Type> createCallback,
          Action<List<Type>> createAll)
        {
            var filterTypes = TypeCache.GetTypesDerivedFrom<FilterObjectType>();
            return ShowAddObjectMenu<ObjectType>(actionTitle, existingObjects, new(filterTypes), createCallback, createAll);
        }

        public static bool ShowAddObjectMenu<ObjectType>(
            string actionTitle,
            object[] existingObjects,
            List<Type> ignoringTypes,
            Action<Type> createCallback,
            Action<List<Type>> createAll)
        {
            List<Type> existingTypes = new List<object>(existingObjects).ConvertAll(obj => obj.GetType());
            List<Type> typesToAdd = new();

            var types = TypeCache.GetTypesDerivedFrom<ObjectType>();

            foreach (var type in types)
            {
                if (!type.IsAbstract && !existingTypes.Contains(type))
                {
                    if (ignoringTypes.Contains(type))
                        continue;

                    typesToAdd.Add(type);
                }
            }

            bool actionEnabled = typesToAdd.Count > 0;

            GUI.enabled = actionEnabled;

            bool clicked = GUILayout.Button(actionTitle);

            GUI.enabled = true;

            if (!clicked) return false;

            static string ModifyName(string typeName) => InsertSpaceBeforeCamelCase(typeName);

            typesToAdd.Sort((a, b) => a.Name.CompareTo(b.Name));

            GenericMenu menu = new();
            foreach (var type in typesToAdd)
            {
                menu.AddItem(new GUIContent(ModifyName(type.Name)), false, () => createCallback(type));
            }

            if (createAll != null)
            {
                menu.AddSeparator("");
                menu.AddItem(new GUIContent("Select all"), false, () =>
                {
                    createAll.Invoke(typesToAdd);
                });
            }

            menu.ShowAsContext();
            return true;
        }

        #endregion
    }


}