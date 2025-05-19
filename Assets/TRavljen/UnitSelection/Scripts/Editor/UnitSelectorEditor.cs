using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;

namespace TRavljen.UnitSelection.Editor
{
    using UnityEngine;
    using UnityEditor;
    using TRavljen.UnitSelection;
    using TRavljen.EditorUtility;

    [CustomEditor(typeof(ASelectionArea), true)]
    public class SelectionAreaEditor : HiddenScriptPropertyEditor { }

    [CustomEditor(typeof(UnitSelector))]
    public class UnitSelectorEditor : Editor
    {

        private SerializedProperty configuration;
        private SerializedProperty inputControl;
        private SerializedProperty camera;
        private SerializedProperty selectionArea;
        private SerializedProperty inputType;

        private UnitSelector unitSelector;
        private Transform modulesContainer;

        // Editors
        private Editor inputEditor;
        private QuickAccessUnitSelectorEditor quickAccessEditor;
        private ActiveSelectionsEditor activeSelectionsEditor;
        private Editor eventsEditor;
        private Editor selectionAreaEditor;

        private const string selectedTabKey = "UnitSelector.selectedTab";
        private int SelectedTabIndex
        {
            set => EditorPrefs.SetInt(selectedTabKey, value);
            get => EditorPrefs.GetInt(selectedTabKey);
        }

        private void OnEnable()
        {
            configuration = serializedObject.FindProperty("configuration");
            inputControl = serializedObject.FindProperty("inputControl");
            camera = serializedObject.FindProperty("_camera");
            selectionArea = serializedObject.FindProperty("SelectionArea");
            inputType = serializedObject.FindProperty("inputType");

            unitSelector = target as UnitSelector;
            modulesContainer = FindModulesContainer();

            SetupInitialInputValue();
        }

        private void OnDisable()
        {
            DestroyEditor(ref inputEditor);
            DestroyEditor(ref quickAccessEditor);
            DestroyEditor(ref activeSelectionsEditor);
            DestroyEditor(ref eventsEditor);
            DestroyEditor(ref selectionAreaEditor);
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            InspectorGUITabs();

            if (serializedObject.ApplyModifiedProperties())
            {
                Undo.RecordObject(target, "Modify Unit Selector");

                SetDirty(target);
            }
        }

        private void SetupInitialInputValue()
        {
            switch ((InputType)inputType.intValue)
            {
                case InputType.None:
                    // This should avoid destroying the existing component, previous setup.
                    // Pre 2.1.0 update.
                    AInputControl control = unitSelector.GetComponent<AInputControl>();

                    if (control is InputKeysControl)
                        inputType.intValue = (int)InputType.LegacyInput;
#if ENABLE_INPUT_SYSTEM
                    else if (control is InputActionsControl)
                        inputType.intValue = (int)InputType.NewInputSystem;
#endif
                    else
                        break;

                    serializedObject.ApplyModifiedProperties();
                    serializedObject.Update();
                    break;

                case InputType.LegacyInput:
                case InputType.NewInputSystem:
                    break;
            }
        }

        private void InspectorGUITabs()
        {
            SelectedTabIndex = GUILayout.Toolbar(SelectedTabIndex, new string[] { "Main", "Selection Area", "Input", "Events" });
            EditorGUILayout.Space(12);

            switch (SelectedTabIndex)
            {
                case 0:
                    MainTabGUI();
                    break;

                case 1:
                    SelectionAreaTabGUI();
                    break;

                case 2:
                    InputTabGUI();
                    break;

                case 3:
                    EventsTabGUI();
                    break;
            }

            EditorGUILayout.Space(12);

            if (EditorGUILayout.LinkButton("Visit online guide for more information"))
            {
                Application.OpenURL("https://travljen.gitbook.io/unit-selection");
            }
        }

        #region Tabs

        private void MainTabGUI()
        {
            EditorGUILayout.PropertyField(camera);
            EditorGUILayout.PropertyField(configuration);

            RenderActiveSelectionsGUI();
            RenderQuickAccessGUI();
        }

