using UnityEngine;

namespace TRavljen.UnitSystem.Navigation
{
    #if UNITY_EDITOR
    using UnityEditor;
    using TRavljen.EditorUtility;
    internal static class GroundValidationManagerTools
    {
        [MenuItem("GameObject/TRavljen/UnitSystem/Ground Validation Manager")]
        public static void CreateRelationshipManagerInScene()
        {
            if (EditorTools.CreateObjectFromMenu<GroundValidationManager>("Ground Validation Manager", true))
            {
                Debug.Log("New ground validation manager created. Now add a component to it for specific navigation package.");
            }
        }
    }
    [CustomEditor(typeof(GroundValidationManager), true)]
    internal class GroundValidationManagerEditor : HiddenScriptPropertyEditor
    {
        public override void OnInspectorGUI()
        {
            EditorGUILayout.HelpBox("Singleton object for managing reference to the validator which implements IValidateWorldPositions.\n" +
                                    "This can to be implemented for navigating objects and their improved behaviour, but is not a requirement.\n \n" +
                                    "When using Unity's NavMesh system for navigation, add NavMeshPositionValidator component to this object.", MessageType.None);
            base.OnInspectorGUI();
        }
    }
    #endif
   
    /// <summary>
    /// Interface for validation world positions.
    /// </summary>
    public interface IValidateWorldPositions
    {
        /// <summary>
        /// Validates positions and filters out invalid ones.
        /// </summary>
        /// <param name="positions">The array of positions to validate</param>
        /// <returns>An array of valid positions</returns>
        Vector3[] ValidatePositions(Vector3[] positions);

        /// <summary>
        /// Validates position, ensuring its on valid ground.
        /// </summary>
        /// <param name="position">World position to validate</param>
        /// <param name="validPosition">Validated position</param>
        /// <returns>Returns true if position was validated; otherwise false.</returns>
        public bool ValidatePosition(Vector3 position, out Vector3 validPosition);

    }

    /// <summary>
    /// Manages ground validation, ensuring objects have valid world positions.
    /// </summary>
    [ExecuteAlways]
    public class GroundValidationManager : MonoBehaviour
    {

        #region Static context

        private static GroundValidationManager instance;

        public static GroundValidationManager Get() => instance;

        /// <summary>
        /// Gets or creates a singleton instance of the GroundValidationManager.
        /// </summary>
        /// <returns>The singleton instance.</returns>
        public static GroundValidationManager GetOrCreate()
        {
            if (instance == null)
            {
                // When in editor and not playing, find manager by object type.
                // OR for cases where GET is called before Awake of the singleton
                // present in Scene.
                instance = Object.FindFirstObjectByType<GroundValidationManager>();
            }

            // If still null because none was found in scene, create new one without a validator.
            if (instance == null)
                instance = SingletonHandler.Create<GroundValidationManager>("Ground Validation Manager");

            return instance;
        }

        #endregion

        #region Properties

        [Tooltip("Game object reference holding a script which implements IValidateWorldPositions interface.")]
        [SerializeField, RequiresType(typeof(IValidateWorldPositions))]
        private GameObject validatorObject;

        public IValidateWorldPositions Validator { get; private set; }
        private bool isValidatorSet = false;

        #endregion

        private void Awake()
        {
            if (instance == null)
            {
                instance = this;
            }
            else if (instance != this)
            {
                Debug.Log("Singleton already exists, new instance will be destroyed");
                
                #if UNITY_EDITOR
                if (gameObject.transform.childCount == 1)
                    Undo.DestroyObjectImmediate(gameObject);
                else 
                    Undo.DestroyObjectImmediate(this);
                #else
                    DestroyImmediate(this);
                #endif
            }

            // Retrieve the validator from game object by setting it again.
            SetValidator(validatorObject);
        }

        /// <summary>
        /// Sets the validator for position validation.
        /// </summary>
        /// <param name="validator">The validator to set.</param>
        public void SetValidator(IValidateWorldPositions validator)
        {
            this.Validator = validator;
            isValidatorSet = true;
        }

        /// <summary>
        /// Sets the validator using a GameObject reference.
        /// </summary>
        /// <param name="gameObject">The GameObject containing the validator component.</param>
        public void SetValidator(GameObject gameObject)
        {
            if (gameObject && gameObject.TryGetComponent(out IValidateWorldPositions validator))
            {
                validatorObject = gameObject;
                SetValidator(validator);
            }
        }

        /// <summary>
        /// Clears the currently assigned validator.
        /// </summary>
        public void ClearValidator()
        {
            Validator = null;
            isValidatorSet = false;
        }

        /// <summary>
        /// Validates an array of world positions using the assigned validator.
        /// </summary>
        /// <param name="positions">The positions to validate.</param>
        /// <returns>The validated positions.</returns>
        public Vector3[] ValidatePositions(Vector3[] positions)
        {
#if UNITY_EDITOR
            // In editor when not playing, interfaces are not serialized so TryGetComponent is used,
            // however when playing it should be working as expected.
            if (!Application.isPlaying &&
                validatorObject != null &&
                validatorObject.TryGetComponent(out IValidateWorldPositions validator))
            {
                return validator.ValidatePositions(positions);
            }
#endif

            return !isValidatorSet ? positions : Validator.ValidatePositions(positions);
        }

        /// <summary>
        /// Validates an array of world positions using the assigned validator.
        /// </summary>
        /// <param name="position">Position to validate</param>
        /// <param name="validPosition">Validated position</param>
        /// <returns>Returns true if position was validated; otherwise false.</returns>
        public bool ValidatePosition(Vector3 position, out Vector3 validPosition)
        {
#if UNITY_EDITOR
            // In editor when not playing, interfaces are not serialized so TryGetComponent is used,
            // however when playing it should be working as expected.
            if (!Application.isPlaying)
            {
                if (validatorObject != null && 
                    validatorObject.TryGetComponent(out IValidateWorldPositions validator))
                {
                    return validator.ValidatePosition(position, out validPosition);
                }
                
                if (TryGetComponent(out validator))
                {
                    return validator.ValidatePosition(position, out validPosition);
                }
            }
#endif

            // Check for no validator
            if (!isValidatorSet)
            {
                validPosition = position;
                return true;
            }

            return Validator.ValidatePosition(position, out validPosition);
        }

    }

}