#if UNITY_EDITOR
namespace TRavljen.EditorUtility
{
    using UnityEditor;

    public class PersistentFoldout
    {
        public static bool Foldout(string title, string id = null, bool groupHeader = true)
        {
            if (title == null) return false;
            
            var key = "foldout_section_" + title;
            var currentState = EditorPrefs.GetBool(key, false);
            bool newState;
            if (groupHeader)
            {
                newState = EditorGUILayout.BeginFoldoutHeaderGroup(currentState, title);
                EditorGUILayout.EndFoldoutHeaderGroup();
            }
            else
            {
                newState = EditorGUILayout.Foldout(currentState, title, true);
            }

            if (newState != currentState)
                EditorPrefs.SetBool(key, newState);

            return newState;

        }
    }
}
#endif