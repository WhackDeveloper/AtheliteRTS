using System;
using System.Collections.Generic;
using TRavljen.UnitSystem;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;

namespace TRavljen.UnitSystem.Demo
{
    using TRavljen.UnitSystem.Build;

    /// <summary>
    /// Controls the production actions UI for the selected unit or player default actions in the demo scene.
    /// Displays production options and handles user interaction with them.
    /// </summary>
    public class SelectedUnitProductionActionsController : MonoBehaviour
    {

        #region Properties

        [Tooltip("Manages the currently selected units and notifies on selection changes.")]
        public EntitySelectionManager selectionManager;

        [Tooltip("The UI container that holds production action buttons.")]
        [SerializeField]
        private Transform container;

        [Tooltip("Prefab used to create production action buttons.")]
        [SerializeField]
        private UnitProductionView unitProductionViewPrefab;

        [Tooltip("Displays the title for the currently selected entity.")]
        [SerializeField]
        private Text titleText;

        [Tooltip("Button used to destroy the selected unit.")]
        [SerializeField]
        private Button destroyUnitButton;

        [Tooltip("The player component managing units and production actions.")]
        public APlayer Player;

        /// <summary>
        /// Invoked when a production action button is clicked.
        /// </summary>
        /// <summary>
        /// The currently displayed entity in the UI.
        /// </summary>
        public Entity DisplayedEntity { private set; get; }

        private readonly List<AEntityActionUIElement> actionElements = new();

        #endregion

        #region Lifecycle

        private void Start() => ReloadUI();

        private void OnValidate()
        {
            if (selectionManager.IsNull())
                selectionManager = UnityEngine.Object.FindFirstObjectByType<EntitySelectionManager>();

            if (Player.IsNull())
                Player = GetComponentInParent<APlayer>();
        }

        private void OnEnable()
        {   
            selectionManager.OnSelectionChange.AddListener(SelectionChanged);
            Player.OnUnitAdded.AddListener(HandleUnitAdded);
            Player.OnUnitRemoved.AddListener(HandleUnitRemoved);
            Player.OnRegisterProducible.AddListener(HandleProducibleRegistered);

            ProductionEvents.Instance.OnAllProductionCancelled.AddListener(HandleAllProductionCancelled);
            ProductionEvents.Instance.OnProductionCancelled.AddListener(HandleProductionCancelled);
            ProductionEvents.Instance.OnNewProductionScheduled.AddListener(HandleNewProductionScheduled);
            ProductionEvents.Instance.OnProductionFinished.AddListener(HandleProductionFinished);

            BuildEvents.Instance.OnBuildCompleted.AddListener(HandleUnitBuilt);

            if (Player.TryGetModule(out ResearchModule researchModule))
            {
                researchModule.OnResearchFinished.AddListener(HandleResearchFinished);
            }

            if (Player.TryGetModule(out UnitLimitationModule limits))
            {
                limits.OnLimitChanged.AddListener(ReloadUI);
                limits.OnUnitCountChange.AddListener(ReloadUI);
            }
        }

        private void OnDisable()
        {
            selectionManager.OnSelectionChange.RemoveListener(SelectionChanged);
            Player.OnUnitAdded.RemoveListener(HandleUnitAdded);
            Player.OnUnitRemoved.RemoveListener(HandleUnitRemoved);
            Player.OnRegisterProducible.RemoveListener(HandleProducibleRegistered);

            BuildEvents.Instance.OnBuildCompleted.RemoveListener(HandleUnitBuilt);

            if (Player.TryGetModule(out ResearchModule researchModule))
            {
                researchModule.OnResearchFinished.RemoveListener(HandleResearchFinished);
            }

            if (Player.TryGetModule(out UnitLimitationModule limits))
            {
                limits.OnLimitChanged.RemoveListener(ReloadUI);
                limits.OnUnitCountChange.RemoveListener(ReloadUI);
            }
        }

        private void Update()
        {
            // Check if any UI action is triggering action elements.
            foreach (var action in actionElements)
            {
                if (action.CheckActionHotKey())
                {
                    break;
                }
            }
        }

        #endregion

        #region Event Handlers

