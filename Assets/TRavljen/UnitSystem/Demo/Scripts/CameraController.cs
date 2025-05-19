using UnityEngine;

namespace TRavljen.UnitSystem.Demo
{
    
    #if UNITY_EDITOR
    using UnityEditor;
    using TRavljen.EditorUtility;
    internal static class CameraControllerEditorTools
    {
        [MenuItem("GameObject/TRavljen/UnitSystem/Camera Controller")]
        public static void CreateUnitSelectorInScene()
        {
            if (EditorTools.CreateObjectFromMenu<CameraController>("Camera Controller", true))
            {
                Debug.Log("New camera controller created.");
            }
        }
    }
    #endif

    /// <summary>
    /// Provides a basic camera movement controller for top-down games.
    /// Allows smooth movement based on user input, with optional bounds restriction.
    /// </summary>
    public class CameraController : MonoBehaviour
    {

        #region Properties

        [SerializeField] private Camera _camera;
        
        [Tooltip("Speed of the camera movement in units per second.")]
        [SerializeField, Range(5, 100)]
        private float moveSpeed = 15;

        [Tooltip("Determines whether the camera movement is restricted to a specified area.")]
        [SerializeField]
        private bool restrictMovement = true;
        
        /// <summary>
        /// Defines the bounds within which the camera movement is restricted.
        /// Only applicable if <see cref="restrictMovement"/> is enabled.
        /// </summary>
        [Tooltip("Defines the bounds within which the camera movement is restricted. Only applicable if `restrictMovement` is enabled.")]
        [SerializeField]
        private Bounds allowedArea = new(new(0, 50, 0), new(500, 50, 500));

        /// <summary>
        /// Key bindings for moving the camera forward.
        /// Default keys: W and UpArrow.
        /// </summary>
        [Tooltip("Key bindings for moving the camera forward.")]
        [SerializeField]
        private KeyCode[] forwardKeys = { KeyCode.W, KeyCode.UpArrow };

        /// <summary>
        /// Key bindings for moving the camera backward.
        /// Default keys: S and DownArrow.
        /// </summary>
        [Tooltip("Key bindings for moving the camera backward.")]
        [SerializeField]
        private KeyCode[] backwardKey = { KeyCode.S, KeyCode.DownArrow };

        /// <summary>
        /// Key bindings for moving the camera left.
        /// Default keys: A and LeftArrow.
        /// </summary>
        [Tooltip("Key bindings for moving the camera left.")]
        [SerializeField]
        private KeyCode[] leftKeys = { KeyCode.A, KeyCode.LeftArrow };

        /// <summary>
        /// Key bindings for moving the camera right.
        /// Default keys: D and RightArrow.
        /// </summary>
        [Tooltip("Key bindings for moving the camera right.")]
        [SerializeField]
        private KeyCode[] rightKeys = { KeyCode.D, KeyCode.RightArrow };

        /// <summary>
        /// Key bindings for moving the camera up (elevation).
        /// Default key: E.
        /// </summary>
        [Tooltip("Key bindings for moving the camera up (Y axis).")]
        [SerializeField]
        private KeyCode[] upKeys = { KeyCode.E };

        /// <summary>
        /// Key bindings for moving the camera down (elevation).
        /// Default key: Q.
        /// </summary>
        [Tooltip("Key bindings for moving the camera down (Y axis).")]
        [SerializeField]
        private KeyCode[] downKeys = { KeyCode.Q };
        
        private Transform _cameraTransform;

        #endregion

        #region Lifecycle

        private void OnValidate()
        {
            if (_camera != null) return;
            
            var newCamera = Camera.main;
            if (newCamera != null && newCamera.gameObject.scene == gameObject.scene)
            {
                _camera = newCamera;
            }
        }

        private void Awake()
        {
            if (_camera == null)
                _camera = Camera.main;
            
            _cameraTransform = _camera?.transform;
        }

        /// <summary>
        /// Updates the camera's position based on input.
        /// Applies movement restrictions if <see cref="restrictMovement"/> is enabled.
        /// </summary>
        private void Update()
        {
            Vector3 moveDirection = GetNormalizedInput();
            Vector3 speed = Time.deltaTime * moveSpeed * moveDirection;
            // Consider camera's Y rotation only.
            Vector3 cameraRotation = Quaternion.Euler(0, _cameraTransform.eulerAngles.y, 0) * speed;
            Vector3 newPosition = _cameraTransform.position + cameraRotation;

            if (restrictMovement && !allowedArea.Contains(newPosition))
            {
                newPosition = allowedArea.ClosestPoint(newPosition);

                // Recalculate moveDirection for a smoother movement along the boundary
                Vector3 correctedDirection = (newPosition - _cameraTransform.position).normalized;

                // Adjust the movement speed along the corrected direction
                speed = Time.deltaTime * moveSpeed * correctedDirection;
                newPosition = _cameraTransform.position + speed;
            }

            _cameraTransform.position = newPosition;
        }

        #endregion

        #region Input

        /// <summary>
        /// Checks if any of the specified keys are currently being held down.
        /// </summary>
        /// <param name="keyCodes">Array of keys to check.</param>
        /// <returns>True if any key in the array is held down; otherwise, false.</returns>
        private bool IsAnyKeyHeldDown(KeyCode[] keyCodes)
        {
            foreach (KeyCode key in keyCodes)
            {
                if (Input.GetKey(key))
                    return true;
            }

            return false;
        }

        /// <summary>
        /// Gets normalized direction set by the current input.
        /// </summary>
        /// <returns>Returns normalized direction based on input.</returns>
        private Vector3 GetNormalizedInput()
        {
            Vector3 moveDirection = Vector3.zero;

            if (IsAnyKeyHeldDown(forwardKeys))
                moveDirection.z += 1;

            if (IsAnyKeyHeldDown(backwardKey))
                moveDirection.z -= 1;

            if (IsAnyKeyHeldDown(rightKeys))
                moveDirection.x += 1;

            if (IsAnyKeyHeldDown(leftKeys))
                moveDirection.x -= 1;

            if (IsAnyKeyHeldDown(upKeys))
                moveDirection.y += 1;

            if (IsAnyKeyHeldDown(downKeys))
                moveDirection.y -= 1;

            moveDirection.Normalize();
            return moveDirection;
        }

        #endregion

        /// <summary>
        /// Visualizes the camera's movement bounds in the editor when selected.
        /// </summary>
        private void OnDrawGizmosSelected()
        {
            if (!restrictMovement) return;
            
            var color = Color.blue;
            color.a = 0.4f;
            Gizmos.color = color;

            Gizmos.DrawCube(allowedArea.center, allowedArea.size);
        }
    }
}