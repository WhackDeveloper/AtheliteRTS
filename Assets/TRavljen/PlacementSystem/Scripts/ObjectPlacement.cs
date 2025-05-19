using UnityEngine;

namespace TRavljen.PlacementSystem
{
    using Utility;
    using TRavljen.Utility;
    using UnityEngine.EventSystems;

    #region Types

    /// <summary>
    /// Defines the alignment type of placing objects.
    /// </summary>
    enum GroundAlignmentType {
        /// <summary>
        /// Object will not be aligned to ground, nor will they be validated.
        /// </summary>
        None,
        /// <summary>
        /// Object will not be aligned to ground. It will only be validated based on rotation threshold.
        /// </summary>
        ValidateOnly,
        /// <summary>
        /// Object will align to ground with its edges touching the mesh or terrain below it, rotating X and Z axis to achieve it.
        /// </summary>
        AlignToGround,
        /// <summary>
        /// Object will find which of its edges is the lowest on ground and position itself on it, without rotation.
        /// </summary>
        LowestPoint
    }

    #endregion

    public class ObjectPlacement : MonoBehaviour
    {
    
#if UNITY_EDITOR
        [UnityEditor.MenuItem("GameObject/TRavljen/Object Placement")]
        public static void CreateUnitSelectorInScene()
        {
            var existingPlacement = Object.FindFirstObjectByType<ObjectPlacement>();

            if (existingPlacement != null)
            {
                UnityEditor.Selection.activeObject = existingPlacement;
                Debug.LogWarning("There is already a Object Placement object in your scene.");
                return;
            }

            var obj = new GameObject("Object Placement");
            obj.AddComponent<ObjectPlacement>();
            UnityEditor.Selection.activeObject = obj;
            UnityEditor.Undo.RegisterCreatedObjectUndo(obj.gameObject, "Create Object Placement");
            Debug.Log("Object Placement created. Now adjust its options and configure the input.");
        }
#endif

        #region Serialized Properties

        /// <summary>
        /// Instance of currently active object placement. If second instance
        /// attempts to be instantiated, it will be destroyed on <see cref="Awake"/>.
        /// </summary>
        public static ObjectPlacement Instance
        {
            get
            {
                if (_instance == null)
                    _instance = Object.FindFirstObjectByType<ObjectPlacement>();
                return _instance;
            }
        }
        
        private static ObjectPlacement _instance;

        // Main
        [Tooltip("Specifies the main camera which renders world to the player.")]
        [SerializeField] private Camera playerCamera;

        [Tooltip("Specifies the prefab used for placement. This may also be set during runtime, when the placement is not active.")]
        [SerializeField] private GameObject placementObjectPrefab;

        // Detection
        [Tooltip("Specifies the ground layer mask for ground detection. " +
            "Use the layer on which the objects should be placed, commonly 'Ground' is used for terrains or ground planes.")]
        [SerializeField] private LayerMask groundLayer = ~0;

        [Tooltip("Specifies the maximal range allowed for ground detection from camera/cursor position.")]
        [SerializeField, Range(10, 5_000)] private float maxDetectionDistance = 150;

        [Tooltip("Specifies the obstacles layer mask used for detecting for any " +
            "objects that might be in the way, like environment or already placed objects.")]
        [SerializeField] private LayerMask obstacleLayer;

        [Tooltip("Specifies the layer of destructables used for detecting objects in placement bounds that may be destroyed when object is placed.")]
        [SerializeField] private LayerMask destructablesLayer;

        [Tooltip("Specifies if objects detected as destructable obstacles are destroyed on placement." +
                 "If this is set to false, you need to manage destruction with OnObjectDestroy event.")]
        [SerializeField] 
        private bool destroyDestructableOnPlacement = true;

        [Tooltip("Specifies if the placement should be ignored when cursor is hovering over UI components.\n\n" +
            "Example: If this is disabled, clicking a button on UI when placement is active will still trigger placement." +
            "When enabled, such placement action will be ignored.")]
        [SerializeField]
        private bool ignoreWhenOverUI = true;

