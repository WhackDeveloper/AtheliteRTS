using System;
using UnityEngine;

namespace TRavljen.UnitSystem
{

    /// <summary>
    /// Represents a base script for player modules, providing a flexible framework for adding 
    /// custom functionality to players.
    /// </summary>
    public abstract class APlayerModule: MonoBehaviour
    {

        /// <summary>
        /// Reference to the owning player. This is automatically assigned when the module 
        /// is initialized and should not be modified directly.
        /// </summary>
        [SerializeField, HideInInspector]
        protected APlayer player;

        /// <summary>
        /// Indicates whether the module is currently enabled.
        /// </summary>
        public bool IsEnabled => isActiveAndEnabled;

        #region Lifecycle

        /// <summary>
        /// Called during the player's <c>Awake</c> lifecycle. Use this to initialize 
        /// module-specific data or state.
        /// </summary>
        protected virtual void Awake()
        {
            SetUpPlayerReference();
        }

        protected virtual void OnValidate()
        {
            SetUpPlayerReference();
        }

        private void SetUpPlayerReference()
        {
            if (player == null)
            {
                if (TryGetComponent(out player) == false)
                    player = GetComponentInParent<APlayer>();
            }
        }

        #endregion

    }

}