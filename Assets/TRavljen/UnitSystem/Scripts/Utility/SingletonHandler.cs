using UnityEngine;

namespace TRavljen.UnitSystem
{

    /// <summary>
    /// A utility class for creating and managing singleton instances of MonoBehaviour types.
    /// </summary>
    /// <remarks>
    /// This class ensures only one instance of a given singleton type exists during runtime.
    /// It is designed to handle singletons dynamically at runtime and includes safeguards 
    /// against creating new instances after the application quits.
    /// </remarks>
    public static class SingletonHandler
    {
        /// <summary>
        /// Tracks if the application is in the process of quitting.
        /// </summary>
        private static bool isApplicationQuitting;

        /// <summary>
        /// Creates a new singleton instance of the specified MonoBehaviour type, if one does not already exist.
        /// </summary>
        /// <typeparam name="SingletonType">The type of the MonoBehaviour to instantiate as a singleton.</typeparam>
        /// <param name="objectName">The name to assign to the newly created singleton GameObject.</param>
        /// <returns>
        /// A singleton instance of the specified type, or <c>null</c> if the application is quitting.
        /// </returns>
        /// <remarks>
        /// This method will create a new GameObject to house the singleton instance if one does not already exist.
        /// </remarks>
        public static SingletonType Create<SingletonType>(string objectName) where SingletonType : MonoBehaviour
        {
            if (isApplicationQuitting)
                return null;

            var container = new GameObject(objectName);
            var singleton = container.AddComponent<SingletonType>();
            
            #if UNITY_EDITOR
            UnityEditor.Undo.RegisterCreatedObjectUndo(container, "Create " + objectName);
            UnityEditor.EditorUtility.SetDirty(singleton);
            #endif

            return singleton;
        }

        /// <summary>
        /// Registers the application quitting event to handle cleanup logic.
        /// </summary>
        [RuntimeInitializeOnLoadMethod]
        public static void RunOnStart()
        {
            Application.quitting += HandleAppQuit;
        }

        /// <summary>
        /// Marks the application as quitting to prevent new singleton instances from being created.
        /// </summary>
        private static void HandleAppQuit()
        {
            isApplicationQuitting = true;
        }
    }

}