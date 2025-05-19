using System.Collections;
using System.Collections.Generic;
using TRavljen.UnitFormation.Formations;
using UnityEngine;

namespace TRavljen.UnitFormation.Placement
{

    /// <summary>
    /// Component for placing units in formation with a mouse drag.
    /// To disable this when placement is not desired, disable the component like you would any other.
    /// <code>(placement.enabled = false)</code>
    /// </summary>
    public class FormationPlacement : MonoBehaviour
    {

        enum DistanceThresholdCheck
        {
            /// <summary>
            /// Does not use distance check for placement.
            /// </summary>
            Never,
            /// <summary>
            /// Checks distance only on finish of the placement.
            /// </summary>
            OnFinish,
            /// <summary>
            /// Checks distance with threshold distance on start, update and finish.
            /// </summary>
            Always
        }

        #region Properties

        [Tooltip("Specifies the layer mask used for mouse raycasts in order " +
            "to find the drag positions in world/scene.")]
        [SerializeField] private LayerMask groundLayerMask;

        [Tooltip("Specifies maximal range of mouse raycasts.")]
        [SerializeField, Range(1, 5_000)] private float raycastMaxDistance = 100;

        [Tooltip("Specifies custom input by referencing its GameObject. " +
            "If any component implements IInputControl it will be retrieved from it, " +
            "otherwise default input control will be added based on your Input system (new or old).")]
        [SerializeField] private GameObject customInput;

        [Tooltip("Specifies the unit formation component for placement. " +
            "This can be set at runtime, but not during active placement." +
            "You can use one component and modify its list of units. Or use " +
            "multiple, one for each unit group and then set reference with Set method.")]
        [SerializeField] private UnitFormation unitFormation;

        [Tooltip("Specifies if formation positions are also calculated during active placement (used for visuals).")]
        [SerializeField] private bool alwaysCalculatePositions = true;

        [Tooltip("Specifies interval for calculating positions during active placement. " +
            "When threshold is 0, it will calculate positions each frame.")]
        [Range(0f, 1.5f)]
        [SerializeField] private float calculatePositionsIntervalThreshold = 0.01f;

        [Tooltip("Specifies if the visuals should be placed on valid ground and " +
            "not their absolute formation position (uneven terrain, water, " +
            "structures, environment can be in the way).")]
        [SerializeField] private bool placeVisualsOnGround = false;

        [Tooltip("Specifies the placement visuals used for rendering when the " +
            "unit placement is active.")]
        [SerializeField] private APlacementVisuals[] placementVisuals;

        [Tooltip("Specifies the use of distanceThreshold checks.")]
        [SerializeField]
        private DistanceThresholdCheck distanceCheck;

        [Tooltip("Specifies required distance for valid placement. How it is use " +
            "is defined by the `distanceCheck` type. This allows cancelation of " +
            "active placement by returning to original placement position within " +
            "the distance threshold.")]
        [SerializeField]
        private float distanceThreshold = 2;

        [Tooltip("Specifies the minimal placement duration. Set this value if" +
            "placement should be active for certain amount of time, before it " +
            "becomes a valid placement.")]
        [SerializeField]
        private float minPlacementDuration = 0;

        [Tooltip("Specifies the delay of the placement start after its initial " +
            "activation, this will include delaying visuals of the placement.")]
        [SerializeField, Range(1, 5)]
        private float startDelay = 0;

        internal IInputControl input;

        private Vector3 startPosition;
        private Vector3 endPosition;

        private bool hasPlacementStarted = false;
        private bool isPlacementActive = false;
        private bool areIndicatorsHidden = false;

        private float placementDuration = 0;
        private float placementDelay = 0;
        private float calculatePositionsInterval;

        /// <summary>
        /// Specifies the unit formation component for placement.
        /// This can be changed by calling <see cref="SetUnitFormation(UnitFormation)"/>
        /// method, but during active placement it won't have any effect.
        /// </summary>
        public UnitFormation UnitFormation => unitFormation;

        /// <summary>
        /// If placement was activated. This is set to true when
        /// <see cref="StartPlacement"/> is called.
        /// <see cref="HasPlacementStarted"/> is set when placement
        /// is valid which is set up with configuration.
        /// </summary>
        public bool IsPlacementActive => isPlacementActive;

        /// <summary>
        /// If placement has started.
        /// </summary>
        public bool HasPlacementStarted => hasPlacementStarted;

        /// <summary>
        /// Event invoked when placement actually starts. This might not be
        /// when <see cref="StartPlacement"/> is called if configuration
        /// prevents it, like <see cref="distanceCheck"/> or <see cref="startDelay"/>.
        /// </summary>
        public UnityEngine.Events.UnityEvent OnPlacementStarted = new();

        /// <summary>
        /// Event invoked when placement has finished either successfully or
        /// canceled.
        /// </summary>
        public UnityEngine.Events.UnityEvent OnPlacementFinish = new();

