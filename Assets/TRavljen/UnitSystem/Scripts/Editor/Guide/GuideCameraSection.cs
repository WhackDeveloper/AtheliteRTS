
using System.Collections.Generic;

namespace TRavljen.UnitSystem.Editor
{
    using UnityEditor;
    using UnityEngine;
    
    internal class GuideCameraSection : IGuideSection
    {
        public string Title => "Camera";
        public string Id => "_setup_window_camera";

        private GameObject spectator;
        private GameObject spectatorPrefab;

        private GameObject cameraController;
        private GameObject cameraControllerPrefab;

        private List<int> packagesDoneConfiguring;

        public GuideCameraSection(List<int> packagesDoneConfiguring)
        {
            this.packagesDoneConfiguring = packagesDoneConfiguring;
        }

        public void OnEnable()
        {
            spectatorPrefab =
                GuideHelper.LoadPrefab<GameObject>("Integration/Prefabs/Spectator Player.prefab");

            cameraControllerPrefab =
                GuideHelper.LoadPrefab<GameObject>("UnitSystem/Demo/Prefabs/Player/Camera Controller.prefab");
            
            if (spectator.IsNull() && spectatorPrefab.IsNotNull())
                spectator = GameObject.Find(spectatorPrefab.name);
            
            if (cameraController.IsNull() && cameraControllerPrefab.IsNotNull())
                cameraController = GameObject.Find(cameraControllerPrefab.name);
        }

        public void OnGUI(IGuideLayout layout)
        {
            if (spectatorPrefab.IsNull() && cameraControllerPrefab.IsNull())
            {
                return;
            }

            if (spectatorPrefab.IsNotNull())
            {
                OnSpectatorCameraGUI(layout);
            }
            else if (cameraControllerPrefab.IsNotNull())
            {
                OnCameraControllerGUI();
            }
            else
            {
                EditorGUILayout.HelpBox("Prefabs are missing for the camera controls, cannot configure using this window. \n \n" +
                                        "Add SpectatorPlayer or CameraController component on a game object in your scene and adjust its configurations.", MessageType.Warning);
            }
        }

        private void OnSpectatorCameraGUI(IGuideLayout layout)
        {
            if (spectator.IsNull())
            {
                EditorGUILayout.HelpBox(
                    "You can set up camera controller for the player here, using Spectator Player package.",
                    MessageType.Info);

                if (GUILayout.Button($"Add camera controls"))
                {
                    spectator = GuideHelper.EditorCreateObject(spectatorPrefab, layout.PackageRoot);
                    Selection.activeObject = spectator;
                }
            }
            else if (packagesDoneConfiguring.Contains(spectator.GetInstanceID()))
            {
                EditorGUILayout.HelpBox("You are all done.", MessageType.Info);
            }
            else
            {
                spectator = (GameObject)EditorGUILayout.ObjectField(spectator, typeof(GameObject), false);
                EditorGUILayout.HelpBox(
                    $"Spectator camera controls added. Adjust the input & behaviour for your project.",
                    MessageType.None);

                if (GuideHelper.AlignedButton("Done"))
                {
                    packagesDoneConfiguring.Add(spectator.GetInstanceID());
                }
            }
        }

        private void OnCameraControllerGUI()
        {
            if (cameraController.IsNull())
            {
                EditorGUILayout.HelpBox(
                    "You can set up camera controller for the player here, using CameraController prefab.",
                    MessageType.Info);

                if (GUILayout.Button($"Add camera controls"))
                {
                    cameraController = GuideHelper.EditorCreateObject(cameraControllerPrefab, null);
                    Selection.activeObject = cameraController;
                }
            }
            else if (packagesDoneConfiguring.Contains(cameraController.GetInstanceID()))
            {
                EditorGUILayout.HelpBox("You are all done.", MessageType.Info);
            }
            else
            {
                cameraController = (GameObject)EditorGUILayout.ObjectField(cameraController, typeof(GameObject), false);
                EditorGUILayout.HelpBox(
                    $"Camera controller added. Adjust the configuration for your project.",
                    MessageType.None);

                if (GuideHelper.AlignedButton("Done"))
                {
                    packagesDoneConfiguring.Add(cameraController.GetInstanceID());
                }
            }
        }
        
    }
}