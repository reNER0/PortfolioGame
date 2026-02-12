using Assets.Scripts.Weapon;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class MeleeWeaponLogic : IWeaponLogic
{
    private Player player;

    public MeleeWeaponLogic(Player player)
    {
        this.player = player;
    }

    public void Attack(PlayerInputs playerInputs)
    {
        var direction = Tools.DirectionFromYawPitch(playerInputs.Yaw, playerInputs.Pitch);

        bool fireHeld = playerInputs.Fire;

        if (!playerInputs.Fire)
            return;

        if (player.PlayerStateMachine.currentState.GetType() == typeof(PlayerAttackState))
            return;

        player.PlayerStateMachine.ChangeState(new PlayerAttackState(player));
    }

    public bool NeedReload()
    {
        return false;
    }

    // TODO : do smth with this
    public void OnReload() { }
}