        /// <summary>
        /// Checks if the distance between start and end positions is over the
        /// distance threshold.
        /// </summary>
        private bool IsDistanceAboveThreshold
            => Vector3.Distance(startPosition, endPosition) > distanceThreshold;

        #endregion

        #region Lifecycle

        private void Start()
        {
            SetupInput();
        }

        private void OnEnable()
        {
            RemoveInputListeners();
            AddInputListeners();
        }

        private void OnDisable() => RemoveInputListeners();

        private void Update()
        {
            if (isPlacementActive && PerformRaycast(out Vector3 position))
            {
                UpdatePlacement(position);
            }
        }

        #endregion

        #region Input

        private void SetupInput()
        {
            // Ignore if already set up
            if (this.input != null) return;

            if (customInput != null && customInput.TryGetComponent(out IInputControl input))
            {
                SetInput(input);
            }
            else if (TryGetComponent(out input))
            {
                SetInput(input);
            }
            else
            {
                AddDefaultInput();
            }
        }

        public void AddDefaultInput()
        {
            IInputControl newInput;
#if ENABLE_INPUT_SYSTEM
            // New input system backends are enabled.
            ActionInputControl control = gameObject.AddComponent<ActionInputControl>();
            control.SetupDefaultActionsIfNull();
            newInput = control;
#elif ENABLE_LEGACY_INPUT_MANAGER
            // Old input backends are enabled.
            newInput = gameObject.AddComponent<KeyInputControl>();
#endif
            SetInput(newInput);
        }

        private void AddInputListeners()
        {
            if (input != null)
            {
                input.OnPlacementActionPress.AddListener(StartPlacement);
                input.OnPlacementActionRelease.AddListener(FinishPlacement);
                input.OnPlacementActionCancel.AddListener(CancelPlacement);
            }
        }

        private void RemoveInputListeners()
        {
            if (input != null)
            {
                input.OnPlacementActionPress.RemoveListener(StartPlacement);
                input.OnPlacementActionRelease.RemoveListener(FinishPlacement);
                input.OnPlacementActionCancel.RemoveListener(CancelPlacement);
            }
        }

        #endregion

        #region Placement

        /// <summary>
        /// Initiates the unit placement process and, if all conditions are met, activates visuals.
        /// </summary>
        /// <remarks>
        /// Starts the placement process if:
        /// - <see cref="unitFormation"/> is defined and has units available.
        /// - A raycast successfully hits a valid ground position.
        /// 
        /// If <paramref name="startDelay"/> is set, the placement visuals will not appear
        /// immediately but will activate only after the delay period has passed. Additionally, 
        /// placement activation depends on the <see cref="distanceCheck"/> criteria:
        /// - If <see cref="distanceCheck"/> is **not** set to <see cref="DistanceThresholdCheck.Always"/>, 
        ///   placement will proceed regardless of distance.
        /// - If <see cref="distanceCheck"/> is set to <see cref="DistanceThresholdCheck.Always"/>, 
        ///   the distance must exceed the threshold defined by <see cref="distanceThreshold"/> 
        ///   for placement to start.
        /// </remarks>
        public void StartPlacement()
        {
            if (unitFormation != null &&
                unitFormation.HasUnits &&
                PerformRaycast(out Vector3 position))
            {
                isPlacementActive = true;
                startPosition = position;
                endPosition = position;

                placementDelay = startDelay;

                bool isDistanceValid = distanceCheck != DistanceThresholdCheck.Always || IsDistanceAboveThreshold;
                if (placementDelay <= 0 && isDistanceValid)
                {
                    StartPlacementVisuals();
                }
            }
        }

        private void StartPlacementVisuals()
        {
            OnPlacementStarted.Invoke();
            hasPlacementStarted = true;
            placementDuration = 0;

            foreach (var visuals in placementVisuals)
                visuals.StartPlacement(startPosition);
        }

        /// <summary>
        /// Completes the process of placement, if placement is active.
        /// </summary>
        public void FinishPlacement()
        {
            if (!isPlacementActive) return;

            // If not within distance it only cancels
            CancelPlacement();

            // Checks distance when its set to Always and OnFinish
            if ((distanceCheck == DistanceThresholdCheck.Never || IsDistanceAboveThreshold)
                && placementDuration >= minPlacementDuration)
            {
                ApplyCurrentUnitFormation();
            }
        }

        /// <summary>
        /// Cancels current placement if one is active.
        /// </summary>
        public void CancelPlacement()
        {
            if (!isPlacementActive) return;

            isPlacementActive = false;
            hasPlacementStarted = false;

            foreach (var visuals in placementVisuals)
                visuals.StopPlacement();

            OnPlacementFinish.Invoke();
        }

