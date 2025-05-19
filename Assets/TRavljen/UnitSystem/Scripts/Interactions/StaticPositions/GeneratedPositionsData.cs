using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TRavljen.UnitSystem.Interactions.PredefinedPositions
{

    /// <summary>
    /// Encapsulates stored data for generated positions and reserved positions.
    /// This class manages the generation of positions within a specified range,
    /// keeps track of which positions are reserved by units, and provides methods
    /// to retrieve available interaction positions for units.
    /// </summary>
    sealed class GeneratedPositionsData
    {

        // Taken positions, that cannot be used by other units.
        private readonly Dictionary<int, IUnitInteractorComponent[]> reservedPositions;

        // Already generated positions, store for convenience and reducing CPU load.
        private readonly Dictionary<int, Vector3[]> generatedPositions;

        private readonly IPositionGenerator generator;

        /// <summary>
        /// Initializes a new instance of the <see cref="GeneratedPositionsData"/> class.
        /// </summary>
        /// <param name="generator">An instance of <see cref="IPositionGenerator"/> used for generating positions.</param>
        /// <exception cref="ArgumentNullException">Thrown when the generator is null.</exception>
        public GeneratedPositionsData(IPositionGenerator generator)
        {
            this.generator = generator ?? throw new System.ArgumentNullException("Generator must not be null!");
            reservedPositions = new();
            generatedPositions = new();
        }

        /// <summary>
        /// Retrieves an available interaction position for the specified <see cref="IUnitInteractorComponent"/>.
        /// </summary>
        /// <param name="interactor">The unit requesting the interaction position.</param>
        /// <param name="interactee">The target interactee to interact with.</param>
        /// <param name="reserve">Whether to reserve the position for the interactor.</param>
        /// <returns>A world-space <see cref="Vector3"/> representing the available interaction position.</returns>
        public Vector3 GetAvailableInteractionPosition(IUnitInteractorComponent interactor, IUnitInteracteeComponent interactee, bool reserve)
        {
            Transform targetTransform = interactee.transform;

            int key = DistanceToKey(interactor.MaxInteractionRange);

            if (!generatedPositions.TryGetValue(key, out Vector3[] positions))
            {
                generator.TryGeneratePositionsInRange(interactor.MaxInteractionRange, out positions);
                generatedPositions.Add(key, positions);
            }

            if (positions.Length == 0)
            {
                // No positions were generated.
                return targetTransform.position;
            }

            Vector3 local = targetTransform.InverseTransformPoint(interactor.Position);

            // Do this with units position in local space of the target
            int index = Utility.Vector3Distance.GetClosestPointIndex(local, positions);

            if (!reservedPositions.TryGetValue(key, out IUnitInteractorComponent[] interactors))
            {
                interactors = new IUnitInteractorComponent[positions.Length];

                // Add only if reserve is enabled.
                if (reserve)
                    reservedPositions.Add(key, interactors);
            }
            // Cleanup only if reserve was enabled, otherwise leave it in, it might just be
            // looking for better positions.
            else if (reserve)
            {
                // Remove previous entry if one is present already.
                for (int _index = 0; _index < interactors.Length; _index++)
                {
                    if (interactors[_index] == interactor)
                    {
                        interactors[_index] = null;
                        break;
                    }
                }
            }

            Vector3 position;
            int closestIndex = FindClosestAvailableIndex(index, positions.Length, interactors);

            if (closestIndex != -1)
            {
                position = positions[closestIndex];

                // Save unit
                interactors[closestIndex] = interactor;
            }
            else
            {
                position = positions[index];
            }

            // Manually calculate world position, scale is already applied in the position.
            // Scaling is applied to achieve proper spacing between positions.
            return targetTransform.position + targetTransform.rotation * position;
        }

        /// <summary>
        /// Releases the interaction position reserved for the specified <see cref="IUnitInteractorComponent"/>.
        /// </summary>
        /// <param name="interactor">The unit for which the interaction position should be released.</param>
        /// <param name="interactee">The target interactee associated with the position.</param>
        /// <returns><c>true</c> if the interaction position was successfully released; otherwise, <c>false</c>.</returns>
        public bool ReleaseInteractionPosition(IUnitInteractorComponent interactor)
        {
            bool cleared = false;

            foreach (var (key, units) in reservedPositions)
            {
                for (int index = 0; index < units.Length; index++)
                {
                    if (units[index] == interactor)
                    {
                        units[index] = null;
                        cleared = true;
                    }
                }
            }

            return cleared;
        }

        #region Debug

        /// <summary>
        /// Draws gizmos to visualize the interaction positions in the scene.
        /// By default, these are rendered when the GameObject holding this script is selected in the Editor.
        /// </summary>
        /// <param name="transform">The transform of the object to visualize.</param>
        /// <param name="showGeneratedPositions">If true, all generated positions will be displayed.</param>
        public void DrawGizmos(Transform transform, bool showGeneratedPositions)
        {
            Color fullPositionColor = Color.red;
            Color gizmosColor = Color.white;

            float gizmoSphereSize = 0.3f;

            Transform targetTransform = transform;

            // Render reserved positions
            foreach (var (rangeIndex, takenPositions) in reservedPositions)
            {
                if (!generatedPositions.TryGetValue(rangeIndex, out Vector3[] generatedRing))
                    continue;

                for (int index = 0; index < generatedRing.Length; index++)
                {
                    Vector3 localPos = generatedRing[index];

                    bool isTaken = takenPositions[index] != null;

                    // Taken are always rendered, regardless of the flag passed
                    if (!isTaken && !showGeneratedPositions) continue;

                    Gizmos.color = isTaken ? fullPositionColor : gizmosColor;
                    Vector3 worldPos = targetTransform.TransformPoint(localPos);
                    Gizmos.DrawSphere(worldPos, gizmoSphereSize);
                }
            }
        }

        #endregion

        #region Utility

        /// <summary>
        /// Converts a float distance to an integer key for internal storage.
        /// </summary>
        /// <param name="distance">The distance to convert.</param>
        /// <returns>An integer key representing the distance.</returns>
        private int DistanceToKey(float distance) => (int)(distance * 100);

        /// <summary>
        /// Finds the closest available position index in a circular array based on reserved units.
        /// </summary>
        /// <param name="currentIndex">The index of the current position.</param>
        /// <param name="length">The total number of positions.</param>
        /// <param name="positionedUnits">The array of reserved units.</param>
        /// <returns>The index of the closest available position; -1 if none are available.</returns>
        private static int FindClosestAvailableIndex(int currentIndex, int length, IUnitInteractorComponent[] positionedUnits)
        {
            // Check the current index first
            if (positionedUnits[currentIndex] == null)
            {
                return currentIndex;
            }

            int offset = 1;
            while (offset <= length)
            {
                int lowerIndex = currentIndex - offset;
                if (lowerIndex < 0)
                    lowerIndex += length;

                if (positionedUnits[lowerIndex] == null)
                {
                    return lowerIndex;
                }

                int upperIndex = currentIndex + offset;
                if (upperIndex >= length)
                    upperIndex -= length;

                if (positionedUnits[upperIndex] == null)
                {
                    return upperIndex;
                }

                offset++;
            }

            // If no available index is found within bounds
            return -1;
        }

        #endregion
    }
}