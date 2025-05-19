#if UNITY_EDITOR
namespace TRavljen.PlacementSystem.Editor
{
    using UnityEngine;
    using UnityEditor;
    using EditorUtility;

    [CustomEditor(typeof(ObjectPlacement))]
    public class ObjectPlacementEditor : Editor
    {

        // Setup
        private SerializedProperty placementObjectPrefab;
        private SerializedProperty groundLayer;
        private SerializedProperty obstacleLayer;
        private SerializedProperty destructablesLayer;
        private SerializedProperty destroyOnPlacement;
        private SerializedProperty maxDetectionDistance;
        private SerializedProperty ignoreWhenOverUI;
        private SerializedProperty playerCamera;

        // Rotation
        private SerializedProperty rotationSpeed;
        private SerializedProperty preserveRotation;

        // Bounding box
        private SerializedProperty placementAreaType;
        private SerializedProperty placementAreaBox;
        private SerializedProperty placementAreaSphere;
        private SerializedProperty clampInsidePlacementArea;

        // Ground alignment
        private SerializedProperty groundAlignmentType;
        private SerializedProperty groundAlignmentDetectionOffset;
        private SerializedProperty groundAngleThreshold;

        // Grid
        private SerializedProperty usePositionSnapping;
        private SerializedProperty gridOrigin;
        private SerializedProperty gridScale;

        // Visuals
        private SerializedProperty applyPlacementMaterials;
        private SerializedProperty validPlacementMaterials;
        private SerializedProperty invalidPlacementMaterials;

        // Animations
        private SerializedProperty allowPlacementDuringAnimations;
        private SerializedProperty animatingAngleThreshold;
        private SerializedProperty animatingDistanceThreshold;
        private SerializedProperty animatePositionChange;
        private SerializedProperty positionChangeSpeed;
        private SerializedProperty animateRotationChange;
        private SerializedProperty rotationChangeSpeed;

        // Gizmos
        private SerializedProperty gizmoPlacementAreaColor;
        private SerializedProperty gizmoPlacementBoundsColor;
        private SerializedProperty gizmoPlacementGroundPositionsColor;
        private SerializedProperty gizmoGridOriginColor;
        private SerializedProperty gizmoGridPointsColor;
        private SerializedProperty gizmoGridPlacementPointColor;
        private SerializedProperty gizmosGridSphereSize;

        // Debug
        private SerializedProperty debugLogsEnabled;
        private SerializedProperty drawGizmosEnabled;

        // Editor Properties
        private ObjectPlacement objectPlacement;

        private Editor inputSystemEditor;
        private Editor observerEditor;
        private Editor prefabsEditor;

        private const string selectedTabKey = "ObjectPlacement.selectedTab";
        private int SelectedTabIndex
        {
            set => EditorPrefs.SetInt(selectedTabKey, value);
            get => EditorPrefs.GetInt(selectedTabKey);
        }

