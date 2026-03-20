using System;
using System.Linq;
using Assets.Scripts.Network.Commands;
using UnityEditor.Rendering;
using UnityEngine;

public class PlayerDeadState : PlayerState
{
    public PlayerDeadState(Player player) : base(player) { }


    public override void OnEnter()
    {
        _player.Rigidbody.constraints = RigidbodyConstraints.None;

        _player.Animator.SetLayerWeight(1, 0);
        _player.Animator.SetTrigger("Dead");
    }

    public override void OnUpdate() { }

    public override void OnInput(PlayerInputs playerInputs) { }

    public override void OnExit() { }
}
