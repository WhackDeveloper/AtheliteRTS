using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TRavljen.UnitSystem.Combat
{
    
    /// <summary>
    /// Contract for checking distance between attacker and his target and determine
    /// if attacker is within the attack range.
    /// </summary>
    public interface IAttackDistanceHandler
    {
        /// <summary>
        /// Checks if the attacker is within the attack range.
        /// </summary>
        /// <param name="attack">Attacker to check for</param>
        /// <param name="attackPosition">Attack position itself, from where attacker must attack.</param>
        /// <param name="target">Target of the attacker</param>
        /// <returns>Returns true if attacker is within attack range to the attack the target; Otherwise false.</returns>
        public bool IsTargetInAttackRange(IUnitAttack attack, Vector3 attackPosition, IHealth target);
    }

    /// <summary>
    /// Default implementation for checking distance for attack. Uses attackers position relative to
    /// it's attack position. 
    /// </summary>
    public class UnitAttackDistanceHandler : IAttackDistanceHandler
    {
        public bool IsTargetInAttackRange(IUnitAttack attack, Vector3 attackPosition, IHealth target)
        {
            float distance = Vector3.Distance(attack.Position, attackPosition);
            return attack.MinInteractionRange < distance && distance < attack.MaxInteractionRange;
        }
    }
    
}