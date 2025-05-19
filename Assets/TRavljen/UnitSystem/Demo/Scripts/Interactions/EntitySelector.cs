using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

namespace TRavljen.UnitSystem.Demo
{

    /// <summary>
    /// Simple selection component responsible for delegating selected units to the
    /// <see cref="EntitySelectionManager"/> which is used by modules. Check it's
    /// documentation for more info.
    /// </summary>
    public class EntitySelector : MonoBehaviour
    {

        #region Properties

        [Tooltip("Specifies the player this component selects for.")]
        [SerializeField]
        private APlayer player;

        [Tooltip("Specifies the keys used for adding instead of replacing selected entities.")]
        [SerializeField]
        private KeyCode[] addToSelectionKeys = new[] { KeyCode.LeftShift, KeyCode.RightShift };

        [Tooltip("Max selection distance which will be used for detecting entities.")]
        [SerializeField]
        private float maxSelectionDistance = 1000;

        [Tooltip("Selection manager which belongs to the player.")]
        [SerializeField]
        private EntitySelectionManager entitySelection;

        /// <summary>
        /// Specifies the camera of the player.
        /// </summary>
        [SerializeField]
        private Camera playerCamera;

        /// <summary>
        /// Specifies layers used for detecting clicks on game objects.
        /// </summary>
        public LayerMask SelectionLayers = ~0;

        /// <summary>
        /// Specifies ground layer for resetting selection.
        /// </summary>
        public LayerMask GroundLayer = ~0;

        /// <summary>
        /// Specifies if the event system in present in scene.
        /// </summary>
        private bool usesEventSystem;

        /// <summary>
        /// Array used for entity hits using raycast. It supports up to 10 hits.
        /// </summary>
        private readonly RaycastHit[] allocatedHits = new RaycastHit[10];

        #endregion

        private void OnValidate()
        {
            if (entitySelection.IsNull())
            {
                entitySelection = Object.FindFirstObjectByType<EntitySelectionManager>();    
            }
            
            if (player.IsNull())
            {
                player = transform.GetComponentInParent<APlayer>();

                if (player == null)
                    transform.GetComponentInChildren<APlayer>();
            }
        }

        private void Start()
        {
            usesEventSystem = EventSystem.current != null;

            if (playerCamera == null)
                playerCamera = Camera.main;

            if (!usesEventSystem)
            {
                Debug.LogWarning("EventSystem is missing in the scene. EntitySelector cannot determine if mouse is hovering over UI.");
            }
        }

        // Update is called once per frame
        void Update()
        {
            if (usesEventSystem &&
                EventSystem.current.IsPointerOverGameObject())
            {
                return;
            }

            // Check for unit selection (left mouse click)
            if (Input.GetKeyDown(KeyCode.Mouse0))
            {
                HandlePrimaryMouseButtonDown();
            }
        }

        private void HandlePrimaryMouseButtonDown()
        {
            if (!TryHitEntity(out Entity hitEntity)) {
                entitySelection.ClearSelectedEntities();
                return;
            }

            if (IsHoldingDownAddToKey())
            {
                List<Entity> entities = new (entitySelection.SelectedEntities);

                if (entities.Contains(hitEntity))
                    entities.Remove(hitEntity);
                else
                    entities.Add(hitEntity);

                entitySelection.SetSelectedEntities(entities);
            }
            else
            {
                entitySelection.SetSelectedEntities(new() { hitEntity });
            }
        }

        /// <summary>
        /// Attempts to find an entity which mouse is hovering over.
        /// </summary>
        /// <param name="entity">Entity returned if hit.</param>
        /// <returns>Returns true if entity was hit, otherwise false.</returns>
        private bool TryHitEntity(out Entity entity)
        {
            Ray ray = playerCamera.ScreenPointToRay(Input.mousePosition);
            int hitCount = Physics.RaycastNonAlloc(ray, allocatedHits, maxSelectionDistance, SelectionLayers);

            for (int index = 0; index < hitCount; index++)
            {
                RaycastHit hit = allocatedHits[index];
                if (hit.transform.TryGetComponent(out entity))
                {
                    return true;
                }
            }

            entity = null;
            return false;
        }

        /// <summary>
        /// Returns true when any of the <see cref="addToSelectionKeys"/> is pressed.
        /// </summary>
        private bool IsHoldingDownAddToKey()
        {
            foreach (KeyCode key in addToSelectionKeys)
            {
                if (Input.GetKey(key))
                {
                    return true;
                }
            }

            return false;
        }

    }

}