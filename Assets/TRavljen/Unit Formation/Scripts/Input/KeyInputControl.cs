using UnityEngine;

namespace TRavljen.UnitFormation
{

    /// <summary>
    /// Input control for formation placement using Unity's old Input System.
    /// </summary>
    public class KeyInputControl : AInputControl
    {

        [Tooltip("Specifies the key used for unit placement. " +
            "Default value is right mouse button.")]
        [SerializeField]
        private KeyCode placementKey = KeyCode.Mouse1;

        [Tooltip("Specifies the key used for canceling active selection.")]
        [SerializeField]
        private KeyCode cancelKey = KeyCode.Escape;

        public override Vector3 MousePosition => Input.mousePosition;

        private void Update()
        {
            if (Input.GetKeyDown(placementKey))
            {
                OnPlacementActionPress.Invoke();
            }
            else if (Input.GetKeyUp(placementKey))
            {
                OnPlacementActionRelease.Invoke();
            }
            else if (Input.GetKeyDown(cancelKey))
            {
                OnPlacementActionCancel.Invoke();

            }
        }
    }

}