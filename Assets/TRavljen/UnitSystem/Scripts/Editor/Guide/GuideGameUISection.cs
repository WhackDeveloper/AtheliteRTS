using UnityEditor;
using UnityEngine;

namespace TRavljen.UnitSystem.Editor
{
    
    internal sealed class GuideGameUISection : IGuideSection
    {
        public string Title => "Demo UI";
        public string Id => "_setup_window_ui_demo";

        private GameObject uiInstance;
        private GameObject playerUIPrefab;

        public void OnEnable()
        {
            playerUIPrefab = GuideHelper.LoadPrefab<GameObject>("UnitSystem/Demo/Prefabs/Player/Player UI.prefab");

            if (playerUIPrefab.IsNotNull())
                uiInstance = GameObject.Find(playerUIPrefab.name);
        }

        public void OnGUI(IGuideLayout layout)
        {
            if (uiInstance.IsNotNull())
            {
                EditorGUILayout.HelpBox("All set up.", MessageType.Info);
                return;
            }

            var isMainPlayerSet = layout.MainPlayer.IsNotNull();
            
            if (!isMainPlayerSet)
            {
                EditorGUILayout.HelpBox("Main player is missing. You must add the main player or reference an existing one.", MessageType.Warning);
            }

            if (GuideHelper.GUIButton("Add Demo UI", isMainPlayerSet))
            {
                uiInstance = GuideHelper.EditorCreateObject(playerUIPrefab, layout.MainPlayer.transform);
            }
        }
    }
}