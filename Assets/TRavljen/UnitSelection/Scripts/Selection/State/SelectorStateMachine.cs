using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TRavljen.UnitSelection
{

    /// <summary>
    /// State management for <see cref="UnitSelector"/>. States define behaviour
    /// and flow of the selection logic.
    /// </summary>
    internal class SelectorStateMachine
    {
        private readonly Dictionary<SelectionStateId, SelectorBaseState> states =
            new Dictionary<SelectionStateId, SelectorBaseState>();

        private SelectionStateId currentState;

        internal SelectionStateId CurrentState => currentState;

        internal void RegisterState(SelectorBaseState state)
        {
            states.Add(state.stateId, state);
        }

        internal void ChangeState(UnitSelector selector, SelectionStateId stateId)
        {
            states[currentState]?.Exit(selector);
            currentState = stateId;
            states[currentState].Enter(selector);
        }

        internal SelectorBaseState GetState(SelectionStateId stateId)
            => states[stateId];

        internal void Update(UnitSelector selector)
        {
            states[currentState].Update(selector);
        }

    }

}