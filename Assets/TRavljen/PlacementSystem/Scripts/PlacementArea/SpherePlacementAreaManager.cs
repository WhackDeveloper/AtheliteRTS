using UnityEngine;

namespace TRavljen.PlacementSystem
{

    /// <summary>
    /// Component which manages a list of areas of type sphere/ring.
    /// You can add or remove them dynamically in runtime.
    /// </summary>
    public class SpherePlacementAreaManager : ASpherePlacementAreas
    {

        #region Properties
        
        [Tooltip("Specifies ring areas for valid placements. At least one must be present!")]
        [SerializeField]
        protected SphereBounds[] areas = new SphereBounds[0];

        protected override SphereBounds[] GetAreas() => areas;

        #endregion
        
    }
    
}