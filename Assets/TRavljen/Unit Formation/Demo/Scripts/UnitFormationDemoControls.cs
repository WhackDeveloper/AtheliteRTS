using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace TRavljen.UnitFormation.Demo
{
    using Formations;
    using Placement;

    public class UnitFormationDemoControls: MonoBehaviour
    {

        #region Inspector Properties

        [SerializeField] private Toggle noiseToggle;
        [SerializeField] private Toggle pivotInCenterToggle;
        [SerializeField] private Slider unitCountSlider;
        [SerializeField] private Slider unitSpacingSlider;
        [SerializeField] private Slider rectangleColumnCountSlider;
        [SerializeField] private Text unitCountText;
        [SerializeField] private Text unitSpacingText;
        [SerializeField] private Text rectangleColumnCountText;

        [Tooltip("Specifies a unit prefab used for instantiating when unit " +
            "count slider is increased.")]
        [SerializeField] private GameObject unitPrefab = null;

        [Tooltip("Specifies the formation placement component.")]
        [SerializeField] private FormationPlacement formationPlacement;

        #endregion

        #region Private Properties

        private UnitFormation unitFormation;

        private bool PivotInCenter => pivotInCenterToggle.isOn;
        private int UnitCount => (int)unitCountSlider.value;
        private int RectangleColumnCount => (int)rectangleColumnCountSlider.value;
        private float UnitSpacing => unitSpacingSlider.value;

        #endregion

        private void Start()
        {
            unitFormation = formationPlacement.UnitFormation;
            var formation = new LineFormation(UnitSpacing);
            formationPlacement.SetFormation(formation);

            // Initial UI update
            UpdateUnitCountText(UnitCount);
            UpdateUnitSpacing(UnitSpacing);
            UpdateRectangleColumnCountText(RectangleColumnCount);
        }

        private void OnEnable()
        {
            noiseToggle.onValueChanged.AddListener(OnNoiseToggleChanged);
            unitCountSlider.onValueChanged.AddListener(UpdateUnitCountText);
            unitSpacingSlider.onValueChanged.AddListener(UpdateUnitSpacing);
            pivotInCenterToggle.onValueChanged.AddListener(OnPivotToggleChanged);
            rectangleColumnCountSlider.onValueChanged.AddListener(UpdateRectangleColumnCountText);
        }

        private void OnDisable()
        {
            noiseToggle.onValueChanged.RemoveListener(OnNoiseToggleChanged);
            unitCountSlider.onValueChanged.RemoveListener(UpdateUnitCountText);
            unitSpacingSlider.onValueChanged.RemoveListener(UpdateUnitSpacing);
            pivotInCenterToggle.onValueChanged.RemoveListener(OnPivotToggleChanged);
            rectangleColumnCountSlider.onValueChanged.RemoveListener(UpdateRectangleColumnCountText);
        }

        private void Update()
        {
            // Add or remove units with keyboard
            if (Input.GetKeyDown(KeyCode.Plus))
                unitCountSlider.value += 1;
            else if (Input.GetKeyDown(KeyCode.Minus))
                unitCountSlider.value -= 1;

            AddOrRemoveUnitIfNeeded();
        }

        private void AddOrRemoveUnitIfNeeded()
        {
            var units = unitFormation.Units;
            if (units.Count < UnitCount)
            {
                for (int index = units.Count; index < UnitCount; index++)
                {
                    var gameObject = Instantiate(
                        unitPrefab, Vector3.zero, Quaternion.identity);
                    units.Insert(index, gameObject.transform);
                }

                formationPlacement.ApplyCurrentUnitFormation();
            }
            else if (units.Count > UnitCount)
            {
                // When removing and destroying a unit during selection it is very important to update formations right away.
                for (int index = units.Count - 1; index >= UnitCount; index--)
                {
                    var transform = units[index];
                    units.RemoveAt(index);
                    Destroy(transform.gameObject);
                }

                formationPlacement.ApplyCurrentUnitFormation();
            }
        }

        #region User Interactions

        public void LineFormationSelected() =>
            formationPlacement.SetFormation(new LineFormation(UnitSpacing));

        public void CircleFormationSelected() =>
            formationPlacement.SetFormation(new RingFormation(UnitSpacing));

        public void TriangleFormationSelected() =>
            formationPlacement.SetFormation(new TriangleFormation(UnitSpacing));

        public void ConeFormationSelected() =>
            formationPlacement.SetFormation(new ConeFormation(UnitSpacing, PivotInCenter));

        public void RectangleFormationSelected() =>
            formationPlacement.SetFormation(new RectangleFormation(RectangleColumnCount, UnitSpacing, true, PivotInCenter));

        private void UpdateRectangleColumnCountText(float _)
        {
            rectangleColumnCountText.text = "Units per ROW: " + RectangleColumnCount;

            if (unitFormation.CurrentFormation is RectangleFormation)
            {
                UpdateFormation();
            }
        }

        private void OnNoiseToggleChanged(bool newState)
        {
            unitFormation.NoiseEnabled = newState;
            formationPlacement.ApplyCurrentUnitFormation();
        }

        private void OnPivotToggleChanged(bool _)
        {
            UpdateFormation();
        }

        private void UpdateUnitCountText(float value)
        {
            unitCountText.text = "Unit Count: " + value;
        }

        private void UpdateUnitSpacing(float spacing)
        {
            unitSpacingText.text = $"Unit Spacing: {spacing:0.00}";

            UpdateFormation();
        }

        /// <summary>
        /// Instantiates a new formation based on the current type with the new
        /// configurations applied from UI.
        /// </summary>
        private void UpdateFormation()
        {
            var currentFormation = unitFormation.CurrentFormation;

            if (currentFormation is LineFormation)
            {
                currentFormation = new LineFormation(UnitSpacing);
            }
            else if (currentFormation is RectangleFormation rectangleFormation)
            {
                currentFormation = new RectangleFormation(
                    RectangleColumnCount, UnitSpacing, true, PivotInCenter);
            }
            else if (currentFormation is RingFormation)
            {
                currentFormation = new RingFormation(UnitSpacing);
            }
            else if (currentFormation is TriangleFormation)
            {
                currentFormation = new TriangleFormation(UnitSpacing, pivotInCenter: PivotInCenter);
            }
            else if (currentFormation is ConeFormation)
            {
                currentFormation = new ConeFormation(UnitSpacing, PivotInCenter);
            }

            formationPlacement.SetFormation(currentFormation);
        }

        #endregion

    }

}
