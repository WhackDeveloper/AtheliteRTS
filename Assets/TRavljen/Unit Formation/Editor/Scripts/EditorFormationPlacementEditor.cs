using UnityEngine;

namespace TRavljen.UnitFormation.Editor
{

    using UnityEditor;

    [CustomEditor(typeof(EditorFormationPlacement))]
    public class EditorFormationPlacementEditor : Editor
    {

        #region Properties

        // Configuration
        SerializedProperty units;
        SerializedProperty formationType;
        SerializedProperty applyNoise;
        SerializedProperty noiseScale;
        SerializedProperty targetGameObject;
        SerializedProperty groundDetectionType;
        SerializedProperty groundMask;
        SerializedProperty groundMaxDistance;

        // Gizmos
        SerializedProperty showGizmos;
        SerializedProperty gizmoPositionColor;
        SerializedProperty gizmoDirectionColor;
        SerializedProperty gizmoSphereRadius;

        // Formations
        SerializedProperty ringFormation;
        SerializedProperty circleFormation;
        SerializedProperty computedCircleFormation;
        SerializedProperty coneFormation;
        SerializedProperty rectangleBorderFormation;
        SerializedProperty rectangleFormation;
        SerializedProperty lineFormation;
        SerializedProperty triangleFormation;
        SerializedProperty triangleBorderFormation;

        EditorFormationPlacement controller;

        private FormationType currentFormationType => (FormationType)formationType.intValue;

        private GUIStyle boldStyle;

        #endregion

        void OnEnable()
        {
            boldStyle = EditorStyles.boldLabel;

            // Configuration
            formationType = serializedObject.FindProperty("type");
            units = serializedObject.FindProperty("units");
            applyNoise = serializedObject.FindProperty("applyNoise");
            noiseScale = serializedObject.FindProperty("noiseScale");
            targetGameObject = serializedObject.FindProperty("target");
            groundDetectionType = serializedObject.FindProperty("groundDetectionType");
            groundMask = serializedObject.FindProperty("groundMask");
            groundMaxDistance = serializedObject.FindProperty("groundMaxDistance");

            // Gizmos
            showGizmos = serializedObject.FindProperty("showGizmos");
            gizmoPositionColor = serializedObject.FindProperty("positionColor");
            gizmoDirectionColor = serializedObject.FindProperty("directionColor");
            gizmoSphereRadius = serializedObject.FindProperty("sphereRadius");

            // Formations
            ringFormation = serializedObject.FindProperty("ringFormation");
            circleFormation = serializedObject.FindProperty("circleFormation");
            computedCircleFormation = serializedObject.FindProperty("computedCircleFormation");
            coneFormation = serializedObject.FindProperty("coneFormation");
            rectangleBorderFormation = serializedObject.FindProperty("rectangleBorderFormation");
            rectangleFormation = serializedObject.FindProperty("rectangleFormation");
            lineFormation = serializedObject.FindProperty("lineFormation");
            triangleFormation = serializedObject.FindProperty("triangleFormation");
            triangleBorderFormation = serializedObject.FindProperty("triangleBorderFormation");

            controller = target as EditorFormationPlacement;
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            BeginSection("Formation");

            EditorGUILayout.PropertyField(formationType);

            string warning = GetFormationWarningBoxText();
            if (warning != null)
                EditorGUILayout.HelpBox(warning, MessageType.Info);

            NoiseGUI();

            FormationSpecificGUI();
            EndSection();

            BeginSection("Placement");
            GroundDetectionGUI();
            EndSection();


            EditorGUILayout.Space();

            GizmosGUI();
            EditorGUILayout.Space();

            // Has its own spacing, if shown
            ApplyFormationButtonGUI();

            TargetChildrenHelperGUI();
            EditorGUILayout.Space();

            SelectedUnitsGUI();

            serializedObject.ApplyModifiedProperties();
        }

        private void GizmosGUI()
        {
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Gizmos", boldStyle);

            if (GUILayout.Button(showGizmos.boolValue ? "Disable" : "Enable"))
            {
                showGizmos.boolValue = !showGizmos.boolValue;
            }

            EditorGUILayout.EndHorizontal();

            if (showGizmos.boolValue)
            {
                BeginSection(null);
                EditorGUILayout.PropertyField(gizmoPositionColor);
                EditorGUILayout.PropertyField(gizmoDirectionColor);
                EditorGUILayout.PropertyField(gizmoSphereRadius);
                EndSection();
            }
        }

        private void GroundDetectionGUI()
        {
            EditorGUILayout.PropertyField(groundDetectionType);
            GroundDetectionType type = (GroundDetectionType)groundDetectionType.intValue;

            // Show mask when raycast type is selected
            if (type == GroundDetectionType.Raycast)
                EditorGUILayout.PropertyField(groundMask);

            // Show max distance for all states but disabled.
            if (type != GroundDetectionType.Disabled)
                EditorGUILayout.PropertyField(groundMaxDistance);
        }

