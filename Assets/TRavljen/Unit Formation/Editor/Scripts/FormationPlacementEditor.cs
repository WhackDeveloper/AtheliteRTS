using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TRavljen.UnitFormation.Editor
{
    using TRavljen.EditorUtility;
    using UnityEditor;
    using TRavljen.UnitFormation.Placement;

    [CustomEditor(typeof(FormationPlacement))]
    public class FormationPlacementEditor : Editor
    {
        SerializedProperty groundLayerMask;
        SerializedProperty raycastMaxDistance;
        SerializedProperty customInput;
        SerializedProperty unitFormation;
        SerializedProperty alwaysCalculatePositions;
        SerializedProperty calculatePositionsIntervalThreshold;
        SerializedProperty placeVisualsOnGround;
        SerializedProperty placementVisuals;

        SerializedProperty distanceCheck;
        SerializedProperty distanceThreshold;
        SerializedProperty minPlacementDuration;
        SerializedProperty startDelay;

        private void OnEnable()
        {
            groundLayerMask = serializedObject.FindProperty("groundLayerMask");
            raycastMaxDistance = serializedObject.FindProperty("raycastMaxDistance");
            customInput = serializedObject.FindProperty("customInput");
            unitFormation = serializedObject.FindProperty("unitFormation");
            alwaysCalculatePositions = serializedObject.FindProperty("alwaysCalculatePositions");
            calculatePositionsIntervalThreshold = serializedObject.FindProperty("calculatePositionsIntervalThreshold");
            placeVisualsOnGround = serializedObject.FindProperty("placeVisualsOnGround");
            placementVisuals = serializedObject.FindProperty("placementVisuals");

            distanceCheck = serializedObject.FindProperty("distanceCheck");
            distanceThreshold = serializedObject.FindProperty("distanceThreshold");
            minPlacementDuration = serializedObject.FindProperty("minPlacementDuration");
            startDelay = serializedObject.FindProperty("startDelay");
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            GeneralSectionGUI();
            VisualsSectionGUI();

            LabelAndIndent("Focused Unit Formation Group");
            EditorGUILayout.PropertyField(unitFormation);
            EditorGUI.indentLevel--;

            serializedObject.ApplyModifiedProperties();
        }

        private void VisualsSectionGUI()
        {
            if (!PersistentFoldout.Foldout("Visuals")) return;

            EditorGUI.indentLevel++;
            EditorGUILayout.PropertyField(alwaysCalculatePositions);

            if (alwaysCalculatePositions.boolValue)
            {
                EditorGUI.indentLevel++;
                EditorGUILayout.PropertyField(calculatePositionsIntervalThreshold);
                EditorGUILayout.PropertyField(placeVisualsOnGround);
                EditorGUI.indentLevel--;
            }

            EditorGUILayout.PropertyField(placementVisuals);
            EditorGUI.indentLevel--;
        }

        private void GeneralSectionGUI()
        {
            if (!PersistentFoldout.Foldout("General")) return;

            EditorGUI.indentLevel++;

            EditorGUILayout.PropertyField(groundLayerMask);
            EditorGUILayout.PropertyField(raycastMaxDistance);

            EditorGUILayout.PropertyField(distanceCheck);
            EditorGUILayout.PropertyField(distanceThreshold);
            EditorGUILayout.PropertyField(minPlacementDuration);
            EditorGUILayout.PropertyField(startDelay);

            InputSetupGUI();

            EditorGUI.indentLevel--;
        }

        private void InputSetupGUI()
        {
            LabelAndIndent("Input");

            var placement = target as FormationPlacement;
            if (IsCustomInputNotSet() && placement.GetComponent<AInputControl>() == null)
            {
                EditorGUILayout.Space();
                if (customInput.objectReferenceValue != null)
                {
                    EditorGUILayout.HelpBox("There is no component which implements 'IInputControl' or subclasses 'AInputControl' on target reference", MessageType.Warning);
                }
                else
                {
                    EditorGUILayout.HelpBox("You can set reference to the game object that contains a component which implements IInputControl." +
                        "\nOr add default input based on your supported input system.", MessageType.Info);
                }
                EditorGUILayout.Space();

                GUILayout.BeginHorizontal();

                EditorGUILayout.PropertyField(customInput);

                GUILayout.EndHorizontal();

                GUILayout.BeginHorizontal();

                GUILayout.Label("OR Add provided input component");

                if (GUILayout.Button("Add"))
                {
                    placement.AddDefaultInput();
                }

                GUILayout.EndHorizontal();
            }
            else
            {
                GUILayout.BeginHorizontal();
                GUILayout.Label("Your Input is set up.", EditorStyles.centeredGreyMiniLabel);

                if (GUILayout.Button("Remove input"))
                {
                    if (customInput.objectReferenceValue != null)
                    {
                        customInput.objectReferenceValue = null;
                    }

                    if (placement.TryGetComponent(out AInputControl control))
                    {
                        DestroyImmediate(control);
                    }
                }

                GUILayout.EndHorizontal();
            }

            EditorGUILayout.Space();
            EditorGUI.indentLevel--;
        }

        private bool IsCustomInputNotSet()
        {
            // First check null reference
            if (customInput.objectReferenceValue == null)
                return true;

            // Then if it contains valid interface
            return (customInput.objectReferenceValue as GameObject).GetComponent<IInputControl>() == null;
        }

        void LabelAndIndent(string text)
        {
            EditorGUILayout.LabelField(text, EditorStyles.boldLabel);
            EditorGUI.indentLevel++;
        }
    }
}