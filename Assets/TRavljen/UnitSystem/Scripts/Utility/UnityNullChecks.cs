using UnityEngine;

namespace TRavljen.UnitSystem
{

    /// <summary>
    /// Provides extension methods for null checks on Unity objects, addressing issues 
    /// with Unity's special handling of the equality operator for destroyed objects.
    /// </summary>
    /// <remarks>
    /// Unity overrides the <c>==</c> operator for <see cref="UnityEngine.Object"/>. 
    /// This can cause unexpected behavior, such as a destroyed object appearing as non-null 
    /// when checked using <c>== null</c>. These methods ensure proper null checks.
    /// </remarks>
    public static class UnityObjectNullChecks
    {

        /// <summary>
        /// Converts a Unity object to a true <c>null</c> if it has been destroyed but not yet deallocated.
        /// </summary>
        /// <typeparam name="T">The type of the Unity object being checked.</typeparam>
        /// <param name="obj">The Unity object to check.</param>
        /// <returns>
        /// The object itself if it is not destroyed, or <c>null</c> if the object is destroyed.
        /// </returns>
        /// <remarks>
        /// This method ensures that destroyed Unity objects return <c>null</c>, 
        /// addressing Unity's custom behavior for the <c>==</c> operator.
        /// </remarks>
        private static T AsTrueNull<T>(this T obj) where T : class
        {
            if (obj is Object uo && uo == null)
                return null;
            return obj;
        }

        /// <summary>
        /// Determines whether the given Unity object is effectively null.
        /// </summary>
        /// <typeparam name="T">The type of the Unity object being checked.</typeparam>
        /// <param name="obj">The Unity object to check.</param>
        /// <returns>
        /// <c>true</c> if the object is <c>null</c> or has been destroyed; otherwise, <c>false</c>.
        /// </returns>
        public static bool IsNull<T>(this T obj) where T : class
        {
            return AsTrueNull(obj) == null;
        }

        /// <summary>
        /// Determines whether the given object is effectively null, 
        /// including destroyed Unity objects.
        /// </summary>
        /// <param name="obj">The object to check.</param>
        /// <returns>
        /// <c>true</c> if the object is <c>null</c> or has been destroyed; otherwise, <c>false</c>.
        /// </returns>
        public static bool IsNull(this object obj)
        {
            return AsTrueNull(obj) == null;
        }

        /// <summary>
        /// Determines whether the given object is not null, including checking for destroyed Unity objects.
        /// </summary>
        /// <param name="obj">The object to check.</param>
        /// <returns>
        /// <c>true</c> if the object is not null and has not been destroyed; otherwise, <c>false</c>.
        /// </returns>
        public static bool IsNotNull(this object obj)
        {
            return AsTrueNull(obj) != null;
        }

    }
}