using UnityEngine;
using TRavljen.UnitSystem;
using TRavljen.PlacementSystem;
using UnityEngine.Events;

namespace IntegrationDemo
{
    using TRavljen.UnitSystem.Build;
    using TRavljen.UnitSystem.Demo;

    /// <summary>
    /// Placement manager using <see cref="ObjectPlacement"/> and
    /// <see cref="ProductionModule.OnStartPlacementRequest"/>.
    /// Module invokes the start of placement, while Object Placement is
    /// responsible for placing the object in world space.
    /// </summary>
    /// <remarks>
    /// This component demonstrates integration of object placement.
    /// </remarks>
    public class EntityPlacementManager : MonoBehaviour, IValidatePlacement
    {

        #region Properties

        [SerializeField]
        private ObjectPlacement objectPlacement;

        [SerializeField]
        private APlayer player;

        [Tooltip("Selection manager which belongs to the player.")]
        [SerializeField]
        private EntitySelectionManager entitySelection;

        private ResourceModule resourceModule;
        private UnitLimitationModule limitationModule;

        public UnityEvent OnPlacementBegin = new();
        public UnityEvent OnPlacementEnd = new();

        #endregion

        #region Lifecycle

        private void Awake()
        {
            resourceModule = player.GetModule<ResourceModule>();
            limitationModule = player.GetModule<UnitLimitationModule>();
        }

        /// <summary>
        /// Subscribes to relevant events and initializes the object placement validators 
        /// and unit selection filters when the component is enabled.
        /// </summary>
        protected void OnEnable()
        {
            objectPlacement.AdditionalValidator = this;

            if (player.TryGetModule(out ProductionModule module))
            {
                module.OnStartPlacementRequest.AddListener(StartPlacement);
            }

            objectPlacement.Events.OnPlacementStart.AddListener(_ =>
            {
                OnPlacementBegin.Invoke();
            });
            objectPlacement.Events.OnPlacementEnd.AddListener(HandlePlacementDone);
            objectPlacement.Events.OnPlacementCancel.AddListener(() =>
            {
                OnPlacementEnd.Invoke();
            });
        }

        /// <summary>
        /// Unsubscribes from events and removes validators and filters when the component is disabled.
        /// </summary>
        protected void OnDisable()
        {
            if (objectPlacement.AdditionalValidator == this as IValidatePlacement)
            {
                objectPlacement.AdditionalValidator = null;
            }

            objectPlacement.Events.OnPlacementEnd.RemoveListener(HandlePlacementDone);

            if (player.TryGetModule(out ProductionModule module))
            {
                module.OnStartPlacementRequest.RemoveListener(StartPlacement);
            }
        }

        /// <summary>
        /// Retrieves required component references.
        /// </summary>
        protected void OnValidate()
        {
            if (player.IsNull())
                player = GetComponentInParent<APlayer>();
            
            if (objectPlacement.IsNull())
                objectPlacement = Object.FindFirstObjectByType<ObjectPlacement>();
            
            if (entitySelection.IsNull())
                entitySelection = Object.FindFirstObjectByType<EntitySelectionManager>();

            if (entitySelection.IsNull())
            {
                entitySelection = transform.GetComponentInParent<EntitySelectionManager>();

                if (entitySelection.IsNull())
                    transform.GetComponentInChildren<EntitySelectionManager>();
            }
        }

        /// <summary>
        /// Starts the placement process for a given entity prefab.
        /// </summary>
        private void StartPlacement(PlacementRequiredInfo info)
        {
            // The quantity is ignored in this case. Only a single object can be placed at once.
            if (info.quantity > 1)
            {
                Debug.Log(
                    $"Object placement supports only 1 object at a time. Quantity of {info.quantity} will be ignored.");
            }

            objectPlacement.BeginPlacement(info.prefab.gameObject);
        }

        #endregion

        #region IValidatePlacement

        /// <summary>
        /// Validates whether a given placement is valid by checking player resources.
        /// </summary>
        /// <param name="gameObject">The object being placed.</param>
        /// <param name="bounds">The bounds of the placement area.</param>
        /// <param name="rotation">The rotation of the object being placed.</param>
        /// <returns><c>true</c> if placement is valid; otherwise, <c>false</c>.</returns>
        public bool IsPlacementValid(GameObject gameObject, Bounds bounds, Quaternion rotation)
        {
            // Validate that player still has resources and other requirements to perform this action
            if (!gameObject.TryGetComponent(out Unit newUnit)) return true;

            // Check for requirements
            if (!player.FulfillsRequirements(newUnit.Data, 1))
                return false;

            // Check for limit reached
            if (limitationModule.IsNotNull() && limitationModule.HasReachedLimit(newUnit.Data.ID))
                return false;

            if (resourceModule.IsNotNull())
            {
                var cost = newUnit.Data.Cost;
                return resourceModule.HasEnoughResources(cost);
            }

            return false;
        }

        #endregion

        #region Building Events

        /// <summary>
        /// Handles the event when object placement is completed, validating resources 
        /// and starting the building process.
        /// </summary>
        private void HandlePlacementDone(GameObject newObject, PlacementResult result)
        {
            OnPlacementEnd.Invoke();

            if (!newObject.TryGetComponent(out IUnit newUnit))
            {
                Debug.Log("Demo integration supports Unit types only.");
                return;
            }

            // Check if still valid for placement, if something changed while player was placing the unit.
            if (!player.FulfillsRequirements(newUnit.Data, 1))
            {
                Debug.Log("Unit no longer fulfills requirements for being placed.");
                return;
            }

            var cost = newUnit.Data.Cost;

            if (!resourceModule.HasEnoughResources(cost))
            {
                Debug.Log("Did not have enough resources when placing. With additional validator in place, this should not happen.");
                return;
            }

            resourceModule.RemoveResources(cost);

            bool sendBuilder = true;
            // Start building if supported.
            if (!player.TryGetModule(out BuildingModule manager) ||
                !manager.StartBuilding(newObject))
            {
                sendBuilder = false;
                Debug.Log("Building process not supported for the placed unit");
            }

            player.AddUnit(newUnit, true);

            if (!sendBuilder) return;
            
            // Send any selected builder to construct the unit
            if (!newObject.TryGetComponent(out EntityBuilding building) ||
                building.BuildAutomatically) return;
            
            foreach (var entity in entitySelection.SelectedEntities)
            {
                if (entity is not Unit { Builder: not null } selectedUnit) continue;
                
                // Check if builder has active task and cancelling it is disabled.
                // if (selectedUnit.HasActiveTask() && !cancelTaskToSelectedBuilders)
                //     continue;
                // selectedUnit.RemoveAllTasks();
                if (selectedUnit.HasActiveTask())
                    continue;
                
                selectedUnit.Builder?.GoBuildUnit(building);
            }
        }

        #endregion
        
    }

}