        [SerializeField]
        [Tooltip(
            "Specifies the area of valid placement. This can restrict placement of objects outside of certain area.")]
        private PlacementAreaInfo placementArea = new() {
            boundsType = PlacementAreaType.Anywhere,
            boxBounds = new Bounds(Vector3.zero, new Vector3(25, 15, 25)),
            sphereBounds = new SphereBounds(Vector3.zero, 25)
        };

        [SerializeField] private bool clampInsidePlacementArea;

        [Tooltip("Specifies if the object should visually align to the ground instead of the placement center. See `GroundAlignmentType` for more information.")]
        [SerializeField] private GroundAlignmentType groundAlignmentType = GroundAlignmentType.ValidateOnly;
        [SerializeField] private GroundAlignment groundAlignment = new GroundAlignment();

        [Tooltip("Specifies the angle range in which the object placement is valid, " +
            "once angles go out of this bounds, placement is no longer valid. " +
            "This ensures that the ground below it is flat enough for placement of objects.")]
        [IntRange(0, 90)]
        [SerializeField] private IntRange groundAngleThreshold = new IntRange(0, 15);

        // Rotation
        [Tooltip("Specifies rotation speed. If holdToRotate is enabled, then this value will be multiplied with delta time on each update.")]
        [SerializeField] private float rotationSpeed = 90;

        [Tooltip("Specifies if the rotation between placements is preserved. " +
            "If a placement is canceled or finished, should the next placement keep the Y rotation or reset to 0.")]
        [SerializeField] private bool preserveRotation = false;

        // Grid/Snapping
        [Tooltip("Specifies if the position snapping feature is enabled.")]
        [SerializeField] private bool usePositionSnapping = false;

        [SerializeField]
        private GridSnapper gridSnapper = new GridSnapper(Vector3.zero, new Vector2(2, 2));

        // Materials
        [Tooltip("Specifies if valid/invalid materials are applied to the placing object.")]
        [SerializeField] private bool applyPlacementMaterials;

        [Tooltip("Specifies a list of materials applied to placing object when placement is valid.")]
        [SerializeField] private Material[] validPlacementMaterials;

        [Tooltip("Specifies a list of materials applied to placing object when placement is invalid.")]
        [SerializeField] private Material[] invalidPlacementMaterials;

        // Animations
        [Tooltip("Specifies if placing an object is allowed during active movement or rotation animations. " +
            "When this is disabled `animatingAngleThreshold` and `animatingDistanceThreshold` will be used.")]
        [SerializeField] private bool allowPlacementDuringAnimations = false;

        [Tooltip("Specifies what the max angle difference can be between target rotation " +
            "and current rotation, when placement during animation is not allowed.")]
        [SerializeField, Range(0, 360)] private float animatingAngleThreshold = 5;

        [Tooltip("Specifies the max distance allowed between target position and current placing object, " +
            "when placement during animation is not allowed")]
        [SerializeField, Range(0, 100)] private float animatingDistanceThreshold = 1;

        [Tooltip("Specifies if the position changes are animated. This means that if enabled, " +
            "placing object will follow cursor with position change speed.")]
        [SerializeField] private bool animatePositionChange;

        [Tooltip("Specifies the speed of position change if the change is animated.")]
        [SerializeField] private float positionChangeSpeed = 15;

        [Tooltip("Specifies if the rotation changes are animated. This means that if enabled, " +
            "placing object will rotate with the rotation change speed.")]
        [SerializeField] private bool animateRotationChange;

        [Tooltip("Specifies the speed of rotation change if the change is animated.")]
        [SerializeField] private float rotationChangeSpeed = 5;

        [Tooltip("Specifies if the console logging is enabled.")]
        public bool debugLogsEnabled = false;

        [Tooltip("Specifies if the gizmos for placement are enabled.")]
        public bool drawGizmosEnabled = false;

        [Tooltip("Specifies the gizmo color for placement area.")]
        [SerializeField]
        Color gizmoPlacementAreaColor = new Color(1, 1, 0, 0.3f);

        [Tooltip("Specifies the gizmo color for placing object's bounds.")]
        [SerializeField]
        Color gizmoPlacementBoundsColor = new Color(1, 0, 0, 1);

