using System;
using UnityEditor;
using UnityEngine;
using UnityEngine.Events;

namespace TRavljen.UnitSelection
{

    /// <summary>
    /// Simple convenience implementation of <see cref="ISelectable"/> interface.
    /// This class can also be derived and override <see cref="SelectionStateChanged"/>
    /// method in order to update visuals, base implementation also uses optional
    /// reference to the <see cref="selectionIndicator"/> to notify for change.
    /// </summary>
    [DisallowMultipleComponent]
    public class SelectableUnit : MonoBehaviour, ISelectable
    {

        enum IndicatorPriority { Selected, Highlighted }

        [SerializeField]
        [Tooltip("Optional reference to the selection indicator.")]
        protected ASelectionIndicator selectionIndicator;

        [SerializeField]
        [Tooltip("Specify priority of selection indicator. If both highlight and " +
            "selected flags are true, then one must take precedence over the other")]
        private IndicatorPriority indicatorPriority = IndicatorPriority.Highlighted;

        /// <summary>
        /// Specifies if the unit is currently selected.
        /// </summary>
        public bool IsSelected { get; protected set; }

        /// <summary>
        /// Specifies if the unit is currently highlighted.
        /// </summary>
        public bool IsHighlighted { get; protected set; }

        /// <summary>
        /// Event invoked when either <see cref="IsSelected"/> or
        /// <see cref="IsHighlighted"/> flag values change.
        /// </summary>
        [Tooltip("Event invoked when either selection or highlight state changes.")]
        public UnityEvent OnSelectionStateChange;

        [Tooltip("Event invoked when selection state has changed.")]
        public UnityEvent<bool> OnSelectionChanged = new();

        [Tooltip("Event invoked when highlight state has changed.")]
        public UnityEvent<bool> OnHighlightChanged = new();

        protected virtual void OnEnable() => UpdateSelectionIndicator();

        protected void OnValidate()
        {
            if (selectionIndicator == null)
                selectionIndicator = GetComponentInChildren<ASelectionIndicator>();
        }

        public virtual void Select()
        {
            if (IsSelected) return;

            SetSeleted(true);
        }

        public virtual void Deselect()
        {
            if (!IsSelected) return;

            SetSeleted(false);
        }

        public virtual void Highlight()
        {
            if (IsHighlighted) return;

            SetHighlight(true);
        }

        public virtual void Unhighlight()
        {
            if (!IsHighlighted) return;

            SetHighlight(false);
        }

        protected virtual void SetHighlight(bool highlight)
        {
            IsHighlighted = highlight;
            OnHighlightChanged.Invoke(highlight);
            SelectionStateChanged();
        }

        protected virtual void SetSeleted(bool select)
        {
            IsSelected = select;
            OnSelectionChanged.Invoke(select);
            SelectionStateChanged();
        }

        /// <summary>
        /// When either <see cref="IsHighlighted"/> or <see cref="IsSelected"/>
        /// state changes, this method is invoked so that unit can update
        /// any other states or visuals that it desires.
        /// </summary>
        public virtual void SelectionStateChanged()
        {
            UpdateSelectionIndicator();
            OnSelectionStateChange?.Invoke();
        }

        /// <summary>
        /// Updates visuals of the <see cref="selectionIndicator"/>.
        /// </summary>
        public virtual void UpdateSelectionIndicator()
        {
            if (selectionIndicator == null) return;

            switch (indicatorPriority)
            {
                case IndicatorPriority.Selected:
                    if (IsSelected)
                        selectionIndicator.Select();
                    else if (IsHighlighted)
                        selectionIndicator.Highlight();
                    else
                        selectionIndicator.Clear();

                    break;

                case IndicatorPriority.Highlighted:
                    if (IsHighlighted)
                        selectionIndicator.Highlight();
                    else if (IsSelected)
                        selectionIndicator.Select();
                    else
                        selectionIndicator.Clear();

                    break;
            }
        }

    }

}