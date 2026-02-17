using Assets.Scripts.Commands;
using Assets.Scripts.Network.Commands;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class PlayerAttackState : PlayerStandingState
{
    private float inputDelayTime = 1/3f;

    private float lastComboTime;

    private List<IDamagable> currentComboHits = new();

    private bool continueCombo;

    public PlayerAttackState(Player player) : base(player) { }


    public override void OnEnter()
    {
        base.OnEnter();

        SetAttackAnimation(true);

        lastComboTime = Time.time;

        _player.WeaponController.Weapon.weaponObject.MeleeTrigger.OnMeleeHit += OnHit;
        _player.WeaponController.PlayerAnimationEvents.OnAnimationCombo += OnCombo;
    }


    private void SetAttackAnimation(bool state) 
    {
        string animationName = "Attack";

        _player.Animator.SetBool(animationName, state);

        if (!NetworkRepository.Current.IsServer)
            return;

        var playerObjectId = NetworkRepository.Current.NetworkObjectById.First(x => x.Predictable == _player).Id;

        var attackAnimationCmd = new SetPlayerAnimatorBoolCmd(playerObjectId, animationName, state);

        var networkClient = NetworkRepository.Current.ConnectedClients.FirstOrDefault(x => x.ClientObjectId == playerObjectId);

        if (networkClient == null)
            return;

        NetworkBus.OnCommandSendToClientsExcept(attackAnimationCmd, networkClient);
    }

    public override void OnInput(PlayerInputs playerInputs)
    {
        base.OnInput(playerInputs);

        if (!playerInputs.Fire)
            return;

        if (lastComboTime + inputDelayTime > Time.time)
            return;

        continueCombo = true;
    }

    private void OnHit(IDamagable damagable) 
    {
        if (!NetworkRepository.Current.IsServer)
            return;

        if (currentComboHits.Contains(damagable))
            return;

        currentComboHits.Add(damagable);

        damagable.Damage(_player.WeaponController.Weapon.weaponModel.damage);

        var damagablePlayer = ((Component)damagable).GetComponent<Player>();

        if (damagablePlayer == null)
            return;

        damagablePlayer.PlayerStateMachine.ChangeState(new PlayerKnockedState(damagablePlayer, damagablePlayer.transform.position - _player.transform.position));

        var networkObject = NetworkRepository.Current.NetworkObjectById.First(x => x.Predictable == damagablePlayer);
        var hitCmd = new HitCmd(networkObject.Id, _player.WeaponController.Weapon.weaponModel.damage);

        var shooterNetworkObject = NetworkRepository.Current.NetworkObjectById.First(x => x.Predictable == _player);
        var shooterClient = NetworkRepository.Current.ConnectedClients.First(x => x.ClientObjectId == shooterNetworkObject.Id);

        NetworkBus.OnCommandSendToClient?.Invoke(hitCmd, shooterClient);

        var hitClient = NetworkRepository.Current.ConnectedClients.FirstOrDefault(x => x.ClientObjectId == networkObject.Id);
        if (hitClient == null)
            return;

        NetworkBus.OnCommandSendToClient?.Invoke(hitCmd, hitClient);
    }

    private void OnCombo()
    {
        currentComboHits.Clear();

        if (continueCombo)
        {
            lastComboTime = Time.time;
            continueCombo = false;
            return;
        }

        _player.PlayerStateMachine.ChangeState(new PlayerWalkingState(_player, 0));
    }

    public override void OnExit() 
    {
        base.OnExit();

        SetAttackAnimation(false);

        _player.WeaponController.Weapon.weaponObject.MeleeTrigger.OnMeleeHit -= OnHit;
        _player.WeaponController.PlayerAnimationEvents.OnAnimationCombo -= OnCombo;
    }
}