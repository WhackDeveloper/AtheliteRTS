#if UNITY_EDITOR
using System.Collections.Generic;
using UnityEngine;
using TRavljen.UnitFormation.Formations;

namespace TRavljen.UnitFormation.Editor
{
    using Placement;
    using UnityEditor;
    
    [System.Serializable]
    public enum FormationType
    {
        Ring, Circle, ComputedCircle, Cone, Line, Rectangle, RectangleBorder, Triangle, TriangleBorder
    }

    [System.Serializable]
    public enum GroundDetectionType
    {
        Disabled, NavMesh, Raycast
    }

    [ExecuteInEditMode]
    public class EditorFormationPlacement : MonoBehaviour
    {

        #region Properties

        private const float defaultSpacing = 2;

        [Tooltip("Units to move in formation")]
        [SerializeField]
        private List<Transform> units = new List<Transform>();

        [SerializeField]
        private FormationType type;

        [Tooltip("When enabled, random offset will be applied to the units positions.")]
        [SerializeField]
        private bool applyNoise = false;

        [Tooltip("Specifies the the max offset a unit positions can have.")]
        [SerializeField, Range(0.1f, 100f)]
        private float noiseScale = .2f;

        [Tooltip("When placing units on ground, it is convenient to use this feature " +
            "to position them on it, instead of doing it by hand. " +
            "\n\nYou can use Unity's 'NavMesh' or using a 'Raycast' with a specified 'LayerMask'")]
        [SerializeField]
        GroundDetectionType groundDetectionType = GroundDetectionType.NavMesh;

        [Tooltip("Specifies layer for detecting ground for unit position")]
        [SerializeField]
        LayerMask groundMask = ~0;

        [Tooltip("Maximal range allowed for ground detection features.")]
        [SerializeField, Range(10, 1000)]
        float groundMaxDistance = 100;

        // Used in editor component.
        [Tooltip("Specifies the parent GameObject for automatic retrieval of unit GameObjects.")]
        [SerializeField]
        Transform target;

        [Tooltip("Specifies if gizmos are shown, instead of real-time unit updates.")]
        [SerializeField]
        private bool showGizmos = false;

        [Tooltip("Specifies the color in which Gizmo position spheres are rendered.")]
        [SerializeField]
        Color positionColor = Color.red;

        [Tooltip("Specifies the color in which Gizmo direction shapes are rendered.")]
        [SerializeField]
        Color directionColor = Color.yellow;

        [Tooltip("Specifies the sphere radius used on Gizmos for formation positions.")]
        [SerializeField, Range(0.1f, 15f)]
        float sphereRadius = 0.5f;

        // Accessible only in Edit mode.
        [Tooltip("Mesh used for showing the direction at the end of a direction line. Otherwise spheres are used.")]
        [SerializeField]
        private Mesh directionArrowMesh;

        /// <summary>
        /// Keeps a list of noise offsets, creating one for each formation run would
        /// change offset each frame and not save it.
        /// </summary>
        [SerializeField]
        private List<Vector3> noiseOffsets = new List<Vector3>(); // Serialized to persist.
        private float _noiseScale = 0;

        [SerializeField]
        private UnitFormationData formationPositions;

        private IFormation currentFormation;
        private List<Vector3> gizmoPositions = new List<Vector3>();

        public IGroundPositioner GroundPositioner = new NavMeshGroundPositioner();

        public UnitFormationData FormationPositions => formationPositions;

        #endregion

        #region Formations

        [SerializeField]
        internal RingFormation ringFormation = new RingFormation(defaultSpacing);

        [SerializeField]
        internal CircleFormation circleFormation = new CircleFormation(6, defaultSpacing);

        [SerializeField]
        internal ComputedCircleFormation computedCircleFormation = new ComputedCircleFormation(defaultSpacing);

        [SerializeField]
        internal ConeFormation coneFormation = new ConeFormation(defaultSpacing);

        [SerializeField]
        internal RectangleBorderFormation rectangleBorderFormation = new RectangleBorderFormation(defaultSpacing);

        [SerializeField]
        internal RectangleFormation rectangleFormation = new RectangleFormation(5, defaultSpacing);

        [SerializeField]
        internal LineFormation lineFormation = new LineFormation(defaultSpacing);

        [SerializeField]
        internal TriangleFormation triangleFormation = new TriangleFormation(defaultSpacing);

        [SerializeField]
        internal TriangleBorderFormation triangleBorderFormation = new TriangleBorderFormation(defaultSpacing);

        #endregion

        private void OnValidate()
        {
            if (!Mathf.Approximately(noiseScale, _noiseScale))
            {
                _noiseScale = noiseScale;
                UpdateNoise(true);
            }

            UpdateFormation();
        }

