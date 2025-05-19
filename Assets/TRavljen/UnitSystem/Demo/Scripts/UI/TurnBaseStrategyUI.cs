using UnityEngine;

namespace TRavljen.UnitSystem.Demo
{

    /// <summary>
    /// UI component for handling turn-based strategy interactions, including 
    /// detecting input for ending the current player's turn and invoking the 
    /// turn-end process.
    /// </summary>
    /// <remarks>
    /// Currently, only the Production module supports turn-based updates. 
    /// Other systems such as unit movement, collection, building, and attacking 
    /// are not yet integrated with turn-based behavior.
    /// </remarks>
    public class TurnBaseStrategyUI : MonoBehaviour
    {

        /// <summary>
        /// Reference to the <see cref="TurnBaseProductionManager"/> that manages
        /// turn-based production updates.
        /// </summary>
        [SerializeField]
        private TurnBaseProductionManager manager;

        /// <summary>
        /// Key used to end the current player's turn via keyboard input.
        /// </summary>
        [SerializeField]
        private KeyCode endTurnKey = KeyCode.H;

        /// <summary>
        /// Monitors for the end-turn key press each frame.
        /// </summary>
        private void Update()
        {
            if (Input.GetKeyDown(endTurnKey))
            {
                EndTurnClicked();
            }
        }

        /// <summary>
        /// Invoked when the "End Turn" action is triggered (via button or key press).
        /// </summary>
        public void EndTurnClicked()
        {
            manager.EndTurnForCurrentPlayer();
        }

    }

}