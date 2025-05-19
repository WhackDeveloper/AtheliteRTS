using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TRavljen.UnitSystem
{

    /// <summary>
    /// Provides utilities for determining relationships between entities,
    /// such as ally or enemy status.
    /// </summary>
    public static class EntityRelationshipHelper
    {

        /// <summary>
        /// Checks if two entities are considered enemies.
        /// </summary>
        /// <param name="thisEntity">The first entity.</param>
        /// <param name="otherEntity">The second entity.</param>
        /// <returns>True if the entities are enemies; otherwise, false.</returns>
        public static bool IsEnemy(this IEntity thisEntity, IEntity otherEntity)
        {
            return AreEntitiesEnemies(thisEntity, otherEntity);
        }

        /// <summary>
        /// Checks if two entities are considered allies.
        /// </summary>
        /// <param name="thisEntity">The first entity.</param>
        /// <param name="otherEntity">The second entity.</param>
        /// <returns>True if the entities are allies; otherwise, false.</returns>
        public static bool IsAlly(this IEntity thisEntity, IEntity otherEntity)
        {
            return AreEntitiesAllied(thisEntity, otherEntity);
        }

        /// <summary>
        /// Checks if two entities are considered enemies.
        /// </summary>
        /// <param name="entityA">The first entity.</param>
        /// <param name="entityB">The second entity.</param>
        /// <returns>True if the entities are enemies; otherwise, false.</returns>
        public static bool AreEntitiesEnemies(IEntity entityA, IEntity entityB)
        {
            return !AreEntitiesAllied(entityA, entityB);
        }

        /// <summary>
        /// Checks if two entities are considered allies.
        /// </summary>
        /// <param name="entityA">The first entity.</param>
        /// <param name="entityB">The second entity.</param>
        /// <returns>True if the entities are allies; otherwise, false.</returns>
        public static bool AreEntitiesAllied(IEntity entityA, IEntity entityB)
        {
            if (entityA.Owner == entityB.Owner)
                return true;

            if (entityA.Owner == null || entityB.Owner == null)
                return false;

            return entityA.Owner.ArePlayersAllied(entityB.Owner);
        }

    }

}