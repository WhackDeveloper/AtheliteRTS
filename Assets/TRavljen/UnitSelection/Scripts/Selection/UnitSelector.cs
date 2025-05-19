using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

namespace TRavljen.UnitSelection
{

    /// <summary>
    /// Enum for setting up <see cref="UnitSelector"/> input in Editor.
    /// </summary>
    public enum InputType
    {
        None, LegacyInput, NewInputSystem
    }

    /// <summary>
    /// States used by the <see cref="UnitSelector"/>.
    /// </summary>
    internal enum SelectionStateId { Idle, Hover, Click, Drag }

    /// <summary>
    /// Class for handling unit selection primarily with the mouse.
    /// The selection is then modified on the <see cref="ActiveSelections"/>.
    /// </summary>
    [RequireComponent(typeof(ActiveSelections))]
    public class UnitSelector : MonoBehaviour
    {
        
#if UNITY_EDITOR
        [UnityEditor.MenuItem("GameObject/TRavljen/Unit Selection")]
        public static void CreateUnitSelectorInScene()
        {
            var existingSelector = Object.FindFirstObjectByType<UnitSelector>();

            if (existingSelector != null)
            {
                UnityEditor.Selection.activeObject = existingSelector;
                Debug.LogError("There is already a Unit Selection object in your scene.");
                return;
            }

            var obj = new GameObject("Unit Selection");
            obj.AddComponent<UnitSelector>();
            UnityEditor.Selection.activeObject = obj;
            UnityEditor.Undo.RegisterCreatedObjectUndo(obj.gameObject, "Create Unit Selection");
            Debug.Log("Unit Selection created. Now configure the selection area & input.");
        }
#endif

        #region Properties

        [SerializeField]
        [Tooltip("Specifies behaviour configuration of the Unit Selector.")]
        private SelectorConfiguration configuration = new()
        {
            SelectableLayerMask = ~0,
            DragSelectionEnabled = true,
            DragHighlightEnabled = true,
            ModifyingCurrentSelectionEnabled = true,
            DoubleClickSelectionEnabled = true,
            IgnoreWhenOverUI = true,
            SortSelectionByDistance = true,
            MaxSelectionDistance = 300,
            MouseDragThreshold = 10,
            HighlightOnMouseHover = true,
            MaxRaycastHits = 10,
            RaycastType = SelectionRaycastType.Nearest
        };

        /// <summary>
        /// Set this to 'false' if you wish to disable selection without disabling
        /// the component. This will prevent any further updates on the selection.
        /// Keep in mind the state will remain as is, so if there are selected
        /// units, disabling this won't deselect them.d
        /// </summary>
        public bool IsSelectionEnabled { private set; get; } = true;

        [SerializeField]
        [Tooltip("Abstract reference that may be set in the Editor. " +
            "This is optional and only accessible in the Inspector, in code the public interface uses IInputControl type.")]
        private AInputControl inputControl;

        // Used by EDITOR component only.
        [HideInInspector, SerializeField]
        private InputType inputType = InputType.None;

        /// <summary>
        /// Specifies the camera that will be used for raycasting. If this is
        /// not set manually then <see cref="Camera.main"/> will be set on
        /// <see cref="Start"/>.
        /// </summary>
        [SerializeField]
        private Camera _camera;

        /// <summary>
        /// Specifies the selection area that is required for gathering information
        /// about units within the selection area when the selection (mouse drag)
        /// is active.
        /// </summary>
        public ASelectionArea SelectionArea = null;

        /// <summary>
        /// Instance of currently active unit selection. If second instance
        /// attempts to be instantiated, it will be destroyed on <see cref="Awake"/>.
        /// </summary>
        public static UnitSelector Instance
        {
            get
            {
                if (_instance == null)
                    _instance = Object.FindFirstObjectByType<UnitSelector>();
                return _instance;
            }
            set => _instance = value;
        }
        
        private static UnitSelector _instance;

        /// <summary>
        /// Get or set currently used input control reference.
        /// </summary>
        public IInputControl InputControl
        {
            get => _inputControl;
            set => SetInputControl(_inputControl);
        }

        /// <summary>
        /// Specifies the component that manages players units. In case all
        /// units can be selected, this can be left on 'null'.
        /// </summary>
        public IUnitManager UnitManager
        {
            set
            {
                unitManager = value;

                if (SelectionArea)
                {
                    SelectionArea.UnitManager = value;
                }
            }
            get => unitManager;
        }

        /// <summary>
        /// Get a list of currently selected units
        /// </summary>
        public List<ISelectable> SelectedUnits => activeSelections.SelectedUnits;

