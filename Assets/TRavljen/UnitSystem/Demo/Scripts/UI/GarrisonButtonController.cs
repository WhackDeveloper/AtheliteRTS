using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace TRavljen.UnitSystem.Demo
{

    using Garrison;

    /// <summary>
    /// The <see cref="GarrisonButtonController"/> class is a demo component responsible for
    /// managing the user interface and functionality of garrisoning units within the game.
    /// It allows players to enter or release units from a garrison through a button interaction.
    /// </summary>
    public class GarrisonButtonController : MonoBehaviour
    {

        public APlayer player;

        [Tooltip("Root UI element for garrison control.")]
        [SerializeField]
        private GameObject garrisonElement;

        [Tooltip("Button for entering or releasing units in garrison.")]
        [SerializeField]
        private Button garrisonButton;

        [Tooltip("Text component for the GarrisonButton, which changes to Release or Garrison units.")]
        [SerializeField]
        private Text garrisonButtonText;

        [Tooltip("The entity selection manager responsible for tracking selected entities.")]
        public EntitySelectionManager selectionManager;

        private IGarrisonEntity garrisonUnit;

        #region Lifecycle

        private void Start()
        {
            // Setup button and event listeners
            garrisonButton.onClick.AddListener(GarrisonButtonClicked);
            garrisonButton.transform.parent.gameObject.SetActive(false);

            selectionManager.OnSelectionChange.AddListener(OnSelectionChanged);
        }

        private void Update()
        {
            UpdateButtonState();
        }

        private void OnSelectionChanged(List<Entity> entities)
        {
            // Update garrison reference

            garrisonUnit = null;

            foreach (var entity in entities)
            {
                if (entity is Unit unit && unit.Garrison != null)
                {
                    // Take first one
                    garrisonUnit = unit.Garrison;
                    break;
                }
            }

            UpdateButtonState();
        }

        /// <summary>
        /// Update Button state in Update method, duo to units coming in and out.
        /// This can also be done via listeners to the GarrisonUnit implementation itself.
        /// </summary>
        private void UpdateButtonState()
        {
            // Make sure the selected garrison is also operational
            if (garrisonUnit != null && garrisonUnit.Entity is IUnit unit && unit.IsOperational)
            {
                garrisonElement.SetActive(true);
                garrisonButtonText.text = garrisonUnit.GarrisonedUnits.Count == 0 ? "Garrison" : "Release units";
            }
            else
            {
                garrisonElement.SetActive(false);
            }
        }

        #endregion

        #region User Interaction

        /// <summary>
        /// Toggles garrison behavior when the button is clicked. If units are already 
        /// garrisoned, they are released. Otherwise, it attempts to garrison eligible 
        /// units from the player's active units.
        /// </summary>
        public void GarrisonButtonClicked()
        {
            if (garrisonUnit.GarrisonedUnits.Count > 0)
            {
                // Removes all units from garrison
                garrisonUnit.RemoveAllUnits();
                return;
            }

            if (player.TryGetModule(out GarrisonModule module))
            {
                module.CallInNearbyUnits(garrisonUnit, 100, true);
            }
            else
            {
                IUnit[] allUnits = player.GetUnits();
                int calledInCharactersCount = 0;

                foreach (var unit in allUnits)
                {
                    // Note: Using unit.TryScheduleTask(garrisonUnit) would work just as well
                    // but here we know exactly which component should be handling this.
                    if (!unit.IsNotNull() ||
                        !unit.IsOperational ||
                        unit.GarrisonableUnit == null) 
                        continue;
                    
                    if (!unit.GarrisonableUnit.GoEnterGarrison(garrisonUnit))
                        continue;

                    calledInCharactersCount += 1;

                    // Recall only as many as building can take.
                    if (calledInCharactersCount == garrisonUnit.GarrisonCapacity)
                        break;
                }
            }
        }

        #endregion

    }

}