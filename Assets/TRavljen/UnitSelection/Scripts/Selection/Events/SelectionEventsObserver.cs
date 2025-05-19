using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace TRavljen.UnitSelection
{

    /// <summary>
    /// Monobehaviour component designed for easy hook up on the selection events.
    /// This can be done in Editor itself or during runtime in code.
    /// </summary>
    public class SelectionEventsObserver : MonoBehaviour
    {

        [Tooltip("Event invoked when list of selections has changed.")]
        public UnityEvent<List<ISelectable>> OnSelectionChange = new();

        [Tooltip("Event invoked when list of highlights has changed.")]
        public UnityEvent<List<ISelectable>> OnHighlightChange = new();

        [Tooltip("Event invoked when hovering selectable has changed.\nEither it was set, cleared or updated.")]
        public UnityEvent<ISelectable> OnUnitHoverChange;

        private readonly SelectionEvents events = SelectionEvents.Instance;

        private void OnEnable()
        {
            events.OnSelectionChange.AddListener(OnSelectionChange.Invoke);
            events.OnHighlightChange.AddListener(OnHighlightChange.Invoke);
            events.OnUnitHoverChange.AddListener(OnUnitHoverChange.Invoke);
        }

        private void OnDisable()
        {
            events.OnSelectionChange.RemoveListener(OnSelectionChange.Invoke);
            events.OnHighlightChange.RemoveListener(OnHighlightChange.Invoke);
            events.OnUnitHoverChange.RemoveListener(OnUnitHoverChange.Invoke);
        }
    }

}