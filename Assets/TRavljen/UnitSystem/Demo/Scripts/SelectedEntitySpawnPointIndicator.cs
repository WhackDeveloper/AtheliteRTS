using System;
using System.Collections.Generic;
using UnityEngine;

namespace TRavljen.UnitSystem.Demo
{
    public class SelectedEntitySpawnPointIndicator : MonoBehaviour
    {
        
        [SerializeField] private EntitySelectionManager selectorManager;

        [SerializeField] private GameObject targetIndicator;
        [SerializeField] private LineRenderer lineRenderer;

        [SerializeField] private Vector3 lineOffset = new(0f, 0.2f, 0f);

        private IEntity _currentEntityControl;

        private void Awake()
        {
            if (selectorManager == null)
                selectorManager = UnityEngine.Object.FindFirstObjectByType<EntitySelectionManager>();
            
            selectorManager.OnSelectionChange.AddListener(OnSelectionChanged);

            targetIndicator.SetActive(false);

            lineRenderer.positionCount = 2;
            lineRenderer.enabled = false;
        }

        private void OnValidate()
        {
            if (selectorManager == null)
                selectorManager = UnityEngine.Object.FindFirstObjectByType<EntitySelectionManager>();
        }

        private void Update()
        {
            if (_currentEntityControl.IsNotNull())
            {
                UpdateVisuals();
            }
        }

        private void OnSelectionChanged(List<Entity> selectedEntities)
        {
            if (selectedEntities.Count == 0)
            {
                SetCurrentSpawn(null);
                return;
            }

            foreach (var selectedEntity in selectedEntities)
            {
                var spawnControl = selectedEntity.UnitSpawn;
                if (spawnControl == null) continue;

                // Check for change
                if (ReferenceEquals(selectedEntity, _currentEntityControl))
                    return;

                SetCurrentSpawn(selectedEntity);
                UpdateVisuals();
                return;
            }
            
            SetCurrentSpawn(null);
        }

        private void SetCurrentSpawn(IEntity entity)
        {
            var isSet = entity != null;
            _currentEntityControl = entity;
            targetIndicator.SetActive(isSet);
            lineRenderer.enabled = isSet;
        }

        private void UpdateVisuals()
        {
            UpdateVisuals(
                _currentEntityControl.transform.position, 
                _currentEntityControl.UnitSpawn.GetControlPosition());
        }

        private void UpdateVisuals(Vector3 start, Vector3 end)
        {
            lineRenderer.SetPosition(0, start + lineOffset);
            lineRenderer.SetPosition(1, end + lineOffset);

            targetIndicator.transform.position = end;
        }
        
    }

}