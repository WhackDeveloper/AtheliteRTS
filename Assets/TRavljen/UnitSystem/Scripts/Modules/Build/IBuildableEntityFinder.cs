using System.Collections;
using System.Collections.Generic;
using TRavljen.UnitSystem.Interactions;
using UnityEngine;

namespace TRavljen.UnitSystem.Build
{
 
    /// <summary>
    /// Provides a contract for finding buildable units nearby.
    /// </summary>
    public interface IBuildableEntityFinder
    {
        
        /// <summary>
        /// Finds nearby unfinished buildable entities within a specified range.
        /// </summary>
        /// <param name="position">The position to search from.</param>
        /// <param name="range">The maximum search radius.</param>
        /// <param name="filterFull">
        /// Specifies whether to exclude units building that have reached their build limit.
        /// </param>
        /// <returns>
        /// An array of <see cref="IBuildableEntity"/> instances that match the criteria,
        /// sorted by proximity to the given position.
        /// </returns>
        public IBuildableEntity[] FindNearby(Vector3 position, float range, bool filterFull);

        /// <summary>
        /// Finds the closest unfinished buildable entity within a specified range.
        /// </summary>
        /// <param name="position">The position to search from.</param>
        /// <param name="range">The maximum search radius.</param>
        /// <param name="filterFull">
        /// Specifies whether to exclude entities that have reached their build interaction limit.
        /// </param>
        /// <param name="buildable">
        /// When the method returns <c>true</c>, contains the closest <see cref="buildable"/>
        /// instance; otherwise, <c>null</c>.
        /// </param>
        /// <returns>
        /// <c>true</c> if an eligible unit building is found; otherwise, <c>false</c>.
        /// </returns>
        public bool FindClosest(Vector3 position, float range, bool filterFull, out IBuildableEntity buildable);
        
    }

    /// <summary>
    /// Default implementation for <see cref="IBuildableEntityFinder"/> where it takes players list of entities
    /// and searches for the nearby buildable entities.
    /// </summary>
    [System.Serializable]
    public class BuildableEntityFinder : IBuildableEntityFinder
    {
        private APlayer _player;

        public BuildableEntityFinder(APlayer player)
        {
            this._player = player;
        }

        public IBuildableEntity[] FindNearby(Vector3 position, float range, bool filterFull)
        {
            List<(IBuildableEntity buildable, float distance)> buildingUnits = new();

            foreach (var entity in _player.GetEntities())
            {
                if (entity.Buildable is not { } buildableEntity ||
                    buildableEntity.IsNull() ||
                    !buildableEntity.IsActive) 
                    continue;
                
                if (buildableEntity.IsBuilt || buildableEntity.BuildAutomatically)
                    continue;
                
                // If we are filtering full, check if limit is reached.
                if (filterFull && 
                    buildableEntity is ILimitedUnitInteractionTarget limits && 
                    limits.HasReachedLimit())
                    continue;

                buildingUnits.Add((buildableEntity, Vector3.Distance(position, buildableEntity.Position)));
            }

            buildingUnits.Sort((a, b) => a.distance.CompareTo(b.distance));

            return buildingUnits.ConvertAll(item => item.buildable).ToArray();
        }
     
        public bool FindClosest(Vector3 position, float range, bool filterFull, out IBuildableEntity buildable)
        {
            var entities = FindNearby(position, range, filterFull);

            if (entities.Length > 0)
            {
                // Array is already sorted, so we can safely take the first object
                buildable = entities[0];
                return true;
            }

            buildable = null;
            return false;
        }
    }
}