        private void OnEnable()
        {
            objectPlacement = target as ObjectPlacement;

            placementObjectPrefab = serializedObject.FindProperty("placementObjectPrefab");
            groundLayer = serializedObject.FindProperty("groundLayer");
            obstacleLayer = serializedObject.FindProperty("obstacleLayer");
            destructablesLayer = serializedObject.FindProperty("destructablesLayer");
            destroyOnPlacement = serializedObject.FindProperty("destroyDestructableOnPlacement");
            maxDetectionDistance = serializedObject.FindProperty("maxDetectionDistance");
            ignoreWhenOverUI = serializedObject.FindProperty("ignoreWhenOverUI");
            playerCamera = serializedObject.FindProperty("playerCamera");

            // Rotation
            rotationSpeed = serializedObject.FindProperty("rotationSpeed");
            preserveRotation = serializedObject.FindProperty("preserveRotation");

            // Snapping
            usePositionSnapping = serializedObject.FindProperty("usePositionSnapping");
            gridOrigin = serializedObject.FindProperty("gridSnapper.gridOrigin");
            gridScale = serializedObject.FindProperty("gridSnapper.gridScale");

            gizmoGridOriginColor = serializedObject.FindProperty("gridSnapper.gizmoGridOriginColor");
            gizmoGridPointsColor = serializedObject.FindProperty("gridSnapper.gizmoGridPointsColor");
            gizmoGridPlacementPointColor = serializedObject.FindProperty("gridSnapper.gizmoGridPlacementPointColor");
            gizmosGridSphereSize = serializedObject.FindProperty("gridSnapper.gizmosSphereSize");

            // Bounding box
            placementAreaType = serializedObject.FindProperty("placementArea.boundsType");
            placementAreaBox = serializedObject.FindProperty("placementArea.boxBounds");
            placementAreaSphere = serializedObject.FindProperty("placementArea.sphereBounds");
            clampInsidePlacementArea = serializedObject.FindProperty("clampInsidePlacementArea");

            // Ground Alignment
            groundAlignmentType = serializedObject.FindProperty("groundAlignmentType");
            groundAlignmentDetectionOffset = serializedObject.FindProperty("groundAlignment.GroundDetectionOffset");
            groundAngleThreshold = serializedObject.FindProperty("groundAngleThreshold");

            // Visuals
            applyPlacementMaterials = serializedObject.FindProperty("applyPlacementMaterials");
            validPlacementMaterials = serializedObject.FindProperty("validPlacementMaterials");
            invalidPlacementMaterials = serializedObject.FindProperty("invalidPlacementMaterials");

            // Animations
            allowPlacementDuringAnimations = serializedObject.FindProperty("allowPlacementDuringAnimations");
            animatingAngleThreshold = serializedObject.FindProperty("animatingAngleThreshold");
            animatingDistanceThreshold = serializedObject.FindProperty("animatingDistanceThreshold");
            animatePositionChange = serializedObject.FindProperty("animatePositionChange");
            positionChangeSpeed = serializedObject.FindProperty("positionChangeSpeed");
            animateRotationChange = serializedObject.FindProperty("animateRotationChange");
            rotationChangeSpeed = serializedObject.FindProperty("rotationChangeSpeed");

            // Debug
            debugLogsEnabled = serializedObject.FindProperty("debugLogsEnabled");
            drawGizmosEnabled = serializedObject.FindProperty("drawGizmosEnabled");

            // Gizmos
            gizmoPlacementAreaColor = serializedObject.FindProperty("gizmoPlacementAreaColor");
            gizmoPlacementBoundsColor = serializedObject.FindProperty("gizmoPlacementBoundsColor");
            gizmoPlacementGroundPositionsColor = serializedObject.FindProperty("gizmoPlacementGroundPositionsColor");
        }

        private void OnDisable()
        {
            if (inputSystemEditor)
            {
                DestroyImmediate(inputSystemEditor);
                inputSystemEditor = null;
            }

            if (observerEditor)
            {
                DestroyImmediate(observerEditor);
                observerEditor = null;
            }

            if (prefabsEditor)
            {
                DestroyImmediate(prefabsEditor);
                prefabsEditor = null;
            }
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            SelectedTabIndex = GUILayout.Toolbar(SelectedTabIndex, new string[] { "Setup", "Input", "Prefabs", "Visuals", "Events" });
            EditorGUILayout.Space(12);

            switch (SelectedTabIndex)
            {
                case 0:
                    SetupTabGUI();
                    break;

                case 1:
                    InputTabGUI();
                    break;

                case 2:
                    PrefabTabsGUI();
                    break;

                case 3:
                    VisualsTabGUI();
                    break;

                case 4:
                    EventsTabGUI();
                    break;
            }

            if (serializedObject.ApplyModifiedProperties())
            {
                // Record undo and mark object as dirty
                Undo.RecordObject(target, "Modify Object Placement Configuration");
                SetDirty(target);
            }

            EditorGUILayout.Space(12);
            if (EditorGUILayout.LinkButton("Visit online guide for more information"))
            {
                Application.OpenURL("https://travljen.gitbook.io/object-placement/how-it-works/object-placement");
            }
        }

