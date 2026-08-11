using Assets.Scripts.Commands;
using Assets.Scripts.Network;
using System;
using System.Linq;
using Unity.Mathematics;
using Unity.VisualScripting;
using UnityEngine;

public class Player : PhysicsObject, IDamagable, IHealth
{
    public PlayerSound PlayerSound { get; private set; }
    public WeaponController WeaponController { get; private set; }
    public PlayerStateMachine PlayerStateMachine { get; private set; }
    public Animator Animator { get; private set; }

    private int health = 100;
    private int maxHealth = 100;

    public Vector3 Direction { get; private set; }


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

    public event Action<int> HealthChanged;

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
        PlayerStateMachine.ChangeState(new PlayerWalkingState(this, 0));

        WeaponController = GetComponent<WeaponController>();
        PlayerSound = GetComponent<PlayerSound>();
    }


    // same as FixedUpdate
    public override void Input(PlayerInputs playerInputs)
    {
        base.Input(playerInputs);

        PlayerStateMachine.OnInput(playerInputs);

        if (!NetworkRepository.Current.IsServer && !NetworkRepository.Current.IsCurrentClientOwnerOfObject(this))
            return;

        Direction = Tools.DirectionFromYawPitch(playerInputs.Yaw, playerInputs.Pitch);

        if (PlayerStateMachine.currentState.GetType() != typeof(PlayerWalkingState))
            return;

        WeaponController.Input(playerInputs);
    }


    void OnAnimatorMove()
    {
        if (PlayerStateMachine.currentState.GetType() == typeof(PlayerDrivingState))
            return;

        Rigidbody.MovePosition(Rigidbody.position + Animator.deltaPosition);
        //transform.position = transform.position + Animator.deltaPosition;
    }

    public override PredictableState GetState()
    {
        Tools.YawPitchFromDirection(Direction, out var yaw, out var pitch);

        return new PlayerSyncState(InputProcessor.ProcessTick,
            Rigidbody.position,
            Rigidbody.velocity,
            Rigidbody.rotation,
            Rigidbody.angularVelocity,
            lastAppliedInputs,
            health,
            yaw,
            pitch
            );
    }


    private void OnCollisionEnter(Collision collision)
    {
        PlayerStateMachine.OnCollisionEnter(collision);
    }

    protected override void FixedUpdate()
    {
        if (PlayerStateMachine.currentState.GetType() == typeof(PlayerDrivingState))
            return;

        //var serverState = lastServerState as RigidbodyState;
        var interpolateTick = NetworkTime.CurrentTick - NetworkSettings.MaximumPingInTicks;
        var serverState = ServerStates.FirstOrDefault(x => x != null && x.Tick == interpolateTick) as PlayerSyncState;

        if (serverState == null)
        {
            //Debug.LogError("Error while applying server predictable state!");
            return;
        }

        if (health != serverState.Health)
        {
            health = serverState.Health;
            HealthChanged?.Invoke(health);
        }

        if (!NetworkRepository.Current.IsCurrentClientOwnerOfObject(this))
        {
            //Rigidbody.MovePosition(serverState.Position);
            //Rigidbody.MoveRotation(serverState.Rotation);
            //Rigidbody.velocity = serverState.Velocity;
            //Rigidbody.angularVelocity = serverState.RotationVelocity;
            Direction = Tools.DirectionFromYawPitch(serverState.Yaw, serverState.Pitch);
        //    return;
        }


        serverStateTransform.position = serverState.Position;
        serverStateTransform.rotation = serverState.Rotation;


        var localState = LocalStates.FirstOrDefault(x => x?.Tick == serverState.Tick);

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

        if (NetworkRepository.Current.IsCurrentClientOwnerOfObject(this))
        {
            SmoothSync(localState as RigidbodyState, serverState, NetworkSettings.ClientSidePredictionType);
            return;
        }

        SmoothSync(localState as RigidbodyState, serverState, NetworkSettings.ErrorCorrectionType);
    }

    public void Damage(int damage)
    {
        health -= damage;

        health = Math.Max(0, health);

        HealthChanged?.Invoke(health);

        if (health > 0)
            return;

        var killCmd = new KillPlayerCmd(NetworkRepository.Current.NetworkObjectById.First(x => x.Predictable == this).Id);
        NetworkBus.OnPerformCommand?.Invoke(killCmd);
    }

    public int GetHealth()
    {
        return health;
    }

    public int GetMaxHealth()
    {
        return maxHealth;
    }
}