        private void HandleProductionFinished(ActiveProduction unitProduction, ProducibleQuantity _)
            => HandleProductionUpdated(unitProduction);

        private void HandleNewProductionScheduled(ActiveProduction unitProduction, AProducibleSO producible, long quantity)
            => HandleProductionUpdated(unitProduction);

        private void HandleProductionCancelled(ActiveProduction unitProduction, AProducibleSO producible, long quantity)
            => HandleProductionUpdated(unitProduction);

        private void HandleAllProductionCancelled(ActiveProduction unitProduction)
            => HandleProductionUpdated(unitProduction);

        private void HandleResearchFinished(ResearchSO research) => ReloadUI();
        
        private void HandleProductionUpdated(ActiveProduction unitProduction)
        {
            if (unitProduction.Owner == Player)
            {
                ReloadUI();
            }
        }

        private void HandleUnitAdded(IUnit unit) => ReloadUI();
        private void HandleUnitRemoved(IUnit unit) => ReloadUI();

        /// <summary>
        /// Handles the event when a unit is built, refreshing the UI.
        /// </summary>
        private void HandleUnitBuilt(EntityBuilding entity)
        {
            ReloadUI();
        }

        private void HandleProducibleRegistered(AProducibleSO producible, long quantity)
        {
            ReloadUI();
        }

        /// <summary>
        /// Updates the UI when the selection changes.
        /// </summary>
        /// <param name="selectedUnits">The list of currently selected entities.</param>
        private void SelectionChanged(List<Entity> selectedUnits)
        {
            var selectedUnit = selectedUnits.Count > 0 ? selectedUnits[0] : null;
            // Apply changes only if there was a change to apply
            if (DisplayedEntity != selectedUnit)
            {
                DisplayedEntity = selectedUnit;

                titleText.text = selectedUnit != null ? selectedUnit.Data.Name : "None selected";
                
                ReloadUI();
            }

            destroyUnitButton.gameObject.SetActive(selectedUnit is Unit unit && unit != null);
        }

        #endregion

        #region User Interaction

        /// <summary>
        /// Destroys selected unit and removes it from the unit owner (player).
        /// </summary>
        public void DestroySelectedUnit()
        {
            if (DisplayedEntity == null) return;

            // Use method that properly cleanups internal references
            // and refunds the player according to unit and player configuration.
            if (DisplayedEntity is Unit unit)
                unit.DestroyUnit(true);
            // Demo does not refund for entities, player uses only units
            else
                DisplayedEntity.DestroyEntity();

            DisplayedEntity = null;

            ReloadUI();
        }

        #endregion

        #region UI Management

        /// <summary>
        /// Refreshes the production actions UI for the currently selected unit or player default actions.
        /// </summary>
        public void ReloadUI()
        {
            ClearUI();

            if (DisplayedEntity != null &&
                DisplayedEntity is Unit unit)
            {
                BuildUI(unit);
            }
        }

        /// <summary>
        /// Builds the production actions UI for a specific unit.
        /// </summary>
        /// <param name="unit">The selected unit.</param>
        private void BuildUI(Unit unit)
        {
            if (unit.Owner != Player)
            {
                Debug.Log("This unit is not owned by the local player");
                return;
            }

            if (!unit.IsOperational)
            {
                return;
            }

            var layoutData = new EntityUILayoutData()
            {
                Container = container,
                SectionIndex = 0
            };
            
            var uiHandlers = unit.GetComponentsInChildren<IEntityUIHandler>();
            foreach (var uiHandler in uiHandlers)
            {
                if (uiHandler.BuildUIElements(layoutData))
                {
                    UpdateActions();
                    break;
                }

                break;
            }
        }

        private void UpdateActions()
        {
            var actions = container.GetComponentsInChildren<AEntityActionUIElement>();
            actionElements.Clear();
            actionElements.AddRange(actions);
        }

        /// <summary>
        /// Clears all UI elements from the container.
        /// </summary>
        private void ClearUI()
        {
            actionElements.Clear();

            for (int i = container.childCount - 1; i >= 0; --i)
            {
                var child = container.GetChild(i).gameObject;
                Destroy(child);
            }
        }

        #endregion

    }

}