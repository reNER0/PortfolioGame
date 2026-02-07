using Assets.Scripts.Commands;
using DG.Tweening;
using System.Linq;
using UnityEngine;

public class WeaponReadyState : WeaponState
{
    private int damage = 10;

    private float maxDistance = 100f;

    private float _accumulatedTime = 0;

    private float _shotInterval = 0.1f;

    private bool _lastFire;

    private Tween _shootTween;


    public WeaponReadyState(WeaponController weaponController) : base(weaponController) { }


    public override void OnEnter()
    {
    }

    public override void OnExit()
    {
    }

    public override void OnInput(PlayerInputs playerInputs)
    {
        var direction = Tools.DirectionFromYawPitch(playerInputs.Yaw, playerInputs.Pitch);

        bool fireHeld = playerInputs.Fire;
        bool fireDown = fireHeld && !_lastFire;
        bool fireUp = !fireHeld && _lastFire;

        // --- VISUAL (local prediction) ---
        if (fireDown) OnStartShooting();
        if (fireUp) OnStopShooting();

        // --- SIMULATION (server authoritative) ---
        if (NetworkRepository.Current.IsServer)
        {
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

        var trail = Instantiate(_weaponController.TrailRendererPrefab, _weaponController.Weapon.weaponObject.muzzle.position, _weaponController.Weapon.weaponObject.muzzle.rotation);

        trail.transform.DOMove(hit.point, 0.05f).OnComplete(() => { Destroy(trail.gameObject, trail.time); });
    }

    private void Shot(PlayerInputs playerInputs)
    {
        var direction = Tools.DirectionFromYawPitch(playerInputs.Yaw, playerInputs.Pitch);

        var hitCollider = GetHit(_weaponController.Weapon.weaponObject.muzzle.position, direction);

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
        
        var shooterNetworkObject = NetworkRepository.Current.NetworkObjectById.First(x => x.Predictable == _weaponController.GetComponent<Predictable>());
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

    public override void OnUpdate()
    {

    }
}
