using System;
using System.Collections.Generic;

namespace TRavljen.UnitSystem.Editor
{
    using UnityEngine;
    using UnityEditor;
    using TRavljen.EditorUtility;
    
    public class SceneSetupGuideWindow : EditorWindow, IGuideLayout
    {

        #region Properties
        
        [SerializeField] 
        private List<int> packagesDoneConfiguring = new();

        [SerializeField]
        private Vector2 scrollPosition = Vector2.zero;
        
        [SerializeField]
        private GameObject packagesRoot;

        [SerializeField]
        private GuideMainPlayerSection mainPlayerSection;
        
        [SerializeField]
        private GuideAdditionalStuff additionalStuff;
        
        private readonly List<IGuideSection> _sections = new();

        public APlayer MainPlayer => mainPlayerSection.GetMainPlayer();
        public Transform PackageRoot => GetPackagesRoot();

        #endregion
        
        [MenuItem("Tools/TRavljen/Scene Wizard")]
        public static void ShowWindow()
        {
            GetWindow<SceneSetupGuideWindow>("Scene Wizard");
        }

        private void OnEnable()
        {
            mainPlayerSection ??= new GuideMainPlayerSection();
            additionalStuff ??= new GuideAdditionalStuff();
            
            _sections.Clear();
            _sections.Add(mainPlayerSection);
            _sections.Add(new GuideSelectionSection(packagesDoneConfiguring));
            _sections.Add(new GuideInteractionSection(packagesDoneConfiguring));
            _sections.Add(new GuideCameraSection(packagesDoneConfiguring));
            _sections.Add(new GuideGameUISection());
            _sections.Add(additionalStuff);
            
            foreach (var section in _sections)
                section.OnEnable();
        }

        private void OnGUI()
        {
            scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);

            HeaderGUI();
            
            EditorGUILayout.Space(12);
            
            for (var index = 0; index < _sections.Count; index++)
            {
                var section = _sections[index];
                MainFoldoutGUI(index + 1, section);
            }
            
            EditorGUILayout.EndScrollView();
        }

        private void HeaderGUI()
        {
            EditorGUILayout.HelpBox("Here you can follow steps for a quick scene set up by using Unit System with demo or integration prefabs.", MessageType.None);
            
            if (EditorGUILayout.LinkButton("Visit online guide for full details"))
                Application.OpenURL("https://travljen.gitbook.io/unit-system/");
        }

        #region Utility

        private Transform GetPackagesRoot()
        {
            if (packagesRoot.IsNull())
                packagesRoot = GameObject.Find("Packages");
            
            if (packagesRoot.IsNull())
                packagesRoot = new GameObject("Packages");

            return packagesRoot.transform;
        }

        private void MainFoldoutGUI(int step, IGuideSection section)
        {
            MainFoldoutGUI($"{step}. " + section.Title, section.Id, () =>
            {
                section.OnGUI(this);
            });
        }
        
        private void MainFoldoutGUI(string title, string id, Action content)
        {
            if (!PersistentFoldout.Foldout(title, id)) return;
            
            EditorGUILayout.BeginVertical(GUI.skin.box);
            EditorGUI.indentLevel++;
            content.Invoke();
            EditorGUI.indentLevel--;
            EditorGUILayout.EndVertical();
        }
        
        #endregion
    }

}