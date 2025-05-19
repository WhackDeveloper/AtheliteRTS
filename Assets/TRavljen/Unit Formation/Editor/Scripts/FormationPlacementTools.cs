using UnityEngine;

namespace TRavljen.UnitFormation.Editor
{
    using UnityEditor;

    public sealed class FormationPlacementTools
    {
        [MenuItem("Tools/TRavljen/Add Editor Formation Placement")]
        private static void AddEditorFormationPlacement()
        {
            GameObject newGameObject = new GameObject("EditorFormationPlacement");

            newGameObject.AddComponent<EditorFormationPlacement>();

            bool wasNoneSelected = Selection.activeGameObject == null;
            Selection.activeGameObject = newGameObject;

            // Focus the scene view on the new GameObject
            if (wasNoneSelected && SceneView.lastActiveSceneView != null)
            {
                SceneView.lastActiveSceneView.FrameSelected();
            }

            Debug.Log("Editor Formation Placement added to the scene. Happy moving!");
        }
    }
}