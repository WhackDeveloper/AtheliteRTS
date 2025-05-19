using UnityEngine;

namespace TRavljen.UnitSystem.Interactions.PredefinedPositions
{

    /// <summary>
    /// Provides predefined interaction positions for interactors engaging with an interactee.
    /// This class utilizes a predefined position generator and manages interaction positions
    /// dynamically.
    /// </summary>
    [DisallowMultipleComponent]
    public class StaticInteractionPositionProvider : MonoBehaviour, IPredefinedInteractionPositionProvider
    {

        [Tooltip("The generator responsible for creating predefined positions for interactions.")]
        [SerializeField]
        private PredefinedPositionGenerator generator;

        /// <summary>
        /// Stores generated positions and manages availability for interactions.
        /// </summary>
        private GeneratedPositionsData data;

        private void Awake()
        {
            if (generator == null || generator.transform == null)
                generator = new(transform);

            data ??= new(generator);
        }

        /// <summary>
        /// Ensures that the generator and position data are initialized when values are modified in the editor.
        /// </summary>
        private void OnValidate()
        {
            if (generator == null || generator.transform == null)
                generator = new(transform);

            data ??= new(generator);
        }

        /// <inheritdoc/>
        Vector3 IPredefinedInteractionPositionProvider.GetAvailableInteractionPosition(IUnitInteractorComponent interactor, IUnitInteracteeComponent interactee, bool reserve)
        {
            return data.GetAvailableInteractionPosition(interactor, interactee, reserve);
        }

        /// <inheritdoc/>
        bool IPredefinedInteractionPositionProvider.ReleaseInteractionPosition(IUnitInteractorComponent interactor, IUnitInteracteeComponent interactee)
        {
            return data.ReleaseInteractionPosition(interactor);
        }

        #region Debug

        private void OnDrawGizmosSelected()
        {
            generator.DrawGizmos();
            data.DrawGizmos(transform, true);
        }

        #endregion

    }

}
