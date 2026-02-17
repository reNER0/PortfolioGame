using Assets.Scripts.Commands;
using Assets.Scripts.Weapon;
using DG.Tweening;
using System.Linq;
using UnityEngine;

public class GunWeaponLogic : IWeaponLogic
{
    private Player player;

    private GunWeapon weapon;
    private GunModel weaponModel;

    private bool _lastFire;

    private float _accumulatedTime;

    private Vector3 direction;

    private Tween _shootTween;


    public GunWeaponLogic(GunWeapon weapon, GunModel gunModel, Player player)
    {
        this.weapon = weapon;
        this.weaponModel = gunModel;
        this.player = player;
    }

    public void Attack(PlayerInputs playerInputs)
    {
        direction = Tools.DirectionFromYawPitch(playerInputs.Yaw, playerInputs.Pitch);

        bool fireHeld = playerInputs.Fire;
        bool fireDown = fireHeld && !_lastFire;
        bool fireUp = !fireHeld && _lastFire;


        // --- VISUAL (local prediction) ---
        if (fireDown) 
            OnStartShooting();
        if (fireUp) 
            OnStopShooting();

        // --- SIMULATION (server authoritative) ---
        if (fireDown)
        {
            _accumulatedTime = 0f;
            Shot(playerInputs); // первый выстрел сразу
        }
        else if (fireHeld)
        {
            _accumulatedTime += Time.fixedDeltaTime;

            while (_accumulatedTime >= weaponModel.fireRate)
            {
                _accumulatedTime -= weaponModel.fireRate;
                Shot(playerInputs);
            }
        }

        _lastFire = fireHeld;
    }


    private void Shot(PlayerInputs playerInputs)
    {
        //lastAimTime = Time.time;

        weapon.currentAmmo--;

        if (!NetworkRepository.Current.IsServer)
            return;

        var direction = Tools.DirectionFromYawPitch(playerInputs.Yaw, playerInputs.Pitch);

        var hitCollider = GetHit(weapon.weaponObject.muzzle.position, direction);

        if (hitCollider == null)
            return;

        var damagable = hitCollider.GetComponent<IDamagable>();

        if (damagable == null)
            return;

        damagable.Damage(weapon.weaponModel.damage);

        var predictable = hitCollider.GetComponent<Predictable>();

        if (predictable == null)
            return;

        var networkObject = NetworkRepository.Current.NetworkObjectById.First(x => x.Predictable == predictable);
        var hitCmd = new HitCmd(networkObject.Id, weapon.weaponModel.damage);

        var shooterNetworkObject = NetworkRepository.Current.NetworkObjectById.First(x => x.Predictable == player);
        var shooterClient = NetworkRepository.Current.ConnectedClients.FirstOrDefault(x => x.ClientObjectId == shooterNetworkObject.Id);
        if (shooterClient != null)
            NetworkBus.OnCommandSendToClient?.Invoke(hitCmd, shooterClient);

        var hitClient = NetworkRepository.Current.ConnectedClients.FirstOrDefault(x => x.ClientObjectId == networkObject.Id);
        if (hitClient != null)
            NetworkBus.OnCommandSendToClient?.Invoke(hitCmd, hitClient);

        NetworkBus.OnPerformCommand?.Invoke(hitCmd);

        Collider GetHit(Vector3 origin, Vector3 direction)
        {
            Physics.Raycast(origin, direction, out var hit, weapon.weaponModel.range);

            return hit.collider;
        }
    }

    private void OnStartShooting()
    {
        OnStopShooting();

        ShowVisualTrail();

        _shootTween = DOVirtual.DelayedCall(
            weaponModel.fireRate,
            ShowVisualTrail
        )
        .SetLoops(-1, LoopType.Restart)
        .SetUpdate(UpdateType.Normal);
    }

    private void OnStopShooting()
    {
        _shootTween?.Kill();
    }


    private void ShowVisualTrail()
    {
        var camera = Camera.main;

        if (!Physics.Raycast(weapon.weaponObject.muzzle.position, direction, out var hit, weapon.weaponModel.range)) 
            return;

        GameBus.OnBulletFX?.Invoke(new BulletFX
        {
            StartPosition = weapon.weaponObject.muzzle.position,
            EndPosition = hit.point
        });
    }

    public bool NeedReload()
    {
        return weapon.currentAmmo <= 0;
    }

    public void OnReload()
    {
        weapon.currentAmmo = weaponModel.ammoCapacity;
    }
}