        private void SetupTabGUI()
        {
            EditorGUILayout.PropertyField(playerCamera);
            EditorGUILayout.PropertyField(placementObjectPrefab);

            if (PersistentFoldout.Foldout("Detection", "travljen.object_placement.detection"))
            {
                EditorGUI.indentLevel++;
                EditorGUILayout.PropertyField(ignoreWhenOverUI);

                EditorGUILayout.PropertyField(groundLayer);
                EditorGUILayout.PropertyField(obstacleLayer);
                EditorGUILayout.PropertyField(maxDetectionDistance);

                EditorGUILayout.Space();
                EditorGUILayout.LabelField("Destructable", EditorStyles.boldLabel);
                EditorGUILayout.HelpBox("You can use destructables layer to specify if any objects like environment or props may be destroyed when placing an object.", MessageType.Info);
                EditorGUILayout.PropertyField(destroyOnPlacement, new GUIContent("Destroy"));
                EditorGUILayout.PropertyField(destructablesLayer);
                EditorGUILayout.Space();
                EditorGUI.indentLevel--;
            }

            if (PersistentFoldout.Foldout("Placement area", "travljen.object_placement.placement_area"))
            {
                EditorGUI.indentLevel++;
                EditorGUILayout.PropertyField(clampInsidePlacementArea, new GUIContent("Clamp Inside"));
                RenderPlacementAreaInfo();
                EditorGUI.indentLevel--;
            }

            if (PersistentFoldout.Foldout("Rotation", "travljen.object_placement.rotation"))
            {
                EditorGUI.indentLevel++;
                EditorGUILayout.PropertyField(rotationSpeed);
                EditorGUILayout.PropertyField(preserveRotation);
                EditorGUI.indentLevel--;
            }

            if (PersistentFoldout.Foldout("Ground Alignment", "travljen.object_placement.alignment"))
            {
                EditorGUI.indentLevel++;
                EditorGUILayout.PropertyField(groundAlignmentDetectionOffset);
                EditorGUILayout.PropertyField(groundAlignmentType);
                EditorGUILayout.PropertyField(groundAngleThreshold);
                EditorGUI.indentLevel--;
            }

            if (PersistentFoldout.Foldout("Grid Snapping", "travljen.object_placement.grid_snapping"))
            {
                EditorGUI.indentLevel++;
                EditorGUILayout.PropertyField(usePositionSnapping);
                EditorGUILayout.PropertyField(gridOrigin);
                EditorGUILayout.PropertyField(gridScale);
                EditorGUI.indentLevel--;
            }

            if (PersistentFoldout.Foldout("Debug", "travljen.object_placement.debug"))
            {
                EditorGUI.indentLevel++;
                EditorGUILayout.PropertyField(debugLogsEnabled);
                EditorGUILayout.PropertyField(drawGizmosEnabled);

                if (drawGizmosEnabled.boolValue)
                {
                    GUILayout.Space(10);

                    if (PersistentFoldout.Foldout("Gizmo Colors", "travljen.object_placement.gizmo_colors"))
                    {
                        EditorGUI.indentLevel++;

                        EditorGUILayout.PropertyField(gizmoPlacementAreaColor, new GUIContent("Placement Area"));
                        EditorGUILayout.PropertyField(gizmoPlacementBoundsColor, new GUIContent("Placement Bounds"));
                        EditorGUILayout.PropertyField(gizmoPlacementGroundPositionsColor, new GUIContent("Ground Position"));

                        EditorGUILayout.PropertyField(gizmosGridSphereSize, new GUIContent("Grid sphere size"));
                        EditorGUILayout.PropertyField(gizmoGridOriginColor, new GUIContent("Grid Origin"));
                        EditorGUILayout.PropertyField(gizmoGridPointsColor, new GUIContent("Grid Points"));
                        EditorGUILayout.PropertyField(gizmoGridPlacementPointColor, new GUIContent("Grid Placement Point"));
                        EditorGUI.indentLevel--;
                    }
                }

                EditorGUI.indentLevel--;
            }
        }

