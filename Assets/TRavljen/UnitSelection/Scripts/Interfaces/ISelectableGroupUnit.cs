namespace TRavljen.UnitSelection
{

    /// <summary>
    /// Interfaces used for communication between a group unit and
    /// <see cref="UnitSelector"/> and is used to retrieve <see cref="ISelectableGroup"/>
    /// from the group unit.
    /// </summary>
    public interface ISelectableGroupUnit : ISelectable
    {

        /// <summary>
        /// Specifies the group that controls the selection state of the entire group.
        /// </summary>
        public ISelectableGroup Group { get; }

        /// <summary>
        /// Updates the group of the group unit.
        /// </summary>
        /// <param name="group">New group of the unit.</param>
        public void SetGroup(ISelectableGroup group);

    }

}