        [Tooltip("Specifies the gizmo color for positions on ground under the placing object. " +
            "These are indicators of where the ground position was found for each edge of the bounds of the placing object.")]
        [SerializeField]
        Color gizmoPlacementGroundPositionsColor = new Color(0, 1, 0);

        /// <summary>
        /// Validator invoked after internal validation has been performed with
        /// a valid result, if result is invalid this will not be invoked.
        /// Only already valid placements can be rejected by this validator.
        /// </summary>
        public IValidatePlacement AdditionalValidator;

        /// <summary>
        /// Cursor position provider reference. By default the <see cref="DefaultMousePositionProvider"/>
        /// is used. You can override this with any custom implementation of the <see cref="ICursorPositionProvider"/>
        /// interface.
        /// </summary>
        public ICursorPositionProvider CursorPositionProvider = new DefaultMousePositionProvider();

        #endregion

        #region Properties

        private bool isPlacementActive = false;
        private bool isPlacementValid = false;

        private Vector3 targetPosition;
        private float targetRotationY;
        private float currentRotationY;
        private Quaternion CurrentRotation => Quaternion.Euler(0, currentRotationY, 0);

        private Transform placingObjectTemplate;
        private IPlacementBounds placingObjectBounds;

        private readonly Collider[] colliderHits = new Collider[5];

        public ObjectPlacementEvents Events => ObjectPlacementEvents.Instance;

        /// <summary>
        /// Specifies the area of valid placement. This can restrict placement of objects outside of certain area.
        /// </summary>
        public PlacementAreaInfo PlacementArea
        {
            get => placementArea;
            set => placementArea = value;
        }

        /// <summary>
        /// Getter for checking if object placement is currently active.
        /// </summary>
        public bool IsPlacementActive => isPlacementActive;

        /// <summary>
        /// Current prefab used when placement activates.
        /// </summary>
        public GameObject PlacementObjectPrefab {
            get => placementObjectPrefab;
        }

        /// <summary>
        /// Sets and gets the current players camera used to render world objects.
        /// This camera is used for raycasts from screenpoint.
        /// </summary>
        public Camera PlayerCamera
        {
            get => playerCamera;
            set => playerCamera = value;
        }

        /// <summary>
        /// Returns placement prefab component if present. This must be enabled
        /// on <see cref="ObjectPlacement"/> component in Editor under "Prefabs"
        /// tab, otherwise null will be returned.
        /// </summary>
        public PlacementPrefabs PlacementPrefabs;

        #endregion

        #region Lifecycle

        private void OnValidate()
        {
            if (playerCamera == null)
            {
                Camera mainCamera = Camera.main;
                if (mainCamera != null && gameObject.scene == mainCamera.scene)
                    playerCamera = mainCamera;
            }

            if (PlacementPrefabs == null)
                PlacementPrefabs = GetComponentInChildren<PlacementPrefabs>();
            
            gridSnapper.Validate();

            positionChangeSpeed = Mathf.Max(positionChangeSpeed, 0.01f);
            rotationChangeSpeed = Mathf.Max(rotationChangeSpeed, 0.01f);
        }

        private void Awake()
        {
            if (_instance != null && _instance != this)
            {
                if (debugLogsEnabled)
                    Debug.LogWarning("ObjectPlacement component will be destroyed as there is already one present!");
                Destroy(this);
                return;
            }
            
            _instance = this;

            if (placementArea is { boundsType: PlacementAreaType.Custom, customArea: null })
            {
                placementArea.customArea = GetComponentInChildren<IPlacementArea>();
            }
        }

        private void Start()
        {
            if (playerCamera == null)
                playerCamera = Camera.main;

            // Validate if event system feature can be enabled.
            if (ignoreWhenOverUI && EventSystem.current == null)
            {
                ignoreWhenOverUI = false;
                Debug.LogError("IgnoreWhenOverUI feature has been disabled. For this to work you must add Unity's EventSystem component in the scene.");
            }
        }

        private void Update()
        {
            if (isPlacementActive)
            {
                UpdatePlacingObject(true);
            }
        }

