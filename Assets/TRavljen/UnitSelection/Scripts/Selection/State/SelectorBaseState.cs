using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TRavljen.UnitSelection
{
    internal abstract class SelectorBaseState
    {
        internal abstract SelectionStateId stateId { get; }

        internal virtual void Enter(UnitSelector selector) { }
        internal virtual void Update(UnitSelector selector) { }
        internal virtual void Exit(UnitSelector selector) { }
    }

}