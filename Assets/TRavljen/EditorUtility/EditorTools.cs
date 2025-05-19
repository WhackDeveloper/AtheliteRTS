#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

namespace TRavljen.EditorUtility
{
    public static class EditorTools
    {
        public static bool CreateObjectFromMenu<T>(string name, bool checkForExistingInstance) where T : MonoBehaviour
        {
            return CreateObjectFromMenu<T>(name, checkForExistingInstance, out _);
        }
        
        public static bool CreateObjectFromMenu<T>(string name, bool checkForExistingInstance, out T createdObject) where T : MonoBehaviour
        {
            if (checkForExistingInstance)
            {
                var existingObject = Object.FindFirstObjectByType<T>();

    if (existingObject != null)
            {
        Selection.activeObject = existingObject;
        Debug.LogError($"There is already a {name} object in your scene.");
        createdObject = existingObject;
        return false;
         }
            }

            var obj = new GameObject(name);
            createdObject = obj.AddComponent<T>();
            
            if (Selection.activeGameObject != null)
            {
                obj.transform.parent = Selection.activeGameObject.transform;
            }
            
            Selection.activeObject = obj;
            Undo.RegisterCreatedObjectUndo(obj.gameObject, $"Create {name}");
            return true;
        }
    }
}
#endif