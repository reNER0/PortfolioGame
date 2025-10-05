using System.Linq;
using UnityEngine;

public class Player : PhysicsObject
{
    public PlayerStateMachine PlayerStateMachine { get; private set; }
    public Animator Animator { get; private set; }


    [SerializeField]
    private float jumpForce;
    [SerializeField]
    private float maxSpeed;
    [SerializeField]
    private float maxAcceleration;
    [SerializeField]
    private float airAcceleration;
    [SerializeField]
    private AnimationCurve reverseAccelerationMultiplierCurve;
    [SerializeField]
    private float additiveGravity;

    [SerializeField]
    private bool useIK;
    [SerializeField]
    private float legsIKOffset;
    [SerializeField]
    private LayerMask walkableLayerMask;

    [SerializeField]
    private float springDistance;
    [SerializeField]
    private float springForce;
    [SerializeField]
    private float springDamping;

    public float MaxSpeed => maxSpeed;
    public float MaxAcceleration => maxAcceleration;
    public float AirAcceleration => airAcceleration;
    public float JumpForce => jumpForce;
    public float SpringDistance => springDistance;
    public float SpringForce => springForce;
    public float SpringDamping => springDamping;
    public AnimationCurve ReverseAccelerationMultiplierCurve => reverseAccelerationMultiplierCurve;
    public float AdditiveGravity => additiveGravity;
    public bool UseIK => useIK;
    public float LegsIKOffset => legsIKOffset;
    public LayerMask WalkableLayerMask => walkableLayerMask;



    private void Awake()
    {
        Animator = GetComponentInChildren<Animator>();

        PlayerStateMachine = Animator.gameObject.AddComponent<PlayerStateMachine>();
        PlayerStateMachine.ChangeState(new PlayerWalkingState(this));
    }


    // same as FixedUpdate
    public override void Input(PlayerInputs playerInputs)
    {
        base.Input(playerInputs);

        PlayerStateMachine.OnInput(playerInputs);
    }


    private void OnCollisionEnter(Collision collision)
    {
        PlayerStateMachine.OnCollisionEnter(collision);
    }


    public override void UpdateState(PredictableState state)
    {
        if (PlayerStateMachine.currentState.GetType() == typeof(PlayerDrivingState))
            return;

        var serverState = state as RigidbodyState;

        if (serverState == null)
        {
            Debug.LogError("Error while applying server predictable state!");
            return;
        }


        if (!NetworkRepository.IsCurrentClientOwnerOfObject(this))
        {
            Rigidbody.MovePosition(serverState.Position);
            Rigidbody.MoveRotation(serverState.Rotation);
            Rigidbody.velocity = serverState.Velocity;
            Rigidbody.angularVelocity = serverState.RotationVelocity;
            return;
        }


        serverStateTransform.position = serverState.Position;
        serverStateTransform.rotation = serverState.Rotation;


        var localState = States.FirstOrDefault(x => x?.Tick == serverState.Tick);

        if (localState == null)
        {
            //Debug.LogWarning($"Client received server state with tick {serverState.Tick}, " +
            //    $"but clients last state tick was {States.Where(x => x != null)?.OrderByDescending(x => x.Tick).First().Tick}");
            return;
        }

        var error = (serverState.Position - (localState as RigidbodyState).Position).magnitude;

        if (error >= NetworkSettings.MaximumError)
        {
            Reconcilate(serverState);

            return;
        }
    }
}
