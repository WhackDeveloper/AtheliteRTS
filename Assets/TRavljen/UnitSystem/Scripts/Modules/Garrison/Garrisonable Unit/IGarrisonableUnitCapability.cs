namespace TRavljen.UnitSystem.Garrison
{

    /// <summary>
    /// Defines a capability for a unit to enter a garrison. 
    /// This interface is implemented by data structures that specify garrison-related 
    /// settings for a unit. Behavior is managed by components such as 
    /// <see cref="AUnitComponent"/> subclasses.
    /// </summary>
    public interface IGarrisonableUnitCapability : IUnitCapability { }

}