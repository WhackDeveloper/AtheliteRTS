namespace TRavljen.UnitSystem.Editor
{
    using UnityEngine;

    /// <summary>
    /// Defines layout fields required for most sections.
    /// Main player defined or root object for packages,
    /// if none is provided objects will be spawned on scene root.
    /// </summary>
    internal interface IGuideLayout
    {
        public APlayer MainPlayer { get; }
        public Transform PackageRoot { get; }
    }

    /// <summary>
    /// Defines guide section contract.
    /// </summary>
    internal interface IGuideSection
    {
        public string Title { get; }
        public string Id { get; }

        void OnEnable();
        
        void OnGUI(IGuideLayout layout);
    }
}