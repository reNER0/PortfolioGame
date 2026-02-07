using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerAttackState : PlayerStandingState
{
    public PlayerAttackState(Player player) : base(player) { }


    public override void OnEnter()
    {
        base.OnEnter();

        Debug.LogError("onEnter");
        _player.Animator.SetBool("Attack", true);
        _player.Animator.applyRootMotion = true;
    }

    public override void OnInput(PlayerInputs playerInputs)
    {
        base.OnInput(playerInputs);

        if (playerInputs.Aim)
            _player.PlayerStateMachine.ChangeState(new PlayerWalkingState(_player));
    }

    public override void OnExit() 
    {
        base.OnExit();

        Debug.LogError("onexit");
        _player.Animator.SetBool("Attack", false);
        _player.Animator.applyRootMotion = false;
    }
}