        private void VisualsTabGUI()
        {
            // Animations
            if (PersistentFoldout.Foldout("Animations", "travljen.object_placement.animations"))
            {
                EditorGUI.indentLevel++;
                EditorGUILayout.PropertyField(allowPlacementDuringAnimations, new GUIContent("Place During Animation"));
                EditorGUILayout.PropertyField(animatingAngleThreshold);
                EditorGUILayout.PropertyField(animatingDistanceThreshold);

                EditorGUILayout.Space();

                EditorGUILayout.LabelField("Position", EditorStyles.boldLabel);

                EditorGUI.indentLevel++;
                EditorGUILayout.PropertyField(animatePositionChange, new GUIContent("Animate"));

                if (animatePositionChange.boolValue)
                    EditorGUILayout.PropertyField(positionChangeSpeed, new GUIContent("Animation speed"));
                EditorGUI.indentLevel--;

                EditorGUILayout.LabelField("Rotation", EditorStyles.boldLabel);
                EditorGUI.indentLevel++;

                EditorGUILayout.PropertyField(animateRotationChange, new GUIContent("Animate"));

                if (animateRotationChange.boolValue)
                    EditorGUILayout.PropertyField(rotationChangeSpeed, new GUIContent("Animation speed"));
                EditorGUI.indentLevel--;

                EditorGUI.indentLevel--;
                // If expanded, add extra spacing
                EditorGUILayout.Space();
            }

            EditorGUILayout.Space();

            // Materials
            GUILayout.BeginHorizontal();
            GUILayout.Label("Apply Materials", EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(applyPlacementMaterials, new GUIContent(""));
            GUILayout.EndHorizontal();

            if (applyPlacementMaterials.boolValue)
            {
                EditorGUI.indentLevel++;
                EditorGUILayout.HelpBox(
                    "You can set materials here that will be applied to the " +
                    "placing object during placement.\n\n" +
                    "For each object to use custom materials, you can pass " +
                    "them along when invoking `ObjectPlacement.BeginPlacement` method.\n\n" +
                    "For applying custom events you can also use `PlacingObjectObserver` component " +
                    "and hookup events with your own implementation", MessageType.None);
                EditorGUILayout.Space();
                EditorGUI.indentLevel++;
                EditorGUILayout.PropertyField(validPlacementMaterials);
                EditorGUILayout.PropertyField(invalidPlacementMaterials);
                EditorGUI.indentLevel--;
                EditorGUI.indentLevel--;
            }
        }

        private void InputTabGUI()
        {
            var isInvalid = IsObjectInvalid(objectPlacement.gameObject);
            
            if (isInvalid)
            {
                EditorGUILayout.HelpBox("You cannot change input on prefab to remove it. You can only add it on prefab in the scene." +
                    "\nPlease change or remove input in prefab itself.", MessageType.Warning);
                EditorGUILayout.Space();
            }

            EditorGUI.BeginDisabledGroup(isInvalid);
            
            var existingInput = objectPlacement.GetComponentInChildren<AInputControl>();

            if (existingInput == null)
            {
                if (inputSystemEditor != null)
                {
                    DestroyImmediate(inputSystemEditor);
                    inputSystemEditor = null;
                }
                
                EditorGUILayout.HelpBox(
                    "You can manage active placement and currently placing object with public methods on ObjectPlacement.Instance. " +
                    "However, all of the placement controls can be managed by this input", MessageType.Info);

                if (GUILayout.Button("Add Legacy Input"))
                {
                    AddChildObjectComponent<KeyInputControl>("Input");
                }

#if ENABLE_INPUT_SYSTEM
                if (GUILayout.Button("Add New Input"))
                { 
                    AddChildObjectComponent<ActionInputControl>("Input");
                }
#else
                EditorGUILayout.HelpBox("You do not have the new Input System from Unity in the project. " +
                    "To use this feature, please first import the package into your project.", MessageType.Error);
#endif
            }
            else
            {
                if (inputSystemEditor == null || inputSystemEditor.target != existingInput)
                {
                    if (inputSystemEditor != null)
                    {
                        DestroyImmediate(inputSystemEditor);
                        inputSystemEditor = null;
                    }

                    inputSystemEditor = CreateEditor(existingInput);
                    
                }

                inputSystemEditor.OnInspectorGUI();
                
                EditorGUILayout.Space(12);
                
                if (RightAlignedButton("Remove input"))
                    DestroyScript(existingInput, ref inputSystemEditor);
            }

            EditorGUI.EndDisabledGroup();
        }

        private void PrefabTabsGUI()
        {
            PlacementPrefabs prefabs = objectPlacement.GetComponentInChildren<PlacementPrefabs>();

            bool prefabsSet = prefabs != null;
            var hasInput = objectPlacement.GetComponentInChildren<AInputControl>() != null;

            if (hasInput == false)
            {
                EditorGUILayout.Space();
                EditorGUILayout.HelpBox("Without the provided input component, " +
                    "you can access this feature with ObjectPlacement.Instance.GetPlacementPrefabs().", MessageType.Info);
                EditorGUILayout.Space();
            }

            if (!prefabsSet)
            {
                EditorGUILayout.HelpBox("You can setup list of prefabs for placement directly from this component.\n" +
                    "This supports simple list of prefabs, current selection is controlled by the input component.", MessageType.Info);

                EditorGUILayout.Space();

                if (GUILayout.Button("Add prefabs"))
                {
                    AddChildObjectComponent<PlacementPrefabs>("Prefabs");
                }

                EditorGUILayout.Space();
            }
            else
            {
                if (prefabsEditor == null)
                    prefabsEditor = CreateEditor(prefabs);

                prefabsEditor.OnInspectorGUI();

                EditorGUILayout.Space(24);

                RenderRemoveComponentGUI("Remove prefabs", prefabs, ref prefabsEditor);
            }
        }

        private void EventsTabGUI()
        {
            ObjectPlacementObserver observer = objectPlacement.GetComponentInChildren<ObjectPlacementObserver>();
            bool observerSet = observer != null;

            if (!observerSet)
            {
                EditorGUILayout.Space();
                EditorGUILayout.HelpBox("You can setup events directly from this component.", MessageType.None);
                EditorGUILayout.Space();

                if (GUILayout.Button("Add events"))
                {
                    AddChildObjectComponent<ObjectPlacementObserver>("Events");
                }

                EditorGUILayout.Space();
            }
            else
            {
                if (observerEditor == null)
                    observerEditor = CreateEditor(observer);
                else
                    observerEditor.serializedObject.Update();

                observerEditor.OnInspectorGUI();
            }

            EditorGUILayout.HelpBox("If you wish to have more control on how to setup events, you can add `ObjectPlacementObserver` on any game object or use `ObjectPlacementEvents` directly in code.", MessageType.Info);

            if (observerSet)
            {
                EditorGUILayout.Space();

                RenderRemoveComponentGUI("Remove events", observer, ref observerEditor);
            }
        }

        private void RenderPlacementAreaInfo()
        {
            EditorGUILayout.PropertyField(placementAreaType);
            
            var type = (PlacementAreaType)placementAreaType.intValue;
            
            switch (type)
            {
                case PlacementAreaType.Anywhere: break;
                case PlacementAreaType.Box:
                    EditorGUILayout.PropertyField(placementAreaBox);
                    break;
            
                case PlacementAreaType.Sphere:
                    EditorGUILayout.PropertyField(placementAreaSphere);
                    break;
                
                case PlacementAreaType.Custom:

                    if (objectPlacement.GetComponent<IPlacementArea>() == null)
                    {
                        EditorGUILayout.HelpBox("Attach your custom placement area and set the customArea reference in Awake method.", MessageType.Info);
                        
                        EditorGUILayout.LabelField("Area Managers Available", EditorStyles.boldLabel);
                        EditorGUILayout.HelpBox("Manage list of sphere areas.", MessageType.None);
                        
                        if (RightAlignedButton("Add sphere area manager"))
                        {
                            Undo.AddComponent<SpherePlacementAreaManager>(objectPlacement.gameObject);
                        }
                        
                        EditorGUILayout.HelpBox("Manage list of objects which define their own placement area.\n \n" +
                                                "Adding, removing, enabling or disabling those objects will also add/remove its placement area.", MessageType.None);
                        if (RightAlignedButton("Add object area manager"))
                        {
                            Undo.AddComponent<ObjectPlacementAreaManager>(objectPlacement.gameObject);
                        }
                    }
                    else
                    {
                        EditorGUILayout.HelpBox("Set the customArea reference for your area in Awake method.", MessageType.Info);
                    }
                    
                    break;
            }
        }

        private static bool RightAlignedButton(string text)
        {
            GUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
            var clicked = GUILayout.Button(text);
            GUILayout.EndHorizontal();
            return clicked;
        }

        private ComponentType AddChildObjectComponent<ComponentType>(string name) where ComponentType : MonoBehaviour
        {
            Transform parent = objectPlacement.gameObject.transform;
            GameObject childObject = new GameObject(name);
            childObject.transform.SetParent(parent);
            ComponentType newComponent = childObject.AddComponent<ComponentType>();
            SetDirty(objectPlacement);
            return newComponent;
        }

        private void DestroyScript(MonoBehaviour script, ref Editor scriptEditor)
        {
            if (script == null) return;

            SetDirty(objectPlacement);

            if (scriptEditor != null)
            {
                DestroyImmediate(scriptEditor);
                scriptEditor = null;
            }

            DestroyImmediate(script.gameObject);
        }

        private void RenderRemoveComponentGUI(string buttonText, MonoBehaviour script, ref Editor customEditor)
        {
            var isInvalid = IsObjectInvalid(script.gameObject);

            if (isInvalid)
            {
                EditorGUILayout.HelpBox("You cannot remove an object that is part of prefab in the scene." +
                                        "\nPlease do so in prefab itself or unpack the prefab.",
                    MessageType.Warning);
                return;
            }

            EditorGUI.BeginDisabledGroup(isInvalid);
            if (GUILayout.Button(buttonText))
            {
                DestroyScript(script, ref customEditor);
            }
            EditorGUI.EndDisabledGroup();
        }

        private void SetDirty(Object target)
        {
            if (!Application.isPlaying)
                EditorUtility.SetDirty(target);
        }
        
        private static bool IsObjectInvalid(GameObject go)
        {
            // Makes sure that object which:
            // 1. Is selected in scene and is a prefab, cannot be edited.
            // 2. Is selected in inspector as prefab asset, cannot be selected
            // 3. Only prefabs which are opened and have scene focus can be edited.
            // 4. Non prefabs MAY be edited always.
            
            var isPrefabInstance = PrefabUtility.IsPartOfPrefabInstance(go);
            var isPrefabSelectedInProject = PrefabUtility.GetCorrespondingObjectFromSource(go) != null;
            var disabled = isPrefabInstance || isPrefabSelectedInProject;
            var isPersistent = EditorUtility.IsPersistent(go);
            return disabled || isPersistent;
        }
    }
}
#endif