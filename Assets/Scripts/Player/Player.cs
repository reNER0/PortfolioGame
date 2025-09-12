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
        PlayerStateMachine.OnInput(playerInputs);
    }


    private void OnCollisionEnter(Collision collision)
    {
        var car = collision.gameObject.GetComponent<Car>();

        if (car == null)
            return;

        var seat = car.GetNearestSeat(transform.position);

        if (seat == null)
            return;

        var carId = NetworkRepository.NetworkObjectById.First(x => x.Predictable == car).Id;
        var seatId = car.GetSeatId(seat);

        var jumpInCarCmd = new JumpInCarCmd(NetworkRepository.CurrentObjectId, carId, seatId);

        NetworkBus.OnPerformCommand?.Invoke(jumpInCarCmd);
        NetworkBus.OnCommandSendToServer?.Invoke(jumpInCarCmd);
    }
}