        /// <summary>
        /// Activates the game object.
        /// </summary>
        public void Enable() => gameObject.SetActive(true);

        /// <summary>
        /// Deactivates the game object.
        /// </summary>
        public void Disable() => gameObject.SetActive(false);

        #endregion

        #region Begin & End

        /// <summary>
        /// Sets the new placement prefab. If placement is already active it will
        /// be cancelled and activated again with the new prefab.
        /// </summary>
        /// <param name="prefab">New placement prefab</param>
        public void SetPlacementPrefab(GameObject prefab)
        {
            if (isPlacementActive)
                BeginPlacement(prefab);
            else
                placementObjectPrefab = prefab;
        }

        /// <summary>
        /// Begins the placement with the new prefab game object.
        /// Placement materials remain unchanged.
        /// </summary>
        /// <param name="prefab">New placement prefab</param>
        public void BeginPlacement(GameObject prefab)
        {
            CancelPlacement();
            placementObjectPrefab = prefab;
            BeginPlacement();
        }

        /// <summary>
        /// Begins placement with passed prefab and optional materials.
        /// If visuals on placing objects are desired, you can use placement
        /// material arguments, <see cref="IPlacingObject"/> or <see cref="PlacingObjectObserver"/>
        /// to apply visuals for placement states out of the box.
        /// </summary>
        /// <param name="prefab">Prefab reference to be used for placement</param>
        /// <param name="validPlacement">
        /// Materials applied to new instance of the prefab when placement is valid.
        /// Default value is null, which does nothing.
        /// </param>
        /// <param name="invalidPlacement">
        /// Materials applied to new instance of the prefab when placement is invalid.
        /// Default value is null, which does nothing.
        /// </param>
        public void BeginPlacement(GameObject prefab, Material[] validPlacement, Material[] invalidPlacement)
        {
            CancelPlacement();

            placementObjectPrefab = prefab;

            if (validPlacement != null)
                validPlacementMaterials = validPlacement;

            if (invalidPlacement != null)
                invalidPlacementMaterials = invalidPlacement;

            validPlacementMaterials = validPlacement;
            invalidPlacementMaterials = invalidPlacement;

            BeginPlacement();
        }

        /// <summary>
        /// Begins placement with current <see cref="PlacementObjectPrefab"/>
        /// on current's mouse position.
        /// </summary>
        public void BeginPlacement()
        {
            CancelPlacement();

            if (playerCamera == null)
                throw new System.NullReferenceException("Player Camera is not set, but required for placement!");

            if (placementObjectPrefab == null)
                throw new System.NullReferenceException("Cannot instantiate new object from placing object which is a null reference!");

            if (preserveRotation)
                currentRotationY = targetRotationY;
            else
                targetRotationY = currentRotationY = 0;
            
            var placementPosition = GetPlacementPosition();

            GameObject newObject = InstantiatePlacementObject.TryInstantiate(
                placementObjectPrefab,
                placementPosition,
                Quaternion.identity,
                transform,
                out IPlacementBounds bounds);

            if (newObject == null)
            {
                CancelPlacement();
                throw new System.NullReferenceException("Cancelling object placement. The root game object should contain a Box Collider, Renderer, CustomPlacementBounds " +
                    "or any of the provided components for defining object bounds!");
            }

            placingObjectTemplate = newObject.transform;
            placingObjectTemplate.rotation = Quaternion.Euler(0, currentRotationY, 0);

            placingObjectBounds = bounds;

            isPlacementValid = false;
            isPlacementActive = true;

            Events.OnPlacementStart.Invoke(placingObjectTemplate.gameObject);
            GetPlacingObjectComponent(placingObjectTemplate)?.PlacementStarted();
        }

        /// <summary>
        /// Cancels currently active placement.
        /// </summary>
        public void CancelPlacement()
        {
            if (!isPlacementActive) return;

            if (placingObjectTemplate != null)
            {
                GetPlacingObjectComponent(placingObjectTemplate)?.PlacementCancelled();
                Destroy(placingObjectTemplate.gameObject);
            }

            placingObjectTemplate = null;
            isPlacementActive = false;
            Events.OnPlacementCancel.Invoke();
        }

