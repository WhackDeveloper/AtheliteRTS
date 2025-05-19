using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TRavljen.UnitSystem.Combat
{

    /// <summary>
    /// Interface defining a defensive position for a unit.
    /// Allows the unit to defend a specific position and orientation
    /// with a configurable defensive range.
    /// </summary>
    public interface IDefendPosition
    {
        /// <summary>
        /// The central position being defended by the unit.
        /// </summary>
        Vector3 DefendPosition { get; }

        /// <summary>
        /// The facing direction of the unit when defending.
        /// </summary>
        Vector3 DefendDirection { get; }

        /// <summary>
        /// The defensive range at which the unit will engage any enemies in the area.
        /// </summary>
        float DefensiveRange { get; }

        /// <summary>
        /// Checks if the position is within the defend range.
        /// </summary>
        /// <param name="position">Position to check</param>
        /// <returns>Returns true if position is within range; otherwise false.</returns>
        bool IsInRange(Vector3 position);

        /// <summary>
        /// Configures the unit to defend a specific position and direction.
        /// </summary>
        /// <param name="position">The position to defend.</param>
        /// <param name="direction">The direction the unit should face while defending.</param>
        /// <param name="move">Whether the unit should move to the defensive position.</param>
        void Defend(Vector3 position, Vector3 direction, bool move);

        /// <summary>
        /// Checks if unit attack is in defensive stance and if it also implements
        /// <see cref="IDefendPosition"/> interface, then it updates it's position
        /// and direction. The move parameter on interface will always be <c>false</c> when
        /// this helper is used.
        /// </summary>
        /// <param name="unitAttack">Attack component</param>
        /// <param name="position">Defend position</param>
        /// <param name="direction">Defend facing direction</param>
        public static void UpdateStance(IUnitAttack unitAttack, Vector3 position, Vector3 direction)
        {
            if (unitAttack != null &&
                unitAttack.Stance == AttackStance.Defensive &&
                unitAttack is IDefendPosition defend)
            {
                defend.Defend(position, direction, false);
            }
        }
    }

}