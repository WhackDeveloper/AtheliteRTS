using System.Collections.Generic;
using UnityEngine;

namespace TRavljen.UnitSystem.Demo
{

    using Combat;

    /// <summary>
    /// System responsible for displaying visual feedback for damage dealt to units or objects.
    /// </summary>
    /// <remarks>
    /// The system uses a pooling mechanism to optimize performance by reusing damage visual elements.
    /// Damage visual elements must implement <see cref="IDamageVisualElement"/> to integrate with this system.
    /// </remarks>
    public class VisualDamageSystem : MonoBehaviour
    {

        #region Properties

        [SerializeField, Tooltip("Toggle damage visualization on or off.")]
        private bool showDamage = true;

        [SerializeField, Tooltip("Prefab for the visual damage element. Must implement IDamageVisualElement.")]
        [RequiresType(typeof(IDamageVisualElement))]
        private GameObject prefab;

        [SerializeField, Tooltip("Parent transform for spawned damage visual elements.")]
        private Transform elementContainer;

        [SerializeField, Tooltip("Toggle whether damage visual elements use screen-space coordinates.")]
        private bool useScreenSpace = true;

        [SerializeField, Tooltip("Camera to use for screen-space transformations.")]
        private Camera playerCamera;

        /// <summary>
        /// List of currently active damage visual elements.
        /// </summary>
        private readonly List<IDamageVisualElement> activeElements = new();

        /// <summary>
        /// Pool of reusable damage visual elements.
        /// </summary>
        private readonly Queue<IDamageVisualElement> elementPool = new();

        #endregion

        private void OnValidate() => SetupCamera();

        private void OnEnable()
        {
            SetupCamera();
            CombatEvents.Instance.OnHitpointDecreased.AddListener(OnHitpointDecreased);
        }

        private void OnDisable()
        {
            CombatEvents.Instance.OnHitpointDecreased.AddListener(OnHitpointDecreased);
        }

        private void SetupCamera()
        {
            if (playerCamera == null)
                playerCamera = Camera.main;
        }

        private void Update()
        {
            // Check for elements ready to be returned to the pool
            for (var index = activeElements.Count - 1; index >= 0; index--)
            {
                var element = activeElements[index];
                if (!element.IsReadyForPooling()) continue;
                
                elementPool.Enqueue(element);
                activeElements.RemoveAt(index);

                // Hide element to prevent lingering visuals
                element.gameObject.SetActive(false);
            }
        }

        /// <summary>
        /// Handles damage events by displaying visual feedback.
        /// </summary>
        /// <param name="health">The health component of the damaged object.</param>
        /// <param name="unitComponent">The unit component associated with the damage (optional).</param>
        /// <param name="damage">The amount of damage dealt.</param>
        private void OnHitpointDecreased(IHealth health, AUnitComponent _, int damage)
        {
            if (!showDamage) return;

            Vector3 position = health.Position;

            if (useScreenSpace)
                position = playerCamera.WorldToScreenPoint(position);

            IDamageVisualElement damageVisual;

            // Reuse from pool if available, otherwise instantiate a new one
            if (elementPool.Count > 0)
                damageVisual = elementPool.Dequeue();
            else
                damageVisual = Instantiate(prefab, position, Quaternion.identity, elementContainer)
                    .GetComponent<IDamageVisualElement>();

            damageVisual.PlayerCamera = playerCamera;
            damageVisual.ShowDamage(health, damage);
            activeElements.Add(damageVisual);
        }
    }

}