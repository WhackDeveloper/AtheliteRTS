using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;

namespace TRavljen.Tooltip
{
  
    /// <summary>
    /// Manages base detection for hovering an object with cursor. Works for world objects with renderers and colliders
    /// as well as UI elements on Canvas. Hovered object can be accessed through <see cref="FocusedObject"/>.
    /// </summary>
    /// <remarks>
    /// There can only be one focused object at any time.
    /// </remarks>
    public class HoverObject : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
    {

        [Tooltip("Specifies if the this hovering object should be ignored if UI is between cursor and the object (UI overlap).")]
        [SerializeField] private bool ignoreOverlappingUI;

        /// <summary>
        /// Specifies if the cursor is over a game object. This is NOT updated when hovering over UI elements.
        /// </summary>
        private bool _isCursorInside;
    
        /// <summary>
        /// Called when the component starts. Validates the presence of the EventSystem.
        /// </summary>
        /// <remarks>
        /// If no `EventSystem` is found in the scene, mouse interactions with UI may not behave as expected.
        /// </remarks>
        protected virtual void Start()
        {
            if (EventSystem.current == null)
            {
                Debug.Log("Without `EventSystem` game object will be selectable when UI is over it.");
            }
        }
    
        /// <summary>
        /// Clears focus of the object, if it is the one focused.
        /// </summary>
        protected virtual void OnDisable() => ClearFocus();
    
        #region Canvas Interaction Handlers
    
        /// <summary>
        /// Called when the pointer enters the bounds of the UI element.
        /// </summary>
        /// <param name="data">Pointer event data.</param>
        public void OnPointerEnter(PointerEventData data) => GainFocus();
    
        /// <summary>
        /// Called when the pointer exits the bounds of the UI element.
        /// </summary>
        /// <param name="data">Pointer event data.</param>
        public void OnPointerExit(PointerEventData data) => ClearFocus();
    
        #endregion
    
        #region Game Object Interaction Handlers
    
        /// <summary>
        /// Called when the pointer enters the bounds of a 3D object.
        /// </summary>
        private void OnMouseEnter()
        {
            _isCursorInside = true;
            StartCoroutine(CursorInsideUpdate());
    
            if (!IsCursorOverUI())
                GainFocus();
        }
    
        /// <summary>
        /// Called when the pointer exits the bounds of a 3D object.
        /// </summary>
        private void OnMouseExit()
        {
            _isCursorInside = false;
            ClearFocus();
        }
    
        /// <summary>
        /// Ensures that the object which has cursor inside will gain or clear focus if
        /// a UI element gets between it and the cursor.
        /// </summary>
        private IEnumerator CursorInsideUpdate()
        {
            while (_isCursorInside)
            {
                if (IsCursorOverUI() == false && FocusedObject == null)
                {
                    GainFocus();
                }
                else if (IsCursorOverUI() && FocusedObject == this)
                {
                    ClearFocus();
                }
    
                yield return new WaitForEndOfFrame();
            }
        }

        /// <summary>
        /// Checks if cursor is over a UI element.
        /// </summary>
        private bool IsCursorOverUI()
        {
            if (ignoreOverlappingUI) return false;
            return EventSystem.current != null && EventSystem.current.IsPointerOverGameObject();
        }
    
        #endregion
    
        #region Focus
    
        /// <summary>
        /// Will give focus to this object.
        /// </summary>
        protected virtual void GainFocus()
        {
            SetFocusObject(this);
        }
    
        /// <summary>
        /// Will remove focus from this object if it is focused.
        /// </summary>
        protected virtual void ClearFocus()
        {
            if (FocusedObject == this)
                SetFocusObject(null);
        }
    
        #endregion
        
        #region Static Context
        
        /// <summary>
        /// Represents the currently hovered (focus) object.
        /// This is updated automatically when the cursor enters or exits an interactive element's bounds
        /// using <see cref="HoverObject"/> component.
        /// </summary>
        public static HoverObject FocusedObject { get; private set; }
        
        /// <summary>
        /// Updates the reference to the currently focused object. This is generally only updated by the
        /// <see cref="HoverObject"/> component itself and may be overridden once an object is hovered.
        /// </summary>
        /// <param name="obj">The component being hovered.</param>
        public static void SetFocusObject(HoverObject obj)
        {
            FocusedObject = obj;
        }
        
        #endregion
    }
}