        private void NoiseGUI()
        {
            EditorGUILayout.PropertyField(applyNoise);
            if (applyNoise.boolValue)
                EditorGUILayout.PropertyField(noiseScale);
        }

        private string GetFormationWarningBoxText()
        {
            switch (currentFormationType)
            {
                case FormationType.Circle:
                    return "Finds positions inside the circle, starting at border. Formation parameters should be adjusted for each unit count " +
                        "as the same parameters will not work for all cases." +
                        "\n\nAdjust 'Max Radius' and it's 'Unit Spacing' to get the desired result.";

                case FormationType.ComputedCircle:
                    return "Positions units within a filled circle. Works similar to 'Circle' formation, but here it will automatically " +
                        "find the best fit for your unit count by iterating through multiple options. It starts of with minimal radius " +
                        "and increases it from there. " +
                        "\n\nWith high unit count, 'Max Iterations' should be increased or 'Min Radius' where the computation begins.";

                default: return null;
            }
        }

        private void FormationSpecificGUI()
        {
            switch (currentFormationType)
            {
                case FormationType.Ring:
                    ringFormation.isExpanded = true;
                    EditorGUILayout.PropertyField(ringFormation);
                    break;

                case FormationType.Circle:
                    circleFormation.isExpanded = true;
                    EditorGUILayout.PropertyField(circleFormation);
                    break;

                case FormationType.ComputedCircle:
                    computedCircleFormation.isExpanded = true;
                    EditorGUILayout.PropertyField(computedCircleFormation);
                    break;

                case FormationType.Cone:
                    coneFormation.isExpanded = true;
                    EditorGUILayout.PropertyField(coneFormation);
                    break;

                case FormationType.Line:
                    lineFormation.isExpanded = true;
                    EditorGUILayout.PropertyField(lineFormation);
                    break;

                case FormationType.Rectangle:
                    rectangleFormation.isExpanded = true;
                    EditorGUILayout.PropertyField(rectangleFormation);
                    break;

                case FormationType.RectangleBorder:
                    rectangleBorderFormation.isExpanded = true;
                    EditorGUILayout.PropertyField(rectangleBorderFormation);
                    break;

                case FormationType.Triangle:
                    triangleFormation.isExpanded = true;
                    EditorGUILayout.PropertyField(triangleFormation);
                    break;

                case FormationType.TriangleBorder:
                    triangleBorderFormation.isExpanded = true;
                    EditorGUILayout.PropertyField(triangleBorderFormation);
                    break;
            }
        }

        private void TargetChildrenHelperGUI()
        {
            EditorGUILayout.LabelField("Helper", boldStyle);
            EditorGUILayout.HelpBox("You can retrieve child transforms from Target when they are grouped together or reference them manually in the list below.", MessageType.Info);

            EditorGUILayout.BeginHorizontal();

            EditorGUILayout.PropertyField(targetGameObject);

            if (GUILayout.Button("Get children"))
            {
                if (targetGameObject.objectReferenceValue == null)
                {
                    Debug.LogError("No Target was set! Cannot retrieve target children");
                }
                else
                {
                    var root = targetGameObject.objectReferenceValue as Transform;
                    controller.ClearUnits();

                    for (int index = 0; index < root.transform.childCount; index++)
                    {
                        var child = root.GetChild(index);
                        controller.AddUnit(child);
                    }

                    EditorUtility.SetDirty(controller);
                }
            }

            EditorGUILayout.EndHorizontal();
        }

        private void SelectedUnitsGUI()
        {
            if (units.arraySize == 0)
            {
                EditorGUILayout.HelpBox("There are no units to move in formation yet. Either drag & drop them in the list below or use Helper above to add them.", MessageType.Warning);
            }

            EditorGUILayout.LabelField("Units in formation", boldStyle);
            EditorGUILayout.PropertyField(units);
        }

        private void ApplyFormationButtonGUI()
        {
            if (units.arraySize > 0)
            {
                if (showGizmos.boolValue)
                {
                    EditorGUILayout.BeginHorizontal();
                    EditorGUILayout.Space();
                    if (GUILayout.Button("Apply Formation"))
                        controller.ApplyCurrentUnitFormation(false);
                    EditorGUILayout.Space();
                    EditorGUILayout.EndHorizontal();
                }
                else
                {
                    EditorGUILayout.HelpBox("When gizmos are disabled, units are updated every frame.", MessageType.Info);
                }

                EditorGUILayout.Space();
                EditorGUILayout.Space();
            }
        }

        private void BeginSection(string title)
        {
            GUILayout.Label(title, EditorStyles.boldLabel);
            EditorGUI.indentLevel++;
        }

        private void EndSection() => EditorGUI.indentLevel--;

    }
}