        /// <summary>
        /// Attempts to end placement by instantiating a new object on the
        /// valid placement. If placement failed no object will be instantiated
        /// and placement will continue and not be cancelled.
        /// </summary>
        /// <param name="newInstance">Reference to the new instance created</param>
        /// <param name="removeDestructables">Specifies if destructables will be removed when placement ends. By default this is true</param>
        /// <returns>Returns true if new instance was created, returns false if placement was invalid.</returns>
        public bool TryEndPlacement(out GameObject newInstance, bool removeDestructables = true)
        {
            if (!isPlacementActive)
            {
                newInstance = default;
                return false;
            }

            if (!InternalTryEndPlacement(out PlacementResult result, removeDestructables))
            {
                newInstance = null;
                Events.OnPlacementFail.Invoke();
                GetPlacingObjectComponent(placingObjectTemplate)?.PlacementFailed();
                return false;
            }

            newInstance = Instantiate(placementObjectPrefab, result.Position, result.Rotation);
            // In case prefab reference is inactive (like scene/world reference)
            newInstance.SetActive(true);
            Events.OnPlacementEnd.Invoke(newInstance, result);
            GetPlacingObjectComponent(newInstance.transform)?.PlacementEnded();

            return true;
        }

        /// <summary>
        /// Checks if placement should be ignored duo to cursor being over the UI components,
        /// like buttons, texts or images.
        /// </summary>
        /// <returns>Returns true when placement should be ignored.</returns>
        private bool ShouldIgnorePlacement() => ignoreWhenOverUI && EventSystem.current.IsPointerOverGameObject();
        
        /// <summary>
        /// Attempts to end placement by returning a valid position and rotation
        /// for the object currently placing. When placement is inactive or invalid
        /// false is returned along with default result values.
        /// </summary>
        /// <param name="placementResult">Result values, set only in case of valid placement</param>
        /// <param name="removeDestructables">Specifies if destructables will be removed when placement ends. By default this is true</param>
        /// <returns>Returns true if placement was valid, returns false if it was inactive or invalid.</returns>
        public bool TryEndPlacement(out PlacementResult placementResult, bool removeDestructables = true)
        {
            if (!isPlacementActive)
            {
                placementResult = default;
                return false;
            }

            if (InternalTryEndPlacement(out placementResult, removeDestructables))
            {
                Events.OnPlacementEnd.Invoke(null, placementResult);
                return true;
            }
            else
            {
                Events.OnPlacementFail.Invoke();
                GetPlacingObjectComponent(placingObjectTemplate)?.PlacementFailed();
                return false;
            }
        }

        /// <summary>
        /// Method for internal use, does not invoke any events on <see cref="ObjectPlacementEvents"/>,
        /// but only performs common logic for both interfaces to end placement.
        /// Attempts to end currently active placement and destroys <see cref="placingObjectTemplate"/>
        /// if placement is successful.
        /// </summary>
        /// <param name="placementResult">Result of placement. Default values are returned if placement failed.</param>
        /// <returns>Returns true if placement was valid, returns false if it was inactive or invalid.</returns>
        private bool InternalTryEndPlacement(out PlacementResult placementResult, bool removeDestructables)
        {
            // Check if placement should be ignored duo to UI components
            if (ShouldIgnorePlacement())
            {
                placementResult = default;
                return false;
            }

            Vector3 orgPosition = placingObjectTemplate.position;

            // Check if animation is about done
            if (!allowPlacementDuringAnimations &&
                (MathHelper.AngleDistance(currentRotationY, targetRotationY) > animatingAngleThreshold ||
                Vector3.Distance(orgPosition, targetPosition) > animatingDistanceThreshold))
            {
                placementResult = default;
                return false;
            }

            Quaternion orgRotation = placingObjectTemplate.rotation;
            // This changes if animation is disabled, it will be reset if needed.
            float tempRotationY = currentRotationY;

            UpdatePlacingObject(false);

            if (!isPlacementValid)
            {
                // Reset position, rotation and reset the current rotation value
                placingObjectTemplate.SetPositionAndRotation(orgPosition, orgRotation);
                currentRotationY = tempRotationY;
                placementResult = default;
                return false;
            }

            placementResult = new PlacementResult()
            {
                Position = placingObjectTemplate.position,
                Rotation = placingObjectTemplate.rotation
            };

            if (removeDestructables &&
                placingObjectTemplate.TryGetComponent(out IPlacementBounds placingObject))
            {
                RemoveDestructables(placingObject.PlacementBounds, placingObject.PlacementRotation);
            }

            Destroy(placingObjectTemplate.gameObject);
            placingObjectTemplate = null;
            isPlacementActive = false;

            return true;
        }

