using UnityEngine;

namespace TRavljen.UnitSystem.Demo
{

    class PlayerUIManager : MonoBehaviour
    {

        [Header("General")]

        [SerializeField]
        private APlayer player;

        [SerializeField]
        private EntitySelectionManager selectionManager;

        [Header("UI Components")]
        [SerializeField]
        private SelectedEntityInfoController selectedEntityController;

        [SerializeField]
        private PlayerResourceInfoController resourceController;

        [SerializeField]
        private GarrisonButtonController garrisonController;

        [SerializeField]
        private SelectedUnitProductionActionsController productionActionsController;

        [SerializeField]
        private ConsoleController consoleController;

        private void Awake()
        {
            Configure();
        }

        private void OnValidate()
        {
            Configure();
        }

        private void Configure()
        {
            if (player.IsNull())
            {
                player = GetComponentInParent<APlayer>();
                selectionManager = Object.FindFirstObjectByType<EntitySelectionManager>();
            }
            
            // Get UI components
            if (selectedEntityController == null)
                selectedEntityController = GetComponentInChildren<SelectedEntityInfoController>();

            if (resourceController == null)
                resourceController = GetComponentInChildren<PlayerResourceInfoController>();

            if (garrisonController == null)
                garrisonController = GetComponentInChildren<GarrisonButtonController>();

            if (productionActionsController == null)
                productionActionsController = GetComponentInChildren<SelectedUnitProductionActionsController>();

            if (consoleController == null)
                consoleController = GetComponentInChildren<ConsoleController>();

            // Get Player components
            if (player != null)
            {
                if (selectionManager == null)
                    selectionManager = player.GetComponentInChildren<EntitySelectionManager>();
            }

            // Update UI components
            if (selectedEntityController != null && selectionManager != null)
                selectedEntityController.selectionManager = selectionManager;

            if (resourceController != null && player != null)
                resourceController.player = player;

            if (garrisonController != null)
            {
                if (player != null)
                    garrisonController.player = player;
                if (selectionManager != null)
                    garrisonController.selectionManager = selectionManager;
            }

            if (productionActionsController != null)
            {
                if (selectionManager != null)
                    productionActionsController.selectionManager = selectionManager;
                if (player != null)
                    productionActionsController.Player = player;
            }

            if (consoleController != null && player != null)
            {
                consoleController.player = player;
            }
        }
    }

}
