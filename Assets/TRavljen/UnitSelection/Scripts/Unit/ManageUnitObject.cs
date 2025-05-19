using UnityEngine;

namespace TRavljen.UnitSelection
{

    /// <summary>
    /// This component is part of drag selection feature and can be added
    /// through <see cref="SelectableUnit"/> component or manually.
    /// It notifies the <see cref="UnitManager"/> about any changes in the
    /// unit's state like when they are instantiated, enabled, disabled,
    /// unloaded or otherwise destroyed.
    /// </summary>
    public class ManageUnitObject : MonoBehaviour
    {

        private void OnEnable()
        {
            if (TryGetComponent(out ISelectable selectable))
                UnitManager.GetOrCreate().AddUnit(selectable);
            else
                ThrowException();
        }

        private void OnDisable()
        {
            // Check if instance is valid, if its in destruction along with the unit
            // as we cannot remove it as it will throw an exception.
            if (UnitManager.Instance != null &&
                UnitManager.Instance.isActiveAndEnabled)
            {
                if (TryGetComponent(out ISelectable selectable))
                    UnitManager.Instance.RemoveUnit(selectable);
                else
                    ThrowException();
            }
        }

        private void ThrowException()
        {
            throw new System.NullReferenceException("No component on this game object implements ISelectable interface!");
        }

    }

}