        #endregion

        #region Rotation

        /// <summary>
        /// Resets current rotation for placing object. This can also be used
        /// if <see cref="preserveRotation"/> is enabled.
        /// </summary>
        public void ResetRotation()
        {
            targetRotationY = placementObjectPrefab.transform.eulerAngles.y;
            currentRotationY = targetRotationY;
        }

        /// <summary>
        /// Rotates currently placing object on Y axis by increasing current
        /// target Y angle.
        /// </summary>
        /// <param name="yAngle">Y angle to add to object's current Y.</param>
        public void RotateObjectYAxis(float yAngle)
        {
            targetRotationY += yAngle;
        }

        /// <summary>
        /// Rotates currently placing object on Y axis by
        /// multiplying directional magnitude parameter with
        /// <see cref="rotationSpeed"/>.
        /// </summary>
        /// <param name="directionalMagnitude">Y direction with magnitude.</param>
        public void RotatePlacement(float directionalMagnitude)
        {
            RotateObjectYAxis(rotationSpeed * directionalMagnitude);
        }

        #endregion

        #region Convenience

        /// <summary>
        /// Calculates placement position and rotation for the placing object,
        /// while validating and if needed updating renderer materials.
        /// </summary>
        /// <param name="animate">If update should be animated.
        /// Applies to position and rotation changes.</param>
        private void UpdatePlacingObject(bool animate)
        {
            Vector3 newPosition = GetPlacementPosition();

            const float minimalDistanceStep = 0.05f;

            // Lerp from current position if animation is enabled.
            if (animate &&
                animatePositionChange &&
                Vector3.Distance(placingObjectTemplate.position, newPosition) > minimalDistanceStep)
            {
                newPosition = Vector3.Lerp(placingObjectTemplate.position, newPosition, Time.deltaTime * positionChangeSpeed);
            }

            // Lerp from current rotation if animation is enabled. Otherwise set new rotation directly.
            if (animate && animateRotationChange)
                currentRotationY = Mathf.Lerp(currentRotationY, targetRotationY, Time.deltaTime * rotationChangeSpeed);
            else
                currentRotationY = targetRotationY;

            var newRotation = placingObjectTemplate.eulerAngles;
            newRotation.y = currentRotationY;

            placingObjectTemplate
                .SetPositionAndRotation(newPosition, Quaternion.Euler(newRotation));

            isPlacementValid = ValidatePlacement(placingObjectBounds);

            // Perform additional validation check if its set
            if (isPlacementValid && AdditionalValidator != null)
            {
                isPlacementValid = AdditionalValidator.IsPlacementValid(
                    placingObjectTemplate.gameObject, placingObjectBounds.PlacementBounds, placingObjectBounds.PlacementRotation);
            }

            Events.OnPlacementValidate.Invoke(isPlacementValid);
            GetPlacingObjectComponent(placingObjectTemplate)?.PlacementValidated(isPlacementValid);

            UpdatePlacementMaterials(placingObjectTemplate, isPlacementValid);
        }

