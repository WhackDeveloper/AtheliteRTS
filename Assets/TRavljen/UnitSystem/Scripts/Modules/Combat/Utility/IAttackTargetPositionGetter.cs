using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TRavljen.UnitSystem.Combat
{
    
    /// <summary>
    /// Contract for finding the most optimal position to attack, based
    /// on current placement of attacker/target.
    /// </summary>
    public interface IAttackTargetPositionGetter
    {
        /// <summary>
        /// Gets the closest position near the target for the attacker to position
        /// itself before performing an attack.
        /// </summary>
        public Vector3 GetAttackPosition(IUnitAttack attacker, IHealth target);
    }

    /// <summary>
    /// Default implementation of <see cref="IAttackTargetPositionGetter"/>
    /// that uses collider on the object to get the closest point on it.
    /// If this position is used directly, with no modifications,
    /// make sure stopping distance is set to prevent attempting to walk into the collider.
    /// </summary>
    public class UnitAttackTargetPositionGetter: IAttackTargetPositionGetter
    {
        public Vector3 GetAttackPosition(IUnitAttack attacker, IHealth target)
        {
            return target.transform.TryGetComponent(out Collider collider) ?
                collider.bounds.ClosestPoint(attacker.Position) : target.Position;
        }
    }
    
} 