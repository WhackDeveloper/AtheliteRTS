using UnityEngine;
using UnityEngine.Events;

namespace TRavljen.UnitFormation
{

    /// <summary>
    /// Convenience abstraction for <see cref="MonoBehaviour"/> component
    /// implmenting <see cref="IInputControl"/>.
    /// Two default implementations exist for old and new input systems,
    /// see <see cref="ActionInputControl"/> and <see cref="KeyInputControl"/>.
    /// </summary>
    public abstract class AInputControl : MonoBehaviour, IInputControl
    {
        public UnityEvent OnPlacementActionPress { get; set; } = new UnityEvent();
        public UnityEvent OnPlacementActionRelease { get; set; } = new UnityEvent();
        public UnityEvent OnPlacementActionCancel { get; set; } = new UnityEvent();

        public abstract Vector3 MousePosition { get; }
    }

}