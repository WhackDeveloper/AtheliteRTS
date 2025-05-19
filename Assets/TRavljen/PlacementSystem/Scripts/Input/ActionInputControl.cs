
#if ENABLE_INPUT_SYSTEM
namespace TRavljen.PlacementSystem
{
    using UnityEngine.InputSystem;
    using UnityEngine;

#if UNITY_EDITOR
    using EditorUtility;
    using UnityEditor;
    [CustomEditor(typeof(ActionInputControl))]
    public class ActionInputControlEditor : HiddenScriptPropertyEditor { }
#endif

    /// <summary>
    /// Behaviour component for defining players input by using Unity's new
    /// <see cref="UnityEngine.InputSystem"/>. This component is only available
    /// when the package for New Input system is in the project.
    /// </summary>
    public class ActionInputControl : AInputControl, ICursorPositionProvider
    {
        #region Properties

        enum CursorPositionType
        {
            /// <summary>
            /// Uses current position of the mouse cursor.
            /// </summary>
            Mouse,
            /// <summary>
            /// Uses current touch position on screen.
            /// </summary>
            TouchScreen
        }

        [Header("General")]
        [Tooltip("Specifies the action used for canceling active placement.")]
        [Space]
        public InputAction cancelAction;

        [Space]
        [Tooltip("Specifies the action for starting placement process with " +
            "currently selected game object prefab.")]
        public InputAction toggleActivePlacementAction;

        [Space]
        [Tooltip("Specifies the action for finishing placement process " +
            "(placing object in world).")]
        public InputAction finishPlacementAction;

        [Header("Rotation")]
        [Space]
        [Tooltip("Specifies the clockwise rotation input action. You can change " +
            "rotation behaviour from <b>Press</b> to <b>Hold</b> by changing the " +
            "Action Type from <b>Button</b> to <b>Passthrough</b> or <b>Value</b>.")]
        public InputAction rotateClockwiseAction;

        [SerializeField, Space]
        [Tooltip("Specifies the counter clockwise rotation input action. " +
            "You can change rotation behaviour from <b>Press</b> to <b>Hold</b> by " +
            "changing the Action Type from <b>Button</b> to <b>Passthrough</b> or " +
            "<b>Value</b>.")]
        public InputAction rotateCounterClockwiseAction;

        [Header("Prefab Control")]
        [Space]
        public InputAction nextPrefabAction;
        public InputAction previousPrefabAction;
        [Space]
        public InputAction[] prefabActions;

        // Editor version
        [Tooltip("Specifies the type of cursor position behaviour. By default " +
            "mouse position is read, alternatively touch screen position can be " +
            "used for mobile devices and such.")]
        [SerializeField, Header("Cursor")]
        private CursorPositionType cursorPositionType = CursorPositionType.Mouse;

        [SerializeField, HideInInspector]
        private CursorPositionType _cursorPositionType = CursorPositionType.Mouse;

        [Tooltip("Specifies the cursor position. Most commonly this would be " +
            "mouse position or touch screen position. Alternatively custom one " +
            "can be added for others like controller sticks.")]
        [Space]
        public InputAction cursorPositionAction;

        // Internal setup flag.
        [SerializeField, HideInInspector]
        private bool _initialSetup = true;

        public Vector3 CursorPosition {
            get
            {
                return cursorPositionAction.ReadValue<Vector2>();
            }
        }

        #endregion

        #region Lifecycle

        private void OnEnable()
        {
            InitialSetupIfNeeded();

            cancelAction.Enable();
            toggleActivePlacementAction.Enable();
            finishPlacementAction.Enable();
            rotateClockwiseAction.Enable();
            rotateCounterClockwiseAction.Enable();
            cursorPositionAction?.Enable();
            nextPrefabAction.Enable();
            previousPrefabAction.Enable();

            foreach (var action in prefabActions)
                action.Enable();

            switch (cursorPositionType)
            {
                case CursorPositionType.Mouse:
                    UnityEngine.InputSystem.EnhancedTouch.TouchSimulation.Disable();
                    break;

                case CursorPositionType.TouchScreen:
                    UnityEngine.InputSystem.EnhancedTouch.TouchSimulation.Enable();
                    break;
            }
        }