        private void SelectionAreaTabGUI()
        {
            serializedObject.Update();
            EditorGUILayout.PropertyField(selectionArea);

            if (unitSelector.SelectionArea == null)
            {
                EditorGUILayout.HelpBox(
                    "You can reference any selection area here" +
                    "or use one of the common ones provided below.",
                    MessageType.Info);

                // Add buttons
                if (GUILayout.Button("Add 2D Screen Rectangle selection"))
                {
                    InstantiateSelectionArea("Assets/TRavljen/UnitSelection/Prefabs/2D Rectangle Selection Area.prefab");
                }

                if (GUILayout.Button("Add 3D World Cube Selection"))
                {
                    InstantiateSelectionArea("Assets/TRavljen/UnitSelection/Prefabs/3D Cube Selection Area.prefab");
                }
            }
            else if (unitSelector.SelectionArea != null)
            {
                if (selectionAreaEditor == null || selectionAreaEditor.target != unitSelector.SelectionArea)
                    selectionAreaEditor = CreateEditor(unitSelector.SelectionArea);

                if (selectionAreaEditor.serializedObject != null &&
                    unitSelector.SelectionArea.isActiveAndEnabled)
                {
                    EditorGUILayout.Space();
                    selectionAreaEditor.OnInspectorGUI();
                    EditorGUILayout.Space();
                }

                if (GUILayout.Button("Remove selection area"))
                {
                    selectionArea.objectReferenceValue = null;
                    Undo.DestroyObjectImmediate(unitSelector.SelectionArea.gameObject);
                    DestroyEditor(ref selectionAreaEditor);
                    EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene());
                }
            }
        }

        private void RenderActiveSelectionsGUI()
        {
            ActiveSelections selections = unitSelector.GetComponent<ActiveSelections>();

            if (activeSelectionsEditor == null)
                activeSelectionsEditor = CreateEditor(selections) as ActiveSelectionsEditor;

            if (PersistentFoldout.Foldout("Selections"))
            {
                EditorGUI.indentLevel++;
                activeSelectionsEditor.RenderGUI();
                EditorGUI.indentLevel--;
            }
        }

        private void RenderQuickAccessGUI()
        {
            if (PersistentFoldout.Foldout("Quick access"))
            {
                EditorGUI.indentLevel++;
                QuickAccessUnitSelector quickAccess = unitSelector.GetComponentInChildren<QuickAccessUnitSelector>();
                if (quickAccess != null)
                {
                    if (quickAccessEditor == null)
                        quickAccessEditor = CreateEditor(quickAccess) as QuickAccessUnitSelectorEditor;
                    
                    EditorGUILayout.HelpBox("Make sure to define input actions for your project.", MessageType.None);

                    quickAccessEditor.RenderGUI();

                    if (GUILayout.Button("Remove quick access feature"))
                    {
                        DestroyScript(quickAccess, ref quickAccessEditor);
                    }
                }
                else
                {
                    DestroyEditor(ref quickAccessEditor);
                    EditorGUILayout.HelpBox("You can use build-in solution for saving selection and " +
                                            "then accessing it through input controls.", MessageType.None);

                    if (GUILayout.Button("Add quick access feature"))
                    {
                        unitSelector.gameObject.AddComponent<QuickAccessUnitSelector>();
                    }
                }

                EditorGUI.indentLevel--;
            }
        }

