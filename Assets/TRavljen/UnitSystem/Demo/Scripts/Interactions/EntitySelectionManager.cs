using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace TRavljen.UnitSystem.Demo
{

    /// <summary>
    /// Manages entity selection and highlights for integration with the UnitSystem.  
    /// This class serves as a reference manager for entity selection,  
    /// allowing external systems to observe and propagate changes to selected or highlighted entites.
    /// </summary>
    public class EntitySelectionManager : MonoBehaviour
    {

        #region Properties

        [SerializeField, HideInInspector]
        protected List<Entity> selectedEntities = new();

        [SerializeField, HideInInspector]
        protected List<Entity> highlightedEntities = new();

        [SerializeField, HideInInspector]
        protected Entity firstSelectedEntity = null;

        /// <summary>
        /// The first entity in the selection list, used as a primary reference.  
        /// Returns null if no entities are selected.
        /// </summary>
        public Entity FirstSelectedEntity => firstSelectedEntity;

        /// <summary>
        /// The list of currently selected entities.
        /// </summary>
        public List<Entity> SelectedEntities => selectedEntities;

        /// <summary>
        /// Invoked when the selection changes, passing the updated list of selected entities.
        /// </summary>
        public UnityEvent<List<Entity>> OnSelectionChange = new();

        /// <summary>
        /// Invoked when the highlighted entities change, passing the updated list of highlighted entities.
        /// </summary>
        public UnityEvent<List<Entity>> OnHighlightedChange = new();

        #endregion

        #region Lifecycle

        private void OnEnable()
        {
            // Making sure any entities that will be removed are removed from the selection.
            EntityEvents.Instance.OnEntityDestroy.AddListener(HandleEntityDestroy);
        }

        private void OnDisable()
        {
            EntityEvents.Instance.OnEntityDestroy.AddListener(HandleEntityDestroy);
        }

        private void HandleEntityDestroy(Entity entity)
        {
            if (selectedEntities.Contains(entity))
                DeselectEntity(entity);

            if (highlightedEntities.Contains(entity))
            {
                highlightedEntities.Remove(entity);
                OnHighlightedChange.Invoke(highlightedEntities);
            }
        }

        #endregion

        #region Selection

        /// <summary>
        /// Updates the list of highlighted entities and invokes the highlight change event.  
        /// </summary>
        /// <param name="entities">The new list of highlighted entities.</param>
        public virtual void SetHighlightedEntities(List<Entity> entities)
        {
            highlightedEntities.Clear();
            highlightedEntities.AddRange(entities);

            OnHighlightedChange.Invoke(highlightedEntities);
        }

        /// <summary>
        /// Updates the list of selected entities and invokes the selection change event.  
        /// Sets the first entity in the selection as <see cref="FirstSelectedEntity"/>.  
        /// </summary>
        /// <param name="entities">The new list of selected entities.</param>
        public virtual void SetSelectedEntities(List<Entity> entities)
        {
            selectedEntities.Clear();
            selectedEntities.AddRange(entities);

            if (entities.Count > 0)
                firstSelectedEntity = entities[0];
            else
                firstSelectedEntity = null;

            OnSelectionChange.Invoke(selectedEntities);
        }

        /// <summary>
        /// Clears the selection list and invokes the selection change event.
        /// </summary>
        public virtual void ClearSelectedEntities()
        {
            selectedEntities.Clear();
            OnSelectionChange.Invoke(selectedEntities);
        }

        /// <summary>
        /// Deselects a specific entity, removing it from the selection list.  
        /// If the deselected entity was the <see cref="FirstSelectedEntity"/>, resets the reference.
        /// </summary>
        /// <param name="entity">The entity to deselect.</param>
        public virtual void DeselectEntity(Entity entity)
        {
            selectedEntities.Remove(entity);

            if (firstSelectedEntity == entity)
                firstSelectedEntity = null;
        }

        #endregion
    }

}