        /// <summary>
        /// Get a list of currently highlighted units
        /// </summary>
        public List<ISelectable> HighlightedUnits => activeSelections.HighlightedUnits;

        /// <summary>
        /// Get attached active selections component.
        /// </summary>
        public ActiveSelections ActiveSelections {
            get {
                if (!activeSelectionsSet)
                {
                    activeSelections = GetComponent<ActiveSelections>();
                    activeSelectionsSet = true;
                }
                return activeSelections;
            }
        }

        /// <summary>
        /// Current selection system camera. To change this use Inspector or
        /// <see cref="SetCamera(Camera)"/> method.
        /// </summary>
        public Camera Camera => _camera;

        /// <summary>
        /// Specifies behaviour configuration of the Unit Selector
        /// </summary>
        public SelectorConfiguration Configuration => configuration;

        /// <summary>
        /// Current state of the mouse selection. Helps keep track of
        /// starting and ending positions of the mouse movement for object
        /// selection.
        /// </summary>
        internal MouseClickState MouseState = new MouseClickState(Vector3.zero);

        /// <summary>
        /// Current input reference in use.
        /// </summary>
        private IInputControl _inputControl;

        private ActiveSelections activeSelections;

        private IUnitManager unitManager = null;

        private bool activeSelectionsSet = false;

        private SelectorStateMachine stateMachine = new SelectorStateMachine();

        #endregion

        #region Lifecycle

        private void Awake()
        {
            if (_instance != null && _instance != this)
            {
                Destroy(this);
                return;
            }

            _instance = this;
            
            stateMachine.RegisterState(new IdleState());
            stateMachine.RegisterState(new HoverState());
            stateMachine.RegisterState(new ClickState());
            stateMachine.RegisterState(new DragState());

            if (!activeSelectionsSet)
            {
                activeSelections = GetComponent<ActiveSelections>();
                activeSelectionsSet = true;
            }
        }

        private void Start()
        {
            if (_inputControl == null)
            {
                // Attempt to get input control from the game object.
                SetInputControl(inputControl ?? GetComponentInChildren<IInputControl>());
            }

            ChangeState(SelectionStateId.Idle);

            if (inputType == InputType.None)
                Debug.Log("Unit Selector initialized with no InputType");
            
            if (configuration.IgnoreWhenOverUI && EventSystem.current == null)
            {
                configuration.IgnoreWhenOverUI = false;
                Debug.LogError("IgnoreWhenOverUI has been disabled. For this to work you must add Unity's EventSystem component to the game world.");
            }

            if (_camera == null)
            {
                _camera = Camera.main;
            }

            if (SelectionArea != null)
            {
                SelectionArea.UnitManager = unitManager;
                SelectionArea.SelectableLayerMask = configuration.SelectableLayerMask;
                SelectionArea.MaxSelectionDistance = Mathf.Max(0, configuration.MaxSelectionDistance);
                SelectionArea.Camera = Camera;
            }
            else
            {
                Debug.LogError("Selection Area has not be set on Unit Selector component.");
            }

            if (_inputControl == null)
            {
                gameObject.SetActive(false);
                Debug.LogWarning("Disabled selector game object. No Input detected.");
            }
        }

        private void Update()
        {
            stateMachine.Update(this);
        }

        private void OnDestroy()
        {
            if (_instance == this)
                _instance = null;
        }

        #endregion

        #region Public Interface

        /// <summary>
        /// Set a new input control reference.
        /// </summary>
        /// <param name="input">New input control</param>
        public void SetInputControl(IInputControl input)
        {
            if (_inputControl != null) RemoveListeners(_inputControl);

            _inputControl = input;

            // Update quick selector if present.
            if (TryGetComponent(out QuickAccessUnitSelector selector))
            {
                selector.SetInputControl(input);
            }

            if (_inputControl != null) AddListeners(_inputControl);
        }

        /// <summary>
        /// Update the camera selection system will use for raycasting and
        /// world space to screen space conversion.
        /// </summary>
        /// <param name="newCamera">New camera</param>
        public void SetCamera(Camera newCamera)
        {
            _camera = newCamera;
            
            if (SelectionArea != null)
            {
                SelectionArea.Camera = newCamera;
            }
        }

        /// <summary>
        /// Interrupts currently active dragging selection and deselects all
        /// active selections.
        /// </summary>
        public void CancelSelection()
        {
            HandleCancelSelection();
            ActiveSelections.DeselectAll();
        }

