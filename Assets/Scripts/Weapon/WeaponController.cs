using Assets.Scripts.Commands;
using DG.Tweening;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;

public class WeaponController : MonoBehaviour
{
    public TrailRenderer TrailRendererPrefab;

    public AudioClip AudioClip;
    public AudioSource AudioSource { get; private set; }
    public WeaponStateMachine WeaponStateMachine { get; private set; }
    public PlayerAnimationEvents PlayerAnimationEvents { get; private set; }
    public Animator Animator { get; private set; }

    public Transform WeaponSocket;

    public Weapon Weapon;

    public WeaponModel WeaponModel;

    private int damage = 10;

    private float maxDistance = 100f;

    private float _accumulatedTime = 0;

    private float _shotInterval = 0.1f;

    public float AimTime = 1;

    private bool _lastFire;

    private Tween _shootTween;

    private bool isReloading;

    private bool isPreparing;

    private bool isShowingWeapon = true;

    private float lastAimTime;

    private void Awake()
    {
        Animator = GetComponentInChildren<Animator>();

        PlayerAnimationEvents = Animator.AddComponent<PlayerAnimationEvents>();

        Weapon = new Weapon(WeaponModel, WeaponSocket);

        //WeaponStateMachine = Animator.gameObject.AddComponent<WeaponStateMachine>();
        //WeaponStateMachine.ChangeState(new WeaponReadyState(this));

        AudioSource = gameObject.AddComponent<AudioSource>();
    }

    private void Start()
    {
        if (NetworkRepository.Current.IsCurrentClientOwnerOfObject(GetComponent<Predictable>()))
            PlayerInputController.inputSystem.Inputs.Reload.performed += OnReload;

        PlayerAnimationEvents.OnReloadAnimationFinished += OnReloadAnimationFinished;
        PlayerAnimationEvents.OnPrepareAnimationFinished += OnPrepareAnimationFinished;
    }
    private void OnDestroy()
    {
        PlayerInputController.inputSystem.Inputs.Reload.performed -= OnReload;

        PlayerAnimationEvents.OnReloadAnimationFinished -= OnReloadAnimationFinished;
        PlayerAnimationEvents.OnPrepareAnimationFinished -= OnPrepareAnimationFinished;
    }

    private void OnReload(InputAction.CallbackContext context)
    {
        isReloading = true;
        OnReloadAnimationStarted();
        //WeaponStateMachine.ChangeState(new WeaponReloadingState(this));
    }

    // same as FixedUpdate
    public void Input(PlayerInputs playerInputs)
    {
        var direction = Tools.DirectionFromYawPitch(playerInputs.Yaw, playerInputs.Pitch);

        bool fireHeld = playerInputs.Fire;
        bool fireDown = fireHeld && !_lastFire;
        bool fireUp = !fireHeld && _lastFire;

        if (!fireDown)
            return;

        var player = GetComponent<Player>();

        if (player.PlayerStateMachine.currentState.GetType() == typeof(PlayerAttackState))
            return;

        player.PlayerStateMachine.ChangeState(new PlayerAttackState(player));


        return;
        // --- VISUAL (local prediction) ---
        if (fireDown) OnStartShooting();
        if (fireUp) OnStopShooting();

        // --- SIMULATION (server authoritative) ---
        if (fireDown)
        {
            _accumulatedTime = 0f;
            Shot(playerInputs); // первый выстрел сразу
        }
        else if (fireHeld)
        {
            _accumulatedTime += Time.fixedDeltaTime;

            while (_accumulatedTime >= _shotInterval)
            {
                _accumulatedTime -= _shotInterval;
                Shot(playerInputs);
            }
        }

        _lastFire = fireHeld;
    }

    private void OnStartShooting()
    {
        ShowVisualTrail();

        _shootTween = DOVirtual.DelayedCall(
            _shotInterval,
            ShowVisualTrail
        )
        .SetLoops(-1, LoopType.Restart)
        .SetUpdate(UpdateType.Normal);
    }

    private void OnStopShooting()
    {
        if (_shootTween != null)
        {
            _shootTween.Kill();
            _shootTween = null;
        }
    }

    private void ShowVisualTrail()
    {
        var camera = Camera.main;

        if (!Physics.Raycast(camera.transform.position, camera.transform.forward, out var hit, 50))
            return;

        var trail = Instantiate(TrailRendererPrefab, Weapon.weaponObject.muzzle.position, Weapon.weaponObject.muzzle.rotation);

        trail.transform.DOMove(hit.point, 0.05f).OnComplete(() => { Destroy(trail.gameObject, trail.time); });
    }

    private void Shot(PlayerInputs playerInputs)
    {
        lastAimTime = Time.time;

        Weapon.ammoCount--;

        if (!NetworkRepository.Current.IsServer)
            return;

        var direction = Tools.DirectionFromYawPitch(playerInputs.Yaw, playerInputs.Pitch);

        var hitCollider = GetHit(Weapon.weaponObject.muzzle.position, direction);

        if (hitCollider == null)
            return;

        var damagable = hitCollider.GetComponent<IDamagable>();

        if (damagable == null)
            return;

        damagable.Damage(damage);

        var predictable = hitCollider.GetComponent<Predictable>();

        if (predictable == null)
            return;

        var networkObject = NetworkRepository.Current.NetworkObjectById.First(x => x.Predictable == predictable);
        var hitCmd = new HitCmd(networkObject.Id, damage);

        var shooterNetworkObject = NetworkRepository.Current.NetworkObjectById.First(x => x.Predictable == GetComponent<Predictable>());
        var shooterClient = NetworkRepository.Current.ConnectedClients.First(x => x.ClientObjectId == shooterNetworkObject.Id);

        NetworkBus.OnCommandSendToClient?.Invoke(hitCmd, shooterClient);

        var hitClient = NetworkRepository.Current.ConnectedClients.FirstOrDefault(x => x.ClientObjectId == networkObject.Id);
        if (hitClient == null)
            return;

        NetworkBus.OnCommandSendToClient?.Invoke(hitCmd, hitClient);


        Collider GetHit(Vector3 origin, Vector3 direction)
        {
            Physics.Raycast(origin, direction, out var hit, maxDistance);

            return hit.collider;
        }
    }


    public void OnReloadAnimationStarted()
    {
        Animator.SetBool("IsReloading", true);
    }
    public void OnReloadAnimationFinished()
    {
        Animator.SetBool("IsReloading", false);
        OnReloadFinished();
    }

    private void OnReloadFinished()
    {
        Weapon.OnReload();
        OnPrepare();
    }

    private void OnPrepare()
    {
        OnPrepareAnimationStarted();
    }

    public void OnPrepareAnimationStarted()
    {
        Animator.SetBool("IsPreparing", true);
    }
    public void OnPrepareAnimationFinished()
    {
        Animator.SetBool("IsPreparing", false);
        OnPrepareFinished();
    }

    private void OnPrepareFinished()
    {
        isPreparing = false;
    }


    public bool IsUsingLeftHand() 
    {
        return !isReloading && !isPreparing && isShowingWeapon;
    }

    public bool IsUsingRightHand()
    {
        return isShowingWeapon;
    }

    public bool IsAiming()
    {
        return lastAimTime + AimTime > Time.time;
    }
}