namespace TRavljen.UnitSystem.Build
{
    /// <summary>
    /// Provides extension methods for <see cref="AUnitSO"/> to determine 
    /// whether a unit requires building or supports automatic building.
    /// </summary>
    public static class BuildableEntitySOHelper
    {
        /// <summary>
        /// Determines if the unit requires a build process by checking for the <see cref="IBuildableCapability"/>.
        /// </summary>
        /// <param name="entity">The entity ScriptableObject to evaluate.</param>
        /// <returns>
        /// <c>true</c> if the entity has the <see cref="IBuildableCapability"/>; otherwise, <c>false</c>.
        /// </returns>
        public static bool RequiresBuilding(this AEntitySO entity)
            => entity.TryGetCapability(out IBuildableCapability _);
    }
}