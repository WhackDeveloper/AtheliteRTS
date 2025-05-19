using UnityEngine;
using UnityEngine.AI;
namespace TRavljen.UnitFormation
{

    public abstract class AFormationUnit : MonoBehaviour, IFormationUnit
    {

        #region Properties

        [Tooltip("Speed at which the unit will rotate towards the formation " +
            "facing angle (degrees per second).")]
        [SerializeField]
        [Range(1, 5_000)]
        private float rotationSpeed = 100;

        [Tooltip("Specifies if rotating towards the facing angle is enabled. " +
            "Set this to 'false' if you wish to manually handle synced " +
            "rotation of units in rotation.")]
        public bool FacingRotationEnabled = true;

        //private NavMeshAgent agent;
        private float facingAngle = 0f;
        private bool faceOnDestination = false;

        public abstract bool IsWithinStoppingDistance { get; }

        protected abstract Vector3 destination { set; }
        
        #endregion

        #region Lifecycle

        private void Update()
        {
            // If unit is within its stopping distance, start rotating towards the facing angle of the formation.
            if (FacingRotationEnabled &&
                faceOnDestination &&
                IsWithinStoppingDistance)
            {
                float currentAngle = transform.rotation.eulerAngles.y;
                var newAngle = Mathf.MoveTowardsAngle(currentAngle, facingAngle, rotationSpeed * Time.deltaTime);

                transform.Rotate(Vector3.up, newAngle - currentAngle);

                if (Mathf.Approximately(facingAngle, newAngle))
                {
                    faceOnDestination = false;
                }
            }
        }

        #endregion

        public virtual void SetTargetDestination(Vector3 newTargetDestination, float newFacingAngle)
        {
            faceOnDestination = true;
            destination = newTargetDestination;
            facingAngle = newFacingAngle;
        }
    }

    /// <summary>
    /// Unit component for moving to target position
    /// with Unity's NavMesh system. It also faces the angle of
    /// the formation after it reaches its destination (if enabled).
    /// </summary>
    [RequireComponent(typeof(NavMeshAgent))]
    public class FormationUnit : AFormationUnit
    {

        #region Properties

        [SerializeField] NavMeshAgent agent;
     
        public override bool IsWithinStoppingDistance =>
            !agent.pathPending && agent.remainingDistance <= agent.stoppingDistance;

        protected override Vector3 destination {
            set
            {
                if (agent == null)
                    agent = GetComponent<NavMeshAgent>();

                agent.destination = value;
            }
        }

        #endregion

        #region Lifecycle

        private void Awake()
        {
            agent = GetComponent<NavMeshAgent>();
        }

        #endregion

    }

}