using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace TRavljen.UnitSystem.Editor
{
    internal class GuideSelectionSection: IGuideSection
    {
        
        public string Title => "Selection";
        public string Id => "_setup_window_selection";

        private GameObject unitSelectionPrefab;
        
        private GameObject demoSelectionPrefab;
        private GameObject integrationSelectionPrefab;

        private GameObject selectionInstance;
        private GameObject unitSelection;

        private readonly List<int> packagesDoneConfiguring;
        private IGuideSection _guideSectionImplementation;

        public GuideSelectionSection(List<int> packagesDoneConfiguring)
        {
            this.packagesDoneConfiguring = packagesDoneConfiguring;
        }

        public void OnEnable()
        {
            // Load prefabs
            demoSelectionPrefab = GuideHelper.LoadPrefab<GameObject>("UnitSystem/Demo/Prefabs/Player/Player Selection.prefab");
            integrationSelectionPrefab = GuideHelper.LoadPrefab<GameObject>("Integration/Prefabs/Player Selection (Unit Selection).prefab");

            unitSelectionPrefab =
                GuideHelper.LoadPrefab<GameObject>("UnitSelection/Prefabs/SelectionSystem.prefab");
            
            // Find existing instances
            if (integrationSelectionPrefab.IsNotNull())
                selectionInstance = GameObject.Find(integrationSelectionPrefab.name);
            
            if (selectionInstance.IsNull() && demoSelectionPrefab.IsNotNull())
                selectionInstance = GameObject.Find(demoSelectionPrefab.name);
            
            if (unitSelectionPrefab.IsNotNull())
                unitSelection = GameObject.Find(unitSelectionPrefab.name);
        }

        public void OnGUI(IGuideLayout layout)
        {
            var integrationPrefabPresent = integrationSelectionPrefab.IsNotNull(); 
            var demoSelectionPrefabPresent = demoSelectionPrefab.IsNotNull();
            
            var mainPlayer = layout.MainPlayer;
            var isMainPlayerSet = mainPlayer.IsNotNull();
            if (!isMainPlayerSet)
            {
                EditorGUILayout.HelpBox("Main player is missing. You must add the main player or reference an existing one.", MessageType.Warning);
            }

            if (integrationPrefabPresent)
            {
                OnIntegrationGUI(layout);
            }
            else if (demoSelectionPrefabPresent)
            {
                OnDemoGUI(mainPlayer);
            }
            else if (isMainPlayerSet) {
                    EditorGUILayout.HelpBox("No prefabs available to add from Integration or Demo folders. " +
                                            "\nIf they were removed or moved, you must add these prefabs manually or manage selection on your own (see online guide).", MessageType.Warning);   
            }
        }

        private void OnIntegrationGUI(IGuideLayout layout)
        {
            var isMainPlayerSet = layout.MainPlayer.IsNotNull();
            
            if (selectionInstance.IsNotNull() && unitSelection.IsNotNull())
            {
                if (packagesDoneConfiguring.Contains(unitSelection.GetInstanceID()))
                {
                    EditorGUILayout.HelpBox("Great, you are all set!", MessageType.Info);
                }
                else
                {
                    EditorGUILayout.HelpBox("Now configure the input and selection area.", MessageType.Info);
                    EditorGUILayout.ObjectField("Unit Selection Package", unitSelection, typeof(GameObject), true);

                    if (GuideHelper.AlignedButton("Done"))
                    {
                        packagesDoneConfiguring.Add(unitSelection.GetInstanceID());
                    }
                }
            }
            else
            {
                EditorGUILayout.HelpBox(
                    "Add player selection features with selection manager and Unit Selection package.",
                    MessageType.Info);

                if (GuideHelper.GUIButton("Add unit selection", isMainPlayerSet))
                {
                    if (selectionInstance.IsNull())
                        selectionInstance = GuideHelper.EditorCreateObject(integrationSelectionPrefab, layout.MainPlayer.transform);
                        
                    if (unitSelection.IsNull())
                    {
                        unitSelection = GuideHelper.EditorCreateObject(unitSelectionPrefab, layout.PackageRoot);
                        Selection.activeGameObject = unitSelection;
                    }
                }
            }
        }

        private void OnDemoGUI(APlayer mainPlayer)
        {
            if (selectionInstance.IsNotNull())
            {
                EditorGUILayout.HelpBox("All set up.", MessageType.Info);
            }
            else
            {
                EditorGUILayout.HelpBox(
                    "Add player selection features.",
                    MessageType.Info);
                    
                if (GuideHelper.GUIButton("Add player selection", mainPlayer.IsNotNull()))
                    selectionInstance = GuideHelper.EditorCreateObject(demoSelectionPrefab, mainPlayer.transform);
            }
        }
    }
}