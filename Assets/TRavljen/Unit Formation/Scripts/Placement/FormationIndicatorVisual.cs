using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

namespace TRavljen.UnitFormation.Placement
{

    /// <summary>
    /// Component for showing formation positions during active placement.
    /// This can be used with <see cref="FormationPlacement.placementVisuals"/>.
    /// Specify the game object used for unit position in formation and component
    /// will manage the visuals for their formation.
    /// <see cref="FormationPlacement.alwaysCalculatePositions"/> must be enabled
    /// in order to get information about formation positions while placement is active.
    /// </summary>
    public class FormationIndicatorVisual : APlacementVisuals
    {

        #region Properties

        [Tooltip("Specifies prefab object used for indicating unit's position within a formation during active placement. " +
            "One will be instantiated for each unit position.")]
        [SerializeField]
        private GameObject unitIndicatorPrefab;

        [Tooltip("Specifies if the unit formation indicators are hidden with delay.")]
        [SerializeField]
        private bool hideWithDelay = false;

        [Tooltip("Specifies the delay for hiding indicators.")]
        [SerializeField, Range(0.1f, 20f)]
        private float hideDelay = 2.5f;

        /// <summary>
        /// Temporary container for indicator visuals. Simplifies the destruction.
        /// </summary>
        private Transform indicatorsContainer;

        /// <summary>
        /// List of temporary indicators for each unit.
        /// </summary>
        private List<Transform> unitPlacementIndicators = new List<Transform>();

        #endregion

        #region APlacementVisuals

        public override void StartPlacement(Vector3 start)
        {
            base.StartPlacement(start);

            // Make sure to clear it if it was delayed
            StopAllCoroutines();
            DestroyIndicators();
        }

        public override void StopPlacement()
        {
            base.StopPlacement();

            // Hide with delay if it is enabled
            if (hideWithDelay)
            {
                StartCoroutine(DestroyIndicatorsWithDelay(hideDelay));
            }
            else
            {
                // Destroy immediately
                DestroyIndicators();
            }
        }

        public override void OnFormationReady(UnitFormationData formation)
        {
            base.OnFormationReady(formation);

            if (indicatorsContainer == null)
                InstantiateIndicators(formation);
            else
            {
                // Destroy some if there are too many.
                for (int index = unitPlacementIndicators.Count-1; index >= formation.UnitPositions.Count; index--)
                {
                    Destroy(unitPlacementIndicators[index].gameObject);
                    unitPlacementIndicators.RemoveAt(index);
                }

                var rotation = Quaternion.Euler(formation.FacingEuler);
                for (int index = 0; index < formation.UnitPositions.Count; index++)
                {
                    // Create one if needed
                    if (index >= unitPlacementIndicators.Count)
                    {
                        AddNewIndicator(formation.UnitPositions[index], rotation);
                    }
                    // Otherwise update existing
                    else
                    {
                        unitPlacementIndicators[index]
                            .SetPositionAndRotation(formation.UnitPositions[index], rotation);
                    }
                }
            }
        }

        #endregion

        #region Convenience

        private void InstantiateIndicators(UnitFormationData formation)
        {
            indicatorsContainer = new GameObject("Indicators Container").transform;

            // Populate the indicator array with new indicators
            var rotation = Quaternion.Euler(formation.FacingEuler);
            for (int index = 0; index < formation.UnitPositions.Count; index++)
            {
                AddNewIndicator(formation.UnitPositions[index], rotation);
            }
        }

        private void AddNewIndicator(Vector3 position, Quaternion rotation)
        {
            GameObject newIndicator = Instantiate(
                unitIndicatorPrefab, position, rotation, indicatorsContainer);

            unitPlacementIndicators.Add(newIndicator.transform);
        }

        private void DestroyIndicators()
        {
            if (indicatorsContainer != null)
            {
                Destroy(indicatorsContainer.gameObject);
                indicatorsContainer = null;
            }

            unitPlacementIndicators.Clear();
        }

        private IEnumerator DestroyIndicatorsWithDelay(float delay)
        {
            yield return new WaitForSeconds(delay);
            DestroyIndicators();
        }

        #endregion
    }

}