        private void OnDisable()
        {
            cancelAction.Disable();
            toggleActivePlacementAction.Disable();
            finishPlacementAction.Disable();
            rotateClockwiseAction.Disable();
            rotateCounterClockwiseAction.Disable();
            cursorPositionAction?.Disable();
            nextPrefabAction.Disable();
            previousPrefabAction.Disable();

            foreach (var action in prefabActions)
                action.Disable();
        }

        private void Start()
        {
            // Bridge for input & placement.
            gameObject.AddComponent<InputControlResponder>();

            // Setup cursor provider, new input system allows flexible setup
            // with cursor or touch screen.
            if (transform.parent.TryGetComponent(out ObjectPlacement placement))
            {
                placement.CursorPositionProvider = this;
            }
            else if (ObjectPlacement.Instance != null)
            {
                ObjectPlacement.Instance.CursorPositionProvider = this;
            }
            else
            {
                Debug.LogWarning("Object Placement not present in the parent game object nor was it instantiated before the input.");
            }

            cancelAction.performed += HandleCancelAction;
            toggleActivePlacementAction.performed += HandleToggleActivePlacementAction;
            finishPlacementAction.performed += HandleFinishPlacementAction;
            rotateClockwiseAction.performed += HandleClockwiseRotation;
            rotateCounterClockwiseAction.performed += HandleCounterClockwiseRotation;

            nextPrefabAction.performed += HandleNextPrefabAction;
            previousPrefabAction.performed += HandlePreviousPrefabAction;

            foreach (var action in prefabActions)
            {
                action.performed += HandlePrefabAction;
            }
        }

        private void Update()
        {
            if (rotateClockwiseAction.type != InputActionType.Button)
            {
                if (IsPressed(rotateClockwiseAction))
                {
                    OnRotate.Invoke(Time.deltaTime);
                }
            }

            if (rotateCounterClockwiseAction.type != InputActionType.Button)
            {
                if (IsPressed(rotateCounterClockwiseAction))
                {
                    OnRotate.Invoke(-Time.deltaTime);
                }
            }
        }

        private void OnValidate()
        {
            // Re-create action if user switched the type.
            if (cursorPositionType != _cursorPositionType)
            {
                SetupCursorAction();
            }

            InitialSetupIfNeeded();
        }

        private void OnDestroy()
        {
            if (transform.parent.TryGetComponent(out ObjectPlacement placement))
            {
                placement.CursorPositionProvider = null;
            }

            cancelAction.performed -= HandleCancelAction;
            toggleActivePlacementAction.performed -= HandleToggleActivePlacementAction;
            finishPlacementAction.performed -= HandleFinishPlacementAction;
            rotateClockwiseAction.performed -= HandleClockwiseRotation;
            rotateCounterClockwiseAction.performed -= HandleCounterClockwiseRotation;

            nextPrefabAction.performed -= HandleNextPrefabAction;
            previousPrefabAction.performed -= HandlePreviousPrefabAction;

            foreach (var action in prefabActions)
            {
                action.performed -= HandlePrefabAction;
            }
        }

        #endregion

        #region Action Observers

        private void HandleCancelAction(InputAction.CallbackContext context)
        {
            if (IsPressed(context))
                OnPlacementCancel.Invoke();
        }

        private void HandleToggleActivePlacementAction(InputAction.CallbackContext context)
        {
            if (IsPressed(context))
                OnPlacementActiveToggle.Invoke();
        }

        private void HandleFinishPlacementAction(InputAction.CallbackContext context)
        {
            if (IsPressed(context))
                OnPlacementFinish.Invoke();
        }

