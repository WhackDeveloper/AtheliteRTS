using UnityEngine;

namespace IntegrationDemo
{

    using TRavljen.UnitSelection;

    /// <summary>
    /// Component for listening events of <see cref="EntityPlacementManager"/>
    /// and enabling/disabling selection on <see cref="UnitSelector"/> when
    /// placement is active.
    /// </summary>
    [RequireComponent(typeof(EntityPlacementManager))]
    public class ActivePlacementSelectionToggler : MonoBehaviour
    {

        #region Properties

        [Tooltip("Manager of entity placement.")]
        [SerializeField]
        private EntityPlacementManager placement;

        private bool enableSelection;
        
        #endregion

        #region Lifecycle

        private void OnEnable()
        {
            placement.OnPlacementBegin.AddListener(OnPlacementBegin);
            placement.OnPlacementEnd.AddListener(OnPlacementEnd);
        }

        private void OnDisable()
        {
            placement.OnPlacementBegin.RemoveListener(OnPlacementBegin);
            placement.OnPlacementEnd.RemoveListener(OnPlacementEnd);
        }

        private void OnValidate()
        {
            if (placement == null)
                placement = GetComponent<EntityPlacementManager>();
        }

        private void LateUpdate()
        {
            // Update after input's that happen in update cycle.
            // This prevents selection triggering right after being enabled
            // in Update cycle.
            if (!enableSelection) return;
            
            enableSelection = false;
            if (UnitSelector.Instance != null)
            {
                UnitSelector.Instance.SetSelectionEnabled(true);
            }
        }

        #endregion

        #region Listeners

        private void OnPlacementBegin()
        {
            if (UnitSelector.Instance != null)
                UnitSelector.Instance.SetSelectionEnabled(false);
        }

        private void OnPlacementEnd()
        {
            enableSelection = true;
        }

        #endregion
    }

}