using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace TRavljen.UnitSystem.Editor
{
    internal class GuideInteractionSection: IGuideSection
    {
        public string Title => "Interactions";
        public string Id => "_setup_window_interactions";

        private GameObject demoInteractionsPrefab;
        private GameObject integrationInteractionsPrefab;
        
        private GameObject objectPlacementPrefab;
        private GameObject formationPlacementPrefab;
        
        private GameObject interactionsInstance;
        private GameObject formationPlacement;
        private GameObject objectPlacement;
        
        private readonly List<int> packagesDoneConfiguring;
        private IGuideSection _guideSectionImplementation;

        public GuideInteractionSection(List<int> packagesDoneConfiguring)
        {
            this.packagesDoneConfiguring = packagesDoneConfiguring;
        }

        public void OnEnable()
        {
            demoInteractionsPrefab = GuideHelper.LoadPrefab<GameObject>("UnitSystem/Demo/Prefabs/Player/Player Interactions.prefab");
            integrationInteractionsPrefab = GuideHelper.LoadPrefab<GameObject>("Integration/Prefabs/Player Interactions (Object Placement + Formation).prefab");
            
            objectPlacementPrefab =
                GuideHelper.LoadPrefab<GameObject>("Integration/Prefabs/Object Placement.prefab");
            formationPlacementPrefab =
                GuideHelper.LoadPrefab<GameObject>("Unit Formation/Prefabs/FormationPlacement.prefab");
            
            if (integrationInteractionsPrefab.IsNotNull())
                interactionsInstance = GameObject.Find(integrationInteractionsPrefab.name);

            if (interactionsInstance.IsNull() && demoInteractionsPrefab.IsNotNull())
                interactionsInstance = GameObject.Find(demoInteractionsPrefab.name);

            if (formationPlacementPrefab.IsNotNull())
                formationPlacement = GameObject.Find(formationPlacementPrefab.name);
            
            if (objectPlacementPrefab.IsNotNull())
                objectPlacement = GameObject.Find(objectPlacementPrefab.name);
        }

        public void OnGUI(IGuideLayout layout)
        {
             var demoPrefabPresent = demoInteractionsPrefab.IsNotNull();
             var integrationPrefabPresent = integrationInteractionsPrefab.IsNotNull() 
                                            && formationPlacementPrefab.IsNotNull()
                                            && objectPlacementPrefab.IsNotNull();

            var isMainPlayerSet = layout.MainPlayer.IsNotNull();

            if (!isMainPlayerSet)
            {
                EditorGUILayout.HelpBox("Main player is missing. You must add the main player or reference an existing one.", MessageType.Warning);
            }

            if (integrationPrefabPresent)
            {
                var interactionsMissing = interactionsInstance.IsNull();
                var formationMissing = formationPlacement.IsNull();
                var placementMissing = objectPlacement.IsNull();

                if (interactionsMissing || formationMissing || placementMissing)
                {
                    EditorGUILayout.HelpBox(
                        "Add player interaction features for placing objects, commanding units and calculating their formation.\n \n" +
                        "Player-unit interactions provide commanding features for selected units. It also provides integration scripts for " +
                        "using formation and object placement packages.",
                        MessageType.Info);

                    GUI.enabled = isMainPlayerSet;
                    if (interactionsMissing && GUILayout.Button("Add player-unit interactions"))
                        interactionsInstance =
                            GuideHelper.EditorCreateObject(integrationInteractionsPrefab, layout.MainPlayer.transform);

                    if (formationMissing && GUILayout.Button("Add formation placement"))
                    {
                        formationPlacement = GuideHelper.EditorCreateObject(formationPlacementPrefab, layout.PackageRoot);
                        Selection.activeObject = formationPlacement;
                    }

                    if (placementMissing && GUILayout.Button("Add object placement"))
                    {
                        objectPlacement = GuideHelper.EditorCreateObject(objectPlacementPrefab, layout.PackageRoot);
                        Selection.activeObject = objectPlacement;
                    }

                    GUI.enabled = true;
                }
                else
                {
                    if (packagesDoneConfiguring.Contains(formationPlacement.GetInstanceID())
                        && packagesDoneConfiguring.Contains(objectPlacement.GetInstanceID()))
                    {
                        EditorGUILayout.HelpBox("You are all set for player interactions!", MessageType.Info);
                    }
                    else
                    {
                        EditorGUILayout.HelpBox("Now configure the input on Object & Formation Placement for to your project needs.", MessageType.Info);
                        EditorGUILayout.ObjectField(formationPlacement, typeof(GameObject), true);
                        EditorGUILayout.ObjectField(objectPlacement, typeof(GameObject), true);

                        if (GuideHelper.AlignedButton("Done"))
                        {
                            packagesDoneConfiguring.Add(formationPlacement.GetInstanceID());
                            packagesDoneConfiguring.Add(objectPlacement.GetInstanceID());
                        }
                    }

                }
            }
            else if (demoPrefabPresent)
            {
                if (interactionsInstance.IsNotNull())
                {
                    EditorGUILayout.HelpBox("All set up.", MessageType.Info);
                }
                else
                {
                    EditorGUILayout.HelpBox(
                        "Add player interaction features for commanding units.",
                        MessageType.Info);

                    if (GuideHelper.GUIButton("Add player interactions", isMainPlayerSet))
                    {
                        interactionsInstance =
                            GuideHelper.EditorCreateObject(demoInteractionsPrefab, layout.MainPlayer.transform);
                        Selection.activeObject = interactionsInstance;
                    }
                }
            }
            else if (isMainPlayerSet)
            {
                EditorGUILayout.HelpBox("No prefabs available to add from Integration or Demo folders. " +
                                        "\nIf they were removed or moved, you must add these prefabs manually or manage interactions on your own", MessageType.Warning);   
            }
            
            GUI.enabled = true;
        }
    }

}