        private void UpdatePlacement(Vector3 position)
        {
            // Countdown the delay if it didn't start yet.
            if (!hasPlacementStarted)
            {
                // Perform delay until its done
                placementDelay -= Time.deltaTime;

                if (placementDelay <= 0)
                {
                    StartPlacementVisuals();
                }
                else
                {
                    return;
                }
            }

            placementDuration += Time.deltaTime;

            endPosition = position;

            // Check if distance is within threshold to pause placement.
            if (distanceCheck == DistanceThresholdCheck.Always && !IsDistanceAboveThreshold)
            {
                // Hide placement if visible
                if (!areIndicatorsHidden)
                {
                    foreach (var visuals in placementVisuals)
                        visuals.StopPlacement();
                    areIndicatorsHidden = true;
                }
                return;
            }

            // Show indicators if hidden
            if (areIndicatorsHidden)
            {
                areIndicatorsHidden = false;

                foreach (var visuals in placementVisuals)
                    visuals.StartPlacement(startPosition);
            }

            // Update with new end position
            foreach (var visuals in placementVisuals)
                visuals.ContinuePlacement(position);

            // Claculate points if enabled and within the interval
            if (alwaysCalculatePositions)
            {
                calculatePositionsInterval += Time.deltaTime;

                if (calculatePositionsInterval > calculatePositionsIntervalThreshold)
                {
                    calculatePositionsInterval = 0;

                    UpdateFormationVisuals();
                }
            }
        }

        private bool PerformRaycast(out Vector3 hitPoint)
        {
            Ray ray = Camera.main.ScreenPointToRay(input.MousePosition);
            if (Physics.Raycast(ray, out RaycastHit hit, raycastMaxDistance, groundLayerMask))
            {
                hitPoint = hit.point;
                return true;
            }

            hitPoint = Vector3.zero;
            return false;
        }

        private void UpdateFormationVisuals()
        {
            Vector3 direction = endPosition - startPosition;
            var formation = unitFormation.CalculatePositions(startPosition, direction);

            // Find ground for visuals
            if (placeVisualsOnGround)
            {
                for (int index = 0; index < formation.UnitPositions.Count; index++)
                {
                    formation.UnitPositions[index] = unitFormation.MovePositionOnGround(formation.UnitPositions[index]);
                }
            }

            foreach (var visuals in placementVisuals)
                visuals.OnFormationReady(formation);
        }

        #endregion

        #region Public Interface

        /// <summary>
        /// Update unit formation used for placement.
        /// </summary>
        /// <param name="unitFormation">New unit formation</param>
        public void SetUnitFormation(UnitFormation unitFormation)
        {
            if (hasPlacementStarted)
            {
                Debug.LogWarning("UnitFormation cannot be changed during active placement!");
                return;
            }

            this.unitFormation = unitFormation;
        }

        /// <summary>
        /// Set new input reference.
        /// </summary>
        /// <param name="input">Input reference</param>
        public void SetInput(IInputControl input)
        {
            // Remove if there are any
            RemoveInputListeners();

            this.input = input;

            if (enabled)
                AddInputListeners();
        }

        /// <summary>
        /// Set current formation used calculating units positions.
        /// </summary>
        /// <param name="formation">New formation</param>
        public void SetFormation(IFormation formation)
        {
            unitFormation.SetUnitFormation(formation);
            ApplyCurrentUnitFormation();
        }

        /// <summary>
        /// Apply formation positions based on active placement or last active placement.
        /// </summary>
        public void ApplyCurrentUnitFormation()
        {
            Vector3 direction = endPosition - startPosition;
            unitFormation.ApplyCurrentUnitFormation(startPosition, direction);
        }

        /// <summary>
        /// Calculates and applies new formation positions the new position and
        /// with new direction.
        /// </summary>
        /// <param name="position">New formation position</param>
        /// <param name="direction">New formation direction</param>
        /// <param name="triggerVisuals">If indicators should be shown.
        /// This can be utilised with indicators that hide with delay.</param>
        public void PositionUnits(Vector3 position, Vector3 direction, bool triggerVisuals)
        {
            UnitFormation.ApplyCurrentUnitFormation(position, direction);

            if (triggerVisuals)
            {
                foreach (var visuals in placementVisuals)
                {
                    startPosition = position;
                    endPosition = position + direction.normalized * 3;

                    visuals.StartPlacement(position);
                }

                UpdateFormationVisuals();

                foreach (var visuals in placementVisuals)
                {
                    visuals.ContinuePlacement(endPosition);

                    visuals.StopPlacement();
                }
            }
        }

        /// <summary>
        /// Calculates the middle position of all units in formation to calculate
        /// their new direction relative to the new position. Then applies the
        /// new formation positions.
        /// </summary>
        /// <param name="position">New formation position</param>
        /// <param name="triggerVisuals">If indicators should be shown.
        /// This can be utilised with indicators that hide with delay.</param>
        public void PositionUnits(Vector3 position, bool triggerIndicators)
        {
            Vector3 mid = Vector3.zero;
            foreach (var formationUnit in UnitFormation.Units)
                mid += formationUnit.position;

            mid /= UnitFormation.Units.Count;

            Vector3 direction = position - mid;

            PositionUnits(position, direction, triggerIndicators);
        }

        #endregion

    }
}