        /// <summary>
        /// Validates placement bounds with ground alignment and bounds collision check.
        /// </summary>
        /// <param name="placingObject">Placing objects bounds</param>
        /// <returns>Returns true if placement positon & rotation is valid.</returns>
        private bool ValidatePlacement(IPlacementBounds placingObject)
        {
            if (groundAlignmentType != GroundAlignmentType.None)
            {
                // Reset/Set rotation without alignment, removes potential flickering
                placingObjectTemplate.localRotation = CurrentRotation;

                Bounds bounds = placingObject.PlacementBounds;
                Quaternion rotation = placingObject.PlacementRotation;

                if (!TryGroundAlignment(bounds, rotation))
                    return false;
            }
            
            // Checks if placement is inside the area, when clamping position is disabled.
            if (!clampInsidePlacementArea && 
                !placementArea.IsInside(placingObject.PlacementBounds.center))
                return false;

            {
                // Take these values again, may have changed duo to alignment.
                Bounds bounds = placingObject.PlacementBounds;
                Quaternion rotation = placingObject.PlacementRotation;
                return CheckForCollisions(bounds, rotation, obstacleLayer) == 0;
            }
        }

        /// <summary>
        /// Performs ground alignment based on <see cref="groundAlignmentType"/>.
        /// When the type is <see cref="GroundAlignmentType.ValidateOnly"/> object will
        /// not be adjusted in position or rotation, but will still be validated
        /// based on ground angle threshold.
        /// </summary>
        /// <param name="bounds">World space bounds</param>
        /// <param name="rotation">World rotation</param>
        /// <returns>Returns true if the placement is within expected <see cref="groundAngleThreshold"/>.</returns>
        private bool TryGroundAlignment(Bounds bounds, Quaternion rotation)
        {
            // World relative/Absolute rotation.
            var rotationOffset = CurrentRotation;

            // Calculate hits on the ground for the collider edges
            if (groundAlignment.TryGetAlignedPositionWithGround(
                bounds, rotation, groundLayer, out Quaternion rot, out Vector3 pos))
            {
                switch (groundAlignmentType)
                {
                    case GroundAlignmentType.AlignToGround:
                        // Align object to the ground by updating it's position and rotation
                        var objPos = placingObjectTemplate.position;
                        objPos.y = pos.y;
                        placingObjectTemplate.SetPositionAndRotation(objPos, rot * rotationOffset);
                        break;

                    case GroundAlignmentType.LowestPoint:
                        // Move object to the lowest point of the ground (at one of the 4 edges).
                        var points = groundAlignment.BottomPoints;
                        float minY = float.MaxValue;
                        for (int index = 0; index < points.Length; index++)
                        {
                            if (minY > points[index].y)
                                minY = points[index].y;
                        }

                        pos = placingObjectTemplate.position;
                        pos.y = minY;
                        placingObjectTemplate.position = pos;
                        break;

                    case GroundAlignmentType.ValidateOnly:
                    case GroundAlignmentType.None:
                        break;
                }

                // Validate that the angles are within threshold
                Vector3 euler = rot.eulerAngles;
                float angle = Mathf.Max(MathHelper.AngleDistance(euler.x, 0), MathHelper.AngleDistance(euler.z, 0));
                return groundAngleThreshold.IsWithinBounds(angle);
            }
            else if (debugLogsEnabled)
            {
                Debug.LogWarning("No ground nearby found or alignment failed! Try adapting configurations.");
            }

            return false;
        }

        /// <summary>
        /// Performs Physics overlapping collision check with specified
        /// bounds and rotation.
        /// </summary>
        /// <param name="bounds">World space bounds</param>
        /// <param name="rotation">World rotation</param>
        /// <param name="layer">Layer for collision</param>
        /// <returns>Returns true if there are NO colliders in the way.</returns>
        private int CheckForCollisions(Bounds bounds, Quaternion rotation, LayerMask layer)
        {
            int colliderCount = Physics.OverlapBoxNonAlloc(
                   bounds.center,
                   bounds.size / 2,
                   colliderHits,
                   rotation,
                   layer,
                   QueryTriggerInteraction.Collide);

            if (debugLogsEnabled)
            {
                for (int index = 0; index < colliderCount; index++)
                    Debug.Log("Hit object on placement: " + colliderHits[index].name);
            }

            return colliderCount;
        }