        private void Update()
        {
            if (Selection.activeGameObject == gameObject)
                ApplyCurrentUnitFormation(showGizmos);
        }

        public void AddUnit(Transform unit) => units.Add(unit);
        public void ClearUnits() => units.Clear();

        private void UpdateFormation()
        {
            SetUnitFormation(GetFormation());
        }

        private IFormation GetFormation()
        {
            switch (type)
            {
                case FormationType.Ring: return ringFormation;
                case FormationType.ComputedCircle: return computedCircleFormation;
                case FormationType.Circle: return circleFormation;
                case FormationType.Cone: return coneFormation;
                case FormationType.Line: return lineFormation;
                case FormationType.Rectangle: return rectangleFormation;
                case FormationType.RectangleBorder: return rectangleBorderFormation;
                case FormationType.Triangle: return triangleFormation;
                case FormationType.TriangleBorder: return triangleBorderFormation;
            }

            return null;
        }

        private void SetUnitFormation(IFormation formation)
        {
            currentFormation = formation;
            ApplyCurrentUnitFormation(showGizmos);
        }

        private void OnDrawGizmosSelected()
        {
            if (!showGizmos) return;

            // Draw position spheres
            Gizmos.color = positionColor;
            foreach (Vector3 position in gizmoPositions)
            {
                Gizmos.DrawSphere(position, sphereRadius);
            }

            float cubeLength = 5;
            Vector3 directionRotation = new Vector3(0, transform.rotation.eulerAngles.y, 0);
            Vector3 originPosition = transform.position;
            Vector3 endPosition = originPosition + Quaternion.Euler(0, formationPositions.FacingAngle, 0) * Vector3.forward * cubeLength;

            Gizmos.color = directionColor;
            
            // Draw arrow mesh if present
            if (directionArrowMesh)
            {
                Gizmos.DrawMesh(directionArrowMesh, endPosition, Quaternion.Euler(directionRotation), Vector3.one * 100);
                cubeLength -= 1;
            }
            // Otherwise draw spheres at start & end
            else
            {
                Gizmos.DrawSphere(originPosition, sphereRadius);
                Gizmos.DrawSphere(endPosition, sphereRadius);
            }

            Vector3 rawRotation = directionRotation;
            directionRotation.y += 90;
            Vector3 newOrigin = originPosition + Quaternion.Euler(rawRotation) * Vector3.forward * (cubeLength / 2);

            // Draw direction line
            Gizmos.matrix = Matrix4x4.TRS(newOrigin, Quaternion.Euler(directionRotation), Vector3.one);
            Gizmos.DrawCube(Vector3.zero, new Vector3(cubeLength, 0.3f, 0.3f));
        }

        public void ApplyCurrentUnitFormation(bool drawGizmos)
        {
            // Validate units, might have removed one from editor.
            for (int index = units.Count - 1; index >= 0; index--)
            {
                if (units[index] == null)
                    units.RemoveAt(index);
            }

            Vector3 startPosition = transform.position;

            gizmoPositions.Clear();

            float angle = transform.eulerAngles.y;

            var newPositions = FormationPositioner.GetAlignedPositions(
                units.Count, currentFormation, startPosition, angle);
            formationPositions = new UnitFormationData(newPositions, angle);

            if (applyNoise)
            {
                UpdateNoise(false);
            }

            Quaternion rotation = Quaternion.Euler(0, formationPositions.FacingAngle, 0);

            for (int index = 0; index < units.Count; index++)
            {
                Vector3 pos = formationPositions.UnitPositions[index];

                if (applyNoise)
                {
                    pos += noiseOffsets[index];
                }

                switch (groundDetectionType)
                {
                    case GroundDetectionType.Disabled: break;
                    case GroundDetectionType.NavMesh:
                        pos = GroundPositioner.PositionOnGround(pos, groundMaxDistance);
                        break;

                    case GroundDetectionType.Raycast:
                        UnitFormationHelper.TryMovePositionOnGround(pos, groundMaxDistance, out pos, groundMask);
                        break;
                }

                if (drawGizmos)
                {
                    gizmoPositions.Add(pos);
                }
                else if (units[index] == null)
                {
                    // This should generally not happen as they are cleared before this.
                    Debug.LogError("Unit was destroyed, cannot apply position to it. Please update the unit list.");
                }
                else
                {
                    units[index].SetPositionAndRotation(pos, rotation);
                }
            }
        }

        private void UpdateNoise(bool updateExisting)
        {
            if (updateExisting)
            {
                noiseOffsets.Clear();
            }

            int index = noiseOffsets.Count;
            while (index < units.Count)
            {
                noiseOffsets.Add(UnitFormationHelper.GetNoise(noiseScale));
                index++;
            }
        }
    }
}
#endif