        private void HandleClockwiseRotation(InputAction.CallbackContext context)
        {
            if (context.action.type == InputActionType.Button && IsPressed(context))
                OnRotate.Invoke(1);
        }

        private void HandleCounterClockwiseRotation(InputAction.CallbackContext context)
        {
            if (context.action.type == InputActionType.Button && IsPressed(context))
                OnRotate.Invoke(-1);
        }

        private void HandleNextPrefabAction(InputAction.CallbackContext context)
        {
            if (IsPressed(context))
                OnNextPrefab.Invoke();
        }

        private void HandlePreviousPrefabAction(InputAction.CallbackContext context)
        {
            if (IsPressed(context))
                OnPreviousPrefab.Invoke();
        }

        private void HandlePrefabAction(InputAction.CallbackContext context)
        {
            if (!IsPressed(context)) return;

            for (int index = 0; index < prefabActions.Length; index++)
            {
                if (prefabActions[index] == context.action)
                {
                    OnPrefabAction.Invoke(index);
                    break;
                }
            }
        }

        #endregion

        #region Convenience

        private void InitialSetupIfNeeded()
        {
            if (_initialSetup)
            {
                _initialSetup = false;

                cancelAction = new InputAction("Cancel", InputActionType.Button, "<Keyboard>/escape");
                toggleActivePlacementAction = new InputAction("TogglePlacement", InputActionType.Button, "<Keyboard>/b");
                finishPlacementAction = new InputAction("FinishPlacement", InputActionType.Button, "<Mouse>/leftButton");
                rotateClockwiseAction = new InputAction("RotateClockwise", InputActionType.Button, "<Keyboard>/e");
                rotateCounterClockwiseAction = new InputAction("RotateCounterClockwise", InputActionType.Button, "<Keyboard>/q");

                nextPrefabAction = new InputAction("Next Prefab", InputActionType.Button, "<Keyboard>/,");
                previousPrefabAction = new InputAction("Previous Prefab", InputActionType.Button, "<Keyboard>/.");
                prefabActions = new InputAction[10] {
                    new InputAction("Prefab Action", InputActionType.Button, "<Keyboard>/1"),
                    new InputAction("Prefab Action", InputActionType.Button, "<Keyboard>/2"),
                    new InputAction("Prefab Action", InputActionType.Button, "<Keyboard>/3"),
                    new InputAction("Prefab Action", InputActionType.Button, "<Keyboard>/4"),
                    new InputAction("Prefab Action", InputActionType.Button, "<Keyboard>/5"),
                    new InputAction("Prefab Action", InputActionType.Button, "<Keyboard>/6"),
                    new InputAction("Prefab Action", InputActionType.Button, "<Keyboard>/7"),
                    new InputAction("Prefab Action", InputActionType.Button, "<Keyboard>/8"),
                    new InputAction("Prefab Action", InputActionType.Button, "<Keyboard>/9"),
                    new InputAction("Prefab Action", InputActionType.Button, "<Keyboard>/0")
                };

                SetupCursorAction();
            }
        }

        private void SetupCursorAction()
        {
            _cursorPositionType = cursorPositionType;
            switch (cursorPositionType)
            {
                case CursorPositionType.Mouse:
                    UnityEngine.InputSystem.EnhancedTouch.TouchSimulation.Disable();
                    cursorPositionAction = new InputAction("MousePosition", InputActionType.PassThrough, "<Mouse>/position");
                    break;

                case CursorPositionType.TouchScreen:
                    UnityEngine.InputSystem.EnhancedTouch.TouchSimulation.Enable();
                    cursorPositionAction = new InputAction("TouchPosition", InputActionType.PassThrough, "<Touchscreen>/touch0/position");
                    break;
            }

            cursorPositionAction.Enable();
        }

        private bool IsPressed(InputAction.CallbackContext context)
            => IsPressed(context.action);

        private bool IsPressed(InputAction action)
            => action.ReadValue<float>() > 0f;

        #endregion

    }

}
#endif