        /// <summary>
        /// Updates materials on all child <see cref="Renderer"/> components.
        /// In case no materials are set, this does nothing.
        /// </summary>
        /// <param name="placingObject">Root transform of the object</param>
        /// <param name="isValid">If placement is valid</param>
        private void UpdatePlacementMaterials(Transform placingObject, bool isValid)
        {
            if (!applyPlacementMaterials) return;

            var renderers = placingObject.GetComponentsInChildren<Renderer>();
            if (isValid)
            {
                if (validPlacementMaterials != null)
                {
                    foreach (var renderer in renderers)
                        renderer.sharedMaterials = validPlacementMaterials;
                }
            }
            else
            {
                if (invalidPlacementMaterials != null)
                {
                    foreach (var renderer in renderers)
                        renderer.sharedMaterials = invalidPlacementMaterials;
                }
            }
        }

        /// <summary>
        /// Shoots raycast based on current cursor position, validates the hit point and
        /// returns a validated position (snap and clamp features).
        /// In case of failure the last valid position is returned.
        /// </summary>
        /// <returns>Returns validated position for the placement of an object.</returns>
        private Vector3 GetPlacementPosition()
        {
            Vector3 screenPosition = CursorPositionProvider.CursorPosition;

            Ray ray = playerCamera.ScreenPointToRay(screenPosition);

            if (Physics.Raycast(ray, out RaycastHit hit, maxDetectionDistance, groundLayer, QueryTriggerInteraction.Ignore))
            {
                Vector3 newPosition = hit.point;

                // Snap to grid if enabled
                if (usePositionSnapping)
                    newPosition = gridSnapper.SnapToGrid(newPosition);

                if (clampInsidePlacementArea)
                    newPosition = placementArea.ClosestPosition(newPosition);

                // Save before animating
                targetPosition = newPosition;
                return newPosition;
            }
            else
            {
                // In case of failure, return previous position.
                return targetPosition;
            }
        }

        /// <summary>
        /// Gets a component that implements <see cref="IPlacingObject"/>.
        /// </summary>
        private IPlacingObject GetPlacingObjectComponent(Transform transform) =>
            transform.GetComponent<IPlacingObject>();

        /// <summary>
        /// Removes objects with colliders within the specified bounds and rotation.
        /// </summary>
        /// <param name="bounds">Bounds of placed object.</param>
        /// <param name="rotation">Rotation of placed object.</param>
        private void RemoveDestructables(Bounds bounds, Quaternion rotation)
        {
            int collisionCount = CheckForCollisions(bounds, rotation, destructablesLayer);

            // Destroy all colliders on the new placement.
            for (int index = 0; index < collisionCount; index++)
            {
                var obj = colliderHits[index].gameObject;
                ObjectPlacementEvents.Instance.OnObjectDestroy.Invoke(obj);

                if (destroyDestructableOnPlacement)
                {
                    Destroy(obj);   
                }

                colliderHits[index] = null;
            }
        }

        #endregion

        #region Debug

        private void OnDrawGizmos()
        {
            if (!drawGizmosEnabled) return;

            placementArea.RenderGizmos(gizmoPlacementAreaColor);

            if (usePositionSnapping)
                gridSnapper.RenderDebugPoints();

            if (!isPlacementActive) return;

            if (groundAlignment != null)
            {
                var points = groundAlignment.BottomPoints;
                foreach (var point in points)
                {
                    Gizmos.color = gizmoPlacementGroundPositionsColor;
                    Gizmos.DrawSphere(point, 0.3f);
                }
            }

            Bounds bounds = placingObjectBounds.PlacementBounds;
            Gizmos.matrix = Matrix4x4.TRS(bounds.center, placingObjectTemplate.gameObject.transform.rotation, bounds.size);

            Color transparentBoundsColor = gizmoPlacementBoundsColor;
            transparentBoundsColor.a = 0.1f;

            // Draw transparent cube
            Gizmos.color = transparentBoundsColor;
            Gizmos.DrawCube(Vector3.zero, Vector3.one);

            // Draw cube wire
            Gizmos.color = gizmoPlacementBoundsColor;
            Gizmos.DrawWireCube(Vector3.zero, Vector3.one);
        }

        #endregion
    }

}