using System.Collections.Generic;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace TRavljen.UnitSystem.Editor
{
    internal static class GuideHelper
    {
        
        #region GUI
        
        public static bool GUIButton(string text, bool enabled = true)
        {
            GUI.enabled = enabled;
            bool clicked = GUILayout.Button(text);
            GUI.enabled = true;
            return clicked;
        }
        
        public static bool AlignedButton(string text, bool left = false)
        {
            EditorGUILayout.BeginHorizontal();
            if (!left)
                GUILayout.FlexibleSpace();
            var clicked = GUILayout.Button(text);
            if (left)
                GUILayout.FlexibleSpace();

            EditorGUILayout.EndHorizontal();
            return clicked;
        }
        
        public static bool ListFoldoutGUI<T>(List<T> list, T obj, string id)
        {
            var expanded = list.Contains(obj);
                
            EditorGUILayout.BeginHorizontal();
            var newExpanded = EditorGUILayout.Foldout(expanded, id);
            
            if (obj is Object unityObject)
            {
                GUILayout.FlexibleSpace();
                GUI.enabled = false;
                EditorGUILayout.ObjectField(unityObject, typeof(T), true);
                GUI.enabled = true;
            }

            EditorGUILayout.EndHorizontal();

            if (newExpanded == expanded) return newExpanded;
            
            if (newExpanded)
                list.Add(obj);
            else
                list.Remove(obj);

            return newExpanded;
        }
        
        #endregion
        
        #region Game Objects
        
        public static T EditorCreateObject<T>(T prefab, Transform parent = null) where T : Object
        {
            var instance = (T)PrefabUtility.InstantiatePrefab(prefab, parent);
            
            EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene());
            
            if (instance is MonoBehaviour monoBehaviour)
                Undo.RegisterCreatedObjectUndo(monoBehaviour.gameObject, "Create " + instance);
            else
                Undo.RegisterCreatedObjectUndo(instance, "Create " + instance);

            return instance;
        }
        
        public static T LoadPrefab<T>(string path, string root = "Assets/TRavljen/") where T : Object
        {
            return (T)AssetDatabase.LoadAssetAtPath(root + path, typeof(T));
        }

        #endregion

    }
}