        /// <summary>
        /// Enables or disables selection feature. If dragging selection is active,
        /// it will also be interrupted to prevent player from finishing selection
        /// gesture.
        /// </summary>
        /// <param name="enabled"></param>
        public void SetSelectionEnabled(bool enabled)
        {
            if (IsSelectionEnabled == enabled) return;
            
            IsSelectionEnabled = enabled;
            
            if (!enabled && MouseState.IsActive)
            {
                // Cancel current selection only in active state.
                // This does not deselect units, only highlights.
                HandleCancelSelection();
            }
        }

        #endregion

        #region Player Interactions

        private void AddListeners(IInputControl input)
        {
            input.OnCancelTriggered += HandleCancelSelection;
            input.OnMouseDown += HandleMouseDown;
            input.OnMouseUp += HandleMouseUp;
        }

        private void RemoveListeners(IInputControl input)
        {
            input.OnCancelTriggered -= HandleCancelSelection;
            input.OnMouseDown -= HandleMouseDown;
            input.OnMouseUp -= HandleMouseUp;
        }

        private void HandleMouseDown()
        {
            if (!IsSelectionEnabled || ShouldIgnoreSelection()) return;

            // Change state to drag only if select or highlight is enabled.
            if (configuration.DragSelectionEnabled || configuration.DragHighlightEnabled)
            {
                ChangeState(SelectionStateId.Drag);
            }
        }

        private void HandleMouseUp()
        {
            if (!IsSelectionEnabled || !MouseState.IsActive) return;

            if (stateMachine.CurrentState == SelectionStateId.Drag)
            {
                (stateMachine.GetState(SelectionStateId.Drag) as DragState).FinishDrag(this);
            }
            else
            {
                // Trigger click since drag was never initiated
                ChangeState(SelectionStateId.Click);
            }
        }

        /// <summary>
        /// In case selection press is active, this will reset the state and unhighlight
        /// any objects that may have been highlighted during selection process.
        /// In case selection press is not active, this will deselect currently
        /// selected units with <see cref="ActiveSelections.DeselectAll"/>.
        /// </summary>
        private void HandleCancelSelection()
        {
            if (MouseState.IsActive)
            {
                activeSelections.UnhighlightAll();
                ChangeState(SelectionStateId.Idle);
            }
            else
            {
                // If drag was not active, deselect current selection
                activeSelections.DeselectAll();
            }
        }

        #endregion

        #region Internal

        /// <summary>
        /// If <see cref="ignoreWhenOverUI"/> is 'true', then just check if mouse pointer
        /// is over any UI object.
        /// </summary>
        /// <returns>Returns 'true' if selection should be ignored</returns>
        internal bool ShouldIgnoreSelection()
        {
            return configuration.IgnoreWhenOverUI && EventSystem.current.IsPointerOverGameObject();
        }

        /// <summary>
        /// Set new state for the selector.
        /// </summary>
        /// <param name="id">New state id</param>
        internal void ChangeState(SelectionStateId id) => stateMachine.ChangeState(this, id);

        /// <summary>
        /// Validate <see cref="SelectionArea"/> reference, make sure its not missing.
        /// If the reference is 'null' an exception will be thrown.
        /// </summary>
        /// <exception cref="System.ArgumentNullException">Missing SelectionArea reference</exception>
        internal void ValidateSelectionArea()
        {
            // If dragging is enabled, selection area is required
            if (SelectionArea == null)
            {
                // Log warning only on mouse down to inform the developer
                throw new System.ArgumentNullException("SelectionArea reference is missing! This is required for " +
                    "selection to work as it should do the calculation for selection");
            }
        }

        /// <summary>
        /// Update selection indicator visuals.
        /// </summary>
        internal void UpdateSelectionArea()
        {
            SelectionArea?.MouseDragContinues(MouseState.EndPos);
        }

        /// <summary>
        /// Returns true if modifying current selection is enabled and action pressed.
        /// </summary>
        internal bool IsModifyCurrentSelectionPressed()
            => configuration.ModifyingCurrentSelectionEnabled && _inputControl.IsModifyCurrentSelectionPressed;

        #endregion

    }

    #region State

    /// <summary>
    /// State data model for mouse selection.
    /// </summary>
    internal struct MouseClickState
    {
        public Vector2 StartPos;
        public Vector2 EndPos;

        /// <summary>
        /// Value is true if selection/press is active.
        /// </summary>
        public bool IsActive;

        public float Distance => Vector2.Distance(StartPos, EndPos);

        public MouseClickState(Vector2 initialPos, bool isActive = false)
        {
            StartPos = initialPos;
            EndPos = initialPos;

            IsActive = isActive;
        }
    }

    #endregion

}