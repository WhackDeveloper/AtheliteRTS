using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TRavljen.PlacementSystem
{
    /// <summary>
    /// Responder class for invoking methods on <see cref="ObjectPlacement"/>
    /// based on <see cref="AInputControl"/> component events which must be
    /// attached to the same game object.
    /// </summary>
    internal sealed class InputControlResponder: MonoBehaviour
    {

        private ObjectPlacement placement;

        private void Awake()
        {
            placement = transform.parent.GetComponent<ObjectPlacement>();
        }

        private void OnEnable() => AddListeners();
        private void OnDisable() => RemoveListeners();

        private void AddListeners()
        {
            RemoveListeners();

            if (TryGetComponent(out AInputControl inputControl))
            {
                inputControl.OnPlacementActiveToggle.AddListener(ToggleActivePlacement);
                inputControl.OnPlacementCancel.AddListener(CancelPlacement);
                inputControl.OnPlacementFinish.AddListener(EndPlacement);
                inputControl.OnRotate.AddListener(RotatePlacement);
            }
        }

        private void RemoveListeners()
        {
            if (TryGetComponent(out AInputControl inputControl))
            {
                inputControl.OnPlacementActiveToggle.RemoveListener(ToggleActivePlacement);
                inputControl.OnPlacementCancel.RemoveListener(CancelPlacement);
                inputControl.OnPlacementFinish.RemoveListener(EndPlacement);
                inputControl.OnRotate.RemoveListener(RotatePlacement);
            }
        }

        private void ToggleActivePlacement()
        {
            if (!placement.isActiveAndEnabled) return;

            if (placement.IsPlacementActive)
                placement.CancelPlacement();
            else
                placement.BeginPlacement();
        }

        private void CancelPlacement() => placement.CancelPlacement();

        private void EndPlacement() => placement.TryEndPlacement(out GameObject _);

        private void RotatePlacement(float directionalMagnitude)
        {
            placement.RotatePlacement(directionalMagnitude);
        }
    }

}