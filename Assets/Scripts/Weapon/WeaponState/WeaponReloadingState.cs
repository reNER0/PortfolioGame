using Assets.Scripts.Commands;
using DG.Tweening;
using System;
using System.Linq;
using UnityEngine;

public class WeaponReloadingState : WeaponState
{
    private float reloadEndTime;

    public WeaponReloadingState(WeaponController weaponController) : base(weaponController) { }


    public override void OnEnter()
    {
        reloadEndTime = Time.time + _weaponController.Weapon.weaponModel.reloadTime;

        _weaponController.Animator.SetTrigger("Reload");
    }

    public override void OnExit()
    {
    }

    public override void OnInput(PlayerInputs playerInputs)
    {
    }

    public override void OnUpdate()
    {
        if (reloadEndTime > Time.time)
            return;

        OnReload();
    }

    private void OnReload()
    {
        _weaponController.Weapon.OnReload();

        _weaponController.WeaponStateMachine.ChangeState(new WeaponReadyState(_weaponController));
    }
}
