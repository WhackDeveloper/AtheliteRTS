using System;
using UnityEngine;

namespace TRavljen.PlacementSystem
{

    /// <summary>
    /// Type of the bounds in which the objects can be placed.
    /// </summary>
    public enum PlacementAreaType {
        /// <summary>
        /// Objects can be placed anywhere where there is ground.
        /// </summary>
        Anywhere,
        /// <summary>
        /// Objects can be placed on ground within the box bounds of the area.
        /// </summary>
        Box,
        /// <summary>
        /// Objects can be placed on ground in the sphere bounds of the area.
        /// </summary>
        Sphere,
        /// <summary>
        /// Placement area defined by a custom implementation by implementing <see cref="IPlacementArea"/> interface.
        /// </summary>
        Custom
    }

    /// <summary>
    /// Specifies the information about valid placement area for objects.
    /// When placement area is limited, use <see cref="ClosestPosition"/>
    /// to limit the positions within the valid bounds.
    /// </summary>
    [Serializable]
    public struct PlacementAreaInfo : IPlacementArea
    {

        [Tooltip("Specifies placement area type. In cases where map has a limit or " +
            "placement is allowed around certain objects use this to limit " +
            "the placement area.")]
        public PlacementAreaType boundsType;

        [Tooltip("Specifies placement area with box shape. Set it's position and size.")]
        public Bounds boxBounds;

        [Tooltip("Specifies placement area with sphere shape. Set it's position and size.")]
        public SphereBounds sphereBounds;

        /// <summary>
        /// Specifies custom placement area component. This allows easy use of
        /// multiple placement areas with custom implementation.
        /// </summary>
        public IPlacementArea customArea;

        /// <summary>
        /// Clamps position within the valid placement area. For <see cref="PlacementAreaType.Anywhere"/>
        /// the same value will be returned, for limiting types the closest position within the bounds
        /// will be found.
        /// </summary>
        /// <param name="position">Current position</param>
        /// <returns>Valid position</returns>
        public Vector3 ClosestPosition(Vector3 position)
        {
            switch (boundsType)
            {
                case PlacementAreaType.Box: return boxBounds.ClosestPoint(position);
                case PlacementAreaType.Sphere: return sphereBounds.ClosestPoint(position);
                case PlacementAreaType.Custom: return customArea?.ClosestPosition(position) ?? position;
                case PlacementAreaType.Anywhere: return position;
                default: return Vector3.zero;
            }
        }

        public bool IsInside(Vector3 position)
        {
            switch (boundsType)
            {
                case PlacementAreaType.Box: return boxBounds.Contains(position);
                case PlacementAreaType.Sphere: return sphereBounds.Contains(position);
                case PlacementAreaType.Custom: return customArea?.IsInside(position) ?? true;
                case PlacementAreaType.Anywhere: return true;
                default: return false;
            }
        }

        /// <summary>
        /// Renders gizmo for current placement area.
        /// </summary>
        /// <param name="areaColor">Color for area gizmo.</param>
        public void RenderGizmos(Color areaColor)
        {
            switch (boundsType)
            {
                case PlacementAreaType.Custom: break;
                case PlacementAreaType.Anywhere: break;
                case PlacementAreaType.Box:
                    Gizmos.color = areaColor;
                    Gizmos.DrawWireCube(boxBounds.center, boxBounds.size);
                    break;

                case PlacementAreaType.Sphere:
                    Gizmos.color = areaColor;
                    Gizmos.DrawWireSphere(sphereBounds.center, sphereBounds.radius);
                    break;
            }
        }
    }
}