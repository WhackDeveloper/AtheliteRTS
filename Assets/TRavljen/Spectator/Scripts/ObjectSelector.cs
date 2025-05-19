using System;
using UnityEngine;

namespace Spectator
{

    /// <summary>
    /// Class for handling selection in scene by Raycasting with
    /// <see cref="IInputControl.MousePosition"/>. This class
    /// will handle select and double click events for the
    /// <see cref="SpectatorPlayer"/>.
    /// </summary>
    class ObjectSelector
    {

        #region Public Members

        public Action<Collider> OnObjectSelect;
        public Action<Collider> OnObjectDoubleClick;

        #endregion

        #region Private Members

        /// <summary>
        /// Specifies the handler for double click control.
        /// </summary>
        private DoubleClickHandler _doubleClickHandler = new DoubleClickHandler();

        private IInputControl _inputControl;

        private SpectatorPlayer _player;

        #endregion

        #region Lifecycle

        public ObjectSelector(SpectatorPlayer player, IInputControl inputControl)
        {
            _player = player;
            _inputControl = inputControl;
        }

        public void BindActions()
        {
            _inputControl.OnSelectActionPressed += OnSelectActionPressed;
        }

        public void UnbindActions()
        {
            _inputControl.OnSelectActionPressed -= OnSelectActionPressed;
        }

        #endregion

        private void OnSelectActionPressed()
        {
            // Check if selection is enabled, if cursor is visible
            // and if spectator is following a target.
            if (!_player.SelectionOptions.Enabled ||
                _player.FollowTarget ||
                _player.Options.LockCursor) return;

            var mousePos = _inputControl.MousePosition;
            Ray ray = _player.TargetCamera.ScreenPointToRay(mousePos);

            var previousSelection = _player.SelectedObject;

            if (Physics.Raycast(
                ray,
                out RaycastHit hit,
                _player.SelectionOptions.MaxSelectDistance,
                _player.SelectionOptions.LayerMask))
            {
                // Check if primary mouse button is pressed for
                // the second time on the same object.
                if (_player.SelectionOptions.DoubleClickEnabled &&
                    _doubleClickHandler.HandleClick() &&
                    previousSelection != null &&
                    previousSelection == hit.collider)
                {
                    OnObjectDoubleClick?.Invoke(hit.collider);
                    _player.SetSelection(hit.collider);
                }
                // Check if other object was selected
                else if (_player.SelectedObject != hit.collider)
                {
                    OnObjectSelect?.Invoke(hit.collider);
                }
            }
            else
            {
                _player.CancelSelection();
            }
        }

    }

}