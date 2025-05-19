using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Spectator
{

    /// <summary>
    /// Component for rendering Gizmos and simple information about
    /// the <see cref="SpectatorPlayer"/> that is present on the
    /// game object.
    /// </summary>
    [ExecuteInEditMode, RequireComponent(typeof(SpectatorPlayer))]
    public class DebugRenderer : MonoBehaviour
    {

        #region Public Members

        /// <summary>
        /// Specifies if the Gizmos will be rendered.
        /// </summary>
        public bool ShowGizmos = true;

        /// <summary>
        /// Specifies the Gizmo color that will be used to draw top and bottom
        /// cubes for the <see cref="AllowedAreaBounds"/>.
        /// </summary>
        public Color AllowedAreaGizmoColor = new Color() { r = 1, g = 0.58f, a = 1 };

        /// <summary>
        /// Specifies the Gizmo color that will be used to render cube around
        /// selected game object.
        /// </summary>
        public Color SelectionGizmoColor = new Color() { b = 1, g = 0.58f, a = 1 };

        /// <summary>
        /// Specifies if the GUI text will be rendered on the top
        /// left side of the screen.
        /// </summary>
        public bool RenderGUI = true;

        /// <summary>
        /// Specifies the color of the rendered debug text.
        /// </summary>
        public Color TextColor = Color.black;

        #endregion

        private SpectatorPlayer _spectator;
        private FollowCamera _followCamera;

        void Awake()
        {
            _spectator = GetComponent<SpectatorPlayer>();
            _followCamera = GetComponent<FollowCamera>();
        }

        private void OnGUI()
        {
            if (!RenderGUI) return;

            var text = "Camera \n";

            if (_spectator.SpectatorEnabled)
            {
                text += "Position: " + _spectator.TargetCamera.transform.position + "\n" +
                    "Local rotation: " + _spectator.TargetCamera.transform.localRotation + "\n";

                bool contentAdded = false;

                if (_spectator.SelectedObject)
                {
                    text += "\nSelected target\n";

                    var distance = Vector3.Distance(
                        _spectator.SelectedObject.transform.position,
                        _spectator.TargetCamera.transform.position);

                    text += "Name: " + _spectator.SelectedObject.name + "\n";
                    text += "Distance: " + distance + "\n";
                    contentAdded = true;
                }

                if (_spectator.FollowTarget && _followCamera)
                {
                    text += "\nFollow target\n";

                    var distance = Vector3.Distance(
                        _spectator.FollowTarget.transform.position + _followCamera.TargetOffset,
                        _spectator.TargetCamera.transform.position);

                    text += "Name: " + _spectator.FollowTarget.name + "\n";
                    text += "Distance: " + distance + "\n";
                    contentAdded = true;
                }

                if (!contentAdded)
                {
                    text += "\nNo selection or follow";
                }
            }
            else
            {
                text += "DISABLED";
            }

            GUIStyle style = new GUIStyle();
            style.normal.textColor = TextColor;
            style.fontSize = 24;

            GUI.Label(new Rect(16, 16, 400, 500), text, style);
        }

        void OnDrawGizmos()
        {
            // Check if gizmos are disabled or this feature is disabled
            if (!ShowGizmos || !_spectator.SpectatorEnabled) return;

            DrawSelectedObjectGizmo();
            DrawRestrictionAreaGizmo();
        }

        private void DrawRestrictionAreaGizmo()
        {
            if (_spectator.Options.RestrictPosition)
            {
                Gizmos.color = AllowedAreaGizmoColor;
                var bounds = _spectator.Options.AllowedAreaBounds;
                var size = new Vector3(bounds.size.x, 0f, bounds.size.z);
                var center = bounds.center;

                // Render bottom
                center.y -= bounds.size.y / 2f;
                Gizmos.DrawWireCube(center, size);

                // Render top
                center.y += bounds.size.y;
                Gizmos.DrawWireCube(center, size);
            }
        }

        private void DrawSelectedObjectGizmo()
        {
            var selectedObject = _spectator.SelectedObject;

            if (selectedObject)
            {
                Gizmos.color = SelectionGizmoColor;
                var center = selectedObject.bounds.center;

                // Where its possible, draw custom wire frame for collider for
                // the selected object.
                if (selectedObject is SphereCollider sphere)
                {
                    Gizmos.DrawWireSphere(center, sphere.radius);
                }
                else if (selectedObject is MeshCollider mesh)
                {
                    Gizmos.DrawWireMesh(mesh.sharedMesh);
                }
                else
                {
                    Gizmos.DrawWireCube(center, selectedObject.bounds.size);
                }

                // Draw sphere at the center of the objects collider in order to
                // "highlight" the object
                Gizmos.DrawSphere(center, 0.2f);
            }
        }
    }

}