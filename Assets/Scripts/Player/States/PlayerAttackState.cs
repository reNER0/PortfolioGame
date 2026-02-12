using Assets.Scripts.Commands;
using System;
using System.Collections.Generic;
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

        _player.Animator.SetBool("Attack", true);
        _player.Animator.applyRootMotion = true;

        lastComboTime = Time.time;

        _player.WeaponController.Weapon.weaponObject.MeleeTrigger.OnMeleeHit += OnHit;
        _player.WeaponController.PlayerAnimationEvents.OnAnimationCombo += OnCombo;
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
        if (currentComboHits.Contains(damagable))
            return;

        currentComboHits.Add(damagable);

        damagable.Damage(_player.WeaponController.Weapon.weaponModel.damage);

        var damagablePlayer = ((Component)damagable).GetComponent<Player>();

        if (damagablePlayer == null)
            return;

        damagablePlayer.PlayerStateMachine.ChangeState(new PlayerKnockedState(damagablePlayer, damagablePlayer.transform.position - _player.transform.position));
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

        _player.Animator.SetBool("Attack", false);
        _player.Animator.applyRootMotion = false;

        _player.WeaponController.Weapon.weaponObject.MeleeTrigger.OnMeleeHit -= OnHit;
        _player.WeaponController.PlayerAnimationEvents.OnAnimationCombo -= OnCombo;
    }
}