        private void InputTabGUI()
        {

            var inputTypeValues = System.Enum.GetValues(typeof(InputType));
            EditorGUILayout.PropertyField(inputType);

            EditorGUILayout.Space();

            switch ((InputType)inputType.intValue)
            {
                case InputType.None:
                    AInputControl input = unitSelector.GetComponentInChildren<AInputControl>();
                    // Destroy input if its one of the built-in
#if ENABLE_INPUT_SYSTEM
                    if (input is InputKeysControl || input is InputActionsControl)
#else
                    if (input is InputKeysControl)
#endif
                    {
                        inputControl.objectReferenceValue = null;
                        DestroyScript(input, ref inputEditor);
                    }
                    // Otherwise leave a reference to be custom or null
                    else
                    {
                        EditorGUILayout.PropertyField(inputControl, new GUIContent("Custom input"));
                        EditorGUILayout.Space();
                    }

                    EditorGUILayout.HelpBox("You can manage active selections with public methods on UnitSelector.Instance. " +
                "However, all of the selection controls can be managed by this input.", MessageType.Info);
                    break;

                case InputType.LegacyInput:
#if ENABLE_INPUT_SYSTEM
                    InputActionsControl actionInput = unitSelector.GetComponentInChildren<InputActionsControl>();

                    if (actionInput != null)
                    {
                        DestroyScript(actionInput, ref inputEditor);
                    }
#endif
                    InputKeysControl keyInput = unitSelector.GetComponentInChildren<InputKeysControl>();

                    if (keyInput == null)
                    {
                        keyInput = AddModule<InputKeysControl>();
                        inputControl.objectReferenceValue = keyInput;
                    }

                    if (inputEditor == null)
                        inputEditor = CreateEditor(keyInput);

                    if (inputEditor is InputKeysControlEditor keysEditor)
                        keysEditor.RenderInspectorGUI();
                    else
                        inputEditor.OnInspectorGUI();

                    break;

                case InputType.NewInputSystem:
                    keyInput = unitSelector.GetComponentInChildren<InputKeysControl>();

                    if (keyInput != null)
                        DestroyScript(keyInput, ref inputEditor);

#if ENABLE_INPUT_SYSTEM
                    actionInput = unitSelector.GetComponentInChildren<InputActionsControl>();

                    if (actionInput == null)
                    {
                        actionInput = AddModule<InputActionsControl>();
                        inputControl.objectReferenceValue = actionInput;
                    }

                    // Render Input settings
                    if (inputEditor == null)
                        inputEditor = CreateEditor(actionInput);

                    if (inputEditor is InputActionsControlEditor actionsEditor)
                        actionsEditor.RenderInspectorGUI();
                    else
                        inputEditor.OnInspectorGUI();
#else
                    EditorGUILayout.HelpBox("You do not have the new Input System from Unity in the project. " +
                        "To use this feature, please first import the package into your project.", MessageType.Error);
#endif
                    break;
            }
        }

        private void EventsTabGUI()
        {
            SelectionEventsObserver eventsObserver = unitSelector.GetComponentInChildren<SelectionEventsObserver>();

            EditorGUILayout.Space();
            EditorGUILayout.HelpBox("Set up events through editor here or use SelectionEvents.Instance to do this in code.", MessageType.Info);
            EditorGUILayout.Space();

            if (eventsObserver != null)
            {
                if (eventsEditor == null)
                    eventsEditor = CreateEditor(eventsObserver);

                eventsEditor.OnInspectorGUI();

                if (GUILayout.Button("Remove events"))
                {
                    DestroyScript(eventsObserver, ref eventsEditor);
                }
            }
            else
            {
                if (GUILayout.Button("Add events"))
                {
                    AddModule<SelectionEventsObserver>();
                }
            }
        }

#endregion

        #region Convenience

        private Transform FindModulesContainer()
        {
            Transform parent = unitSelector.transform;

            for (int index = 0; index < parent.childCount; index++)
            {
                if (parent.GetChild(index).name == "Modules")
                {
                    return parent.GetChild(index);
                }
            }

            return null;
        }

        private Module AddModule<Module>() where Module : MonoBehaviour
        {
            if (modulesContainer == null)
            {
                modulesContainer = new GameObject("Modules").transform;
                modulesContainer.SetParent((target as MonoBehaviour).gameObject.transform);
            }

            return modulesContainer.gameObject.AddComponent<Module>();
        }

        private void DestroyScript<AnyEditor>(MonoBehaviour script, ref AnyEditor scriptEditor) where AnyEditor : Editor
        {
            if (script == null) return;

            var obj = script.gameObject;
            Undo.DestroyObjectImmediate(script);
            DestroyEditor(ref scriptEditor);
            EditorUtility.SetDirty(obj);
        }

        private void SetDirty(Object target)
        {
            // Do this only on Editor component when in app is not running,
            // cannot mark runtime instantiated objects as dirty.
            if (!Application.isPlaying)
                EditorUtility.SetDirty(target);
        }

        private void DestroyEditor<AnyEditor>(ref AnyEditor editor) where AnyEditor : Editor
        {
            if (editor != null)
            {
                DestroyImmediate(editor);
                editor = null;
            }
        }

        private void InstantiateSelectionArea(string assetPath)
        {
            var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(assetPath);

            if (prefab == null)
            {
                Debug.LogError("Prefab not found at path: " + assetPath);
                return;
            }

            var newInstance = (GameObject)PrefabUtility.InstantiatePrefab(prefab, unitSelector.transform);

            if (newInstance.TryGetComponent(out ASelectionArea area))
            {
                selectionArea.objectReferenceValue = area;
                EditorUtility.SetDirty(target);
            }
            
            Undo.RegisterCreatedObjectUndo(newInstance, "Added new selection area");
            EditorUtility.SetDirty(newInstance);
        }

        #endregion
    }
}
