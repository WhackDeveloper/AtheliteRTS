using System.Collections;
using System.Collections.Generic;
using TRavljen.UnitFormation.Formations;
using TRavljen.UnitFormation.Placement;
using UnityEngine;

namespace TRavljen.UnitFormation
{

    /// <summary>
    /// Component responsible for managing units formations targets. This is done
    /// through <see cref="IFormationUnit"/> interface which allows any type
    /// of movement control to the destination target of each unit within a formation.
    /// </summary>
    public class UnitFormation : MonoBehaviour
    {

        [Tooltip("Specifies if the formation positions should find valid NavMesh " +
            "area position when updating units target positions.")]
        [SerializeField]
        private bool placeOnGround = false;

        [Tooltip("Specifies max distance to check for valid ground from original formation position.")]
        [SerializeField, Range(1, 5_000)]
        private float maxGroundDistance = 20;

        [Tooltip("Specifies if a random noise is applied ontop of formation positions.")]
        [SerializeField]
        private bool noiseEnabled = false;

        [Tooltip("Noise applied to the unit formation.")]
        [Range(0.01f, 100f)]
        [SerializeField]
        private float noiseScale = 0.2f;

        [Tooltip("Specifies a list of units to place in formation")]
        [SerializeField]
        private List<Transform> units = new List<Transform>();

        public IGroundPositioner GroundPositioner = new NavMeshGroundPositioner();

        /// <summary>
        /// Returns true if there is more than 1 unit present.
        /// </summary>
        public bool HasUnits => units.Count > 0;

        /// <summary>
        /// Specifies current calculated positions for the unit formation.
        /// </summary>
        public UnitFormationData FormationPositions { get; private set; }

        /// <summary>
        /// Returns current formation definition.
        /// </summary>
        public IFormation CurrentFormation => currentFormation;

        /// <summary>
        /// List of units used for placing in formation.
        /// </summary>
        public List<Transform> Units => units;

        /// <summary>
        /// Specifies if a random noise is appleid ontop of formation positions.
        /// </summary>
        public bool NoiseEnabled
        {
            get => noiseEnabled;
            set => noiseEnabled = value;
        }

        private IFormation currentFormation = new RectangleFormation(8, 2);

        #region Public Interface

        /// <summary>
        /// Set new formation definition
        /// </summary>
        public void SetUnitFormation(IFormation formation)
        {
            currentFormation = formation;
        }

        /// <summary>
        /// Applies formation data to current units.
        /// <remarks>Make sure the unit count matches!</remarks>
        /// </summary>
        /// <param name="formationData">New formation data</param>
        public void ApplyCurrentUnitFormation(UnitFormationData formationData)
        {
            FormationPositions = ModifyPositions(formationData);
            UpdateUnitsTargetPosition();
        }

        /// <summary>
        /// Calculates and applies new formation positions based on new position and direction.
        /// </summary>
        /// <param name="position">New position</param>
        /// <param name="direction">New direction</param>
        public void ApplyCurrentUnitFormation(Vector3 position, Vector3 direction)
        {
            FormationPositions = ModifyPositions(CalculatePositions(position, direction));
            UpdateUnitsTargetPosition();
        }

        /// <summary>
        /// Calculates and applies new formation positions based on new position.
        /// This method calculates direction from units group center to the
        /// new position of the formation.
        /// </summary>
        /// <param name="position">New position</param>
        public void ApplyCurrentUnitFormation(Vector3 position)
        {
            // Calculate direction angle based on current positions
            var currentPositions = units.ConvertAll(obj => obj.transform.position);

            var formation = FormationPositioner.GetPositions(
                currentPositions, currentFormation, position);
            FormationPositions = ModifyPositions(formation);

            UpdateUnitsTargetPosition();
        }

        /// <summary>
        /// Update units positions with the current formation data.
        /// </summary>
        private void UpdateUnitsTargetPosition()
        {
            for (int index = 0; index < units.Count; index++)
            {
                Vector3 pos = FormationPositions.UnitPositions[index];

                if (units[index].TryGetComponent(out IFormationUnit unit))
                {
                    unit.SetTargetDestination(pos, FormationPositions.FacingAngle);
                }
            }
        }

        /// <summary>
        /// Calculate new unit formation group positions.
        /// </summary>
        /// <param name="position">Formation position</param>
        /// <param name="direction">Formation direction</param>
        /// <returns>Returns calculated formation data for this unit formation group.</returns>
        public UnitFormationData CalculatePositions(Vector3 position, Vector3 direction)
        {
            return FormationPositioner.GetAlignedFormation(
                units.Count, currentFormation, position, direction);
        }

        /// <summary>
        /// Finds the nearest valid ground and returns it.
        /// If there was no valid positions, then value returned is unchanged.
        /// It uses <see cref="GroundPositioner"/> to calculate the position.
        /// </summary>
        /// <param name="position">Returns new position if tere is a valid one within <see cref="maxGroundDistance"/></param>
        public Vector3 MovePositionOnGround(Vector3 position)
            => GroundPositioner.PositionOnGround(position, maxGroundDistance);

        #endregion

        /// <summary>
        /// Modifies unit formation positions by applying noise and valid ground
        /// if any of those features are enabled.
        /// </summary>
        private UnitFormationData ModifyPositions(UnitFormationData formation)
        {
            // Check if iteration makes sense.
            if (!noiseEnabled && !placeOnGround)
                return formation;

            for (int index = 0; index < formation.UnitPositions.Count; index++)
            {
                Vector3 pos = formation.UnitPositions[index];

                if (noiseEnabled)
                {
                    pos += UnitFormationHelper.GetNoise(noiseScale);
                }

                if (placeOnGround)
                    pos = MovePositionOnGround(pos);

                formation.UnitPositions[index] = pos;
            }

            return formation;
        }
    }

}