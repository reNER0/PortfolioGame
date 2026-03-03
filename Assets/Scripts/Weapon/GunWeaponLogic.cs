using Assets.Scripts.Commands;
using Assets.Scripts.Network.Commands;
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



    public GunWeaponLogic(GunWeapon weapon, GunModel gunModel, Player player)
    {
        this.weapon = weapon;
        this.weaponModel = gunModel;
        this.player = player;
    }

    public void Attack(PlayerInputs playerInputs)
    {
        bool fireHeld = playerInputs.Fire;
        bool aimHeld = playerInputs.Aim;
        bool fireDown = fireHeld && !_lastFire;
        bool fireUp = !fireHeld && _lastFire;


        bool needToAim = fireHeld || aimHeld;

        if (needToAim && !player.WeaponController.IsAiming)
        {
            if (aimHeld && NetworkRepository.Current.IsCurrentClientOwnerOfObject(player))
            {
                PlayerCamera.Instance.Zoom(true);
            }

            player.WeaponController.Aim(true);
        }

        if (!needToAim && player.WeaponController.IsAiming)
        {
            PlayerCamera.Instance.Zoom(false);
            player.WeaponController.Aim(false);
        }


        if (player.WeaponController.IsAiming)
        {
            var direction = player.Direction;
            direction.y = 0;
            player.transform.forward = direction;
        }

        var canShoot = weapon.CanShoot();

        // Visual simulation
        if (fireDown && canShoot)
            StartStopVisualShooting(true);
        if (fireUp || !canShoot)
            StartStopVisualShooting(false);

        if (!canShoot)
            return;

        // Shots simulation
        if (fireDown)
        {
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

    private void StartStopVisualShooting(bool start) 
    {
        if (start)
            player.WeaponController.OnStartShooting();
        else
            player.WeaponController.OnStopShooting();

        if (!NetworkRepository.Current.IsServer)
            return;

        var playerObjectId = NetworkRepository.Current.NetworkObjectById.First(x => x.Predictable == player).Id;

        var startStopShootingCmd = new StartStopShootingCmd(playerObjectId, start);

        var shooter = NetworkRepository.Current.ConnectedClients.FirstOrDefault(x => x.ClientObjectId == playerObjectId);

        if (shooter == null) 
        {
            NetworkBus.OnCommandSendToClients(startStopShootingCmd);
            return;
        }

        NetworkBus.OnCommandSendToClientsExcept(startStopShootingCmd, shooter);
    }

    private void Shot(PlayerInputs playerInputs)
    {
        weapon.WasteAmmo();

        if (!NetworkRepository.Current.IsServer)
            return;

        var hitCollider = GetHit(weapon.weaponObject.muzzle.position, player.Direction);

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



    public bool NeedReload()
    {
        return weapon.CurrentAmmo <= 0;
    }

    public void OnReload()
    {
        weapon.Reload();
    }

    public void OnShowVisual()
    {
        var camera = Camera.main;


        if (!Physics.Raycast(weapon.weaponObject.muzzle.position, player.Direction, out var hit, weapon.weaponModel.range))
        {
            GameBus.OnBulletFX?.Invoke(new BulletFX
            {
                StartPosition = weapon.weaponObject.muzzle.position,
                EndPosition = weapon.weaponObject.muzzle.position + player.Direction * weapon.weaponModel.range
            });
            return;
        }


        GameBus.OnBulletFX?.Invoke(new BulletFX
        {
            StartPosition = weapon.weaponObject.muzzle.position,
            EndPosition = hit.point
        });
    }
}
