using UnityEngine;
using UnityEngine.UI;
using TRavljen.UnitSystem.Build;
using TRavljen.UnitSystem.Interactions;
using TRavljen.UnitSystem.Task;
using TRavljen.UnitSystem.Garrison;
using TRavljen.UnitSystem.Collection;
using TRavljen.UnitSystem.Combat;

namespace TRavljen.UnitSystem.Demo
{

    /// <summary>
    /// Displays the current state of a <see cref="Unit"/> as a floating text for debugging or demo purposes.
    /// </summary>
    /// <remarks>
    /// This component helps visualize the state of a unit during gameplay, making it useful for debugging or as an example implementation. 
    /// It dynamically updates the displayed text based on the unit's active task and interaction targets.
    /// 
    /// <para>
    /// **Usage Notes:**
    /// - Relies on <see cref="UnityEngine.UI.Text"/> for rendering the state text.
    /// - This script is simplified for demo purposes and is not optimized for production.
    /// </para>
    /// </remarks>
    public class DebugUnitStateText : MonoBehaviour
    {

        #region Properties

        [SerializeField]
        [Tooltip("The Unit component whose state will be displayed.")]
        private Unit unit;

        [SerializeField]
        [Tooltip("The Unity UI Text component used to render the unit state text.")]
        private Text text;

        [SerializeField]
        [Tooltip("The Canvas component that contains the text.")]
        private Canvas canvas;

        private ResourceCollector collector;
        private IUnitInteracteeComponent target;
        private Transform cameraTransform;

        #endregion

        #region Lifecycle

        private void Awake()
        {
            collector = unit.GetComponent<ResourceCollector>();

            cameraTransform = Camera.main.transform;
        }

        private void OnEnable()
        {
            unit.OnActiveTaskChange.AddListener(HandleTaskChange);
        }

        private void OnDisable()
        {
            unit.OnActiveTaskChange.RemoveListener(HandleTaskChange);
        }

        private void HandleTaskChange(ITaskHandler previousHandler, ITaskHandler newHandler)
        {
            UpdateTarget(newHandler);
        }

        private void Update()
        {
            canvas.transform.LookAt(cameraTransform.position);

            if (!target.IsNull())
            {
                UpdateTarget(unit.ActiveTask);

                switch (target)
                {
                    case ResourceNode node:
                        HandleResourceNodeTarget(node);
                        break;

                    case GarrisonEntity garrison:
                        HandleGarrisonUnitTarget(garrison);
                        break;

                    case Health health:
                        HandleHealthTarget(health);
                        break;

                    case IResourceDepot depot:
                        HandleResourceDepotTarget(depot);
                        break;

                    case EntityBuilding unitBuilding:
                        HandleUnitBuildingTarget(unitBuilding);
                        break;

                    default:
                        text.text = "Idle";
                        break;
                }
            }
            else
            {
                text.text = "Idle";
            }
        }

        #endregion

        private void UpdateTarget(ITaskHandler activeHandler)
        {
            switch (activeHandler)
            {
                case MobileAttackTargetTaskHandler handler:
                    target = handler.CurrentTarget;
                    break;

                case BuildTargetTaskHandler handler:
                    target = handler.CurrentTarget;
                    break;

                case EnterGarrisonTaskHandler handler:
                    target = handler.CurrentTarget;
                    break;

                case ResourceCollectingTaskHandler handler:
                    if (handler.TargetDepot.IsNotNull())
                        target = handler.TargetDepot;
                    else if (handler.TargetNode != null)
                        target = handler.TargetNode;
                    break;

                default:
                    target = null;
                    break;
            }
        }

        #region Text Helpers

        private void HandleResourceNodeTarget(ResourceNode node)
        {
            if (unit.ActiveTask is not ResourceCollectingTaskHandler collect)
            {
                text.text = "Unknown task";
                return;
            }

            if (collect.IsDepositing)
                text.text = "Depositing";
            else if (collect.IsCollecting)
                text.text = "Collecting (" + collector.CollectedResource.Quantity + ")";
            else
                text.text = "Target: " + node.ResourceAmount.Resource.Name;
        }

        private void HandleGarrisonUnitTarget(GarrisonEntity garrison)
        {
            text.text = "Garrison: " + garrison.gameObject.name;
        }

        private void HandleHealthTarget(Health health)
        {
            if (unit.ActiveTask is not MobileAttackTargetTaskHandler attack)
            {
                text.text = "Unknown task";
                return;
            }

            if (attack.IsAttacking)
                text.text = "Attacking";
            else
                text.text = "Attack: " + health.gameObject.name;
        }

        private void HandleResourceDepotTarget(IResourceDepot _)
        {
            if (collector.CollectedResource.Quantity > 0)
                text.text = "Depositing";
            else
                text.text = "Deposited";
        }

        private void HandleUnitBuildingTarget(EntityBuilding entityBuilding)
        {
            if (unit.ActiveTask is not BuildTargetTaskHandler buildTask)
            {
                text.text = "Unknown task";
                return;
            }

            if (buildTask.IsBuilding)
                text.text = "Building";
            else
                text.text = "Target: " + entityBuilding.name;
        }

        #endregion

    }

}