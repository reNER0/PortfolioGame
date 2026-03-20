using Assets.Scripts.Weapon;
using UnityEngine;

public class MeleeWeaponLogic : IWeaponLogic
{
    private Player player;

    public MeleeWeaponLogic(Player player)
    {
        this.player = player;
    }

    public void Attack(PlayerInputs playerInputs)
    {
        bool fireHeld = playerInputs.Fire;
        bool aimHeld = playerInputs.Aim;
        bool attacking = player.PlayerStateMachine.currentState.GetType() == typeof(PlayerAttackState);

        if (attacking)
        {
            if (player.WeaponController.IsAiming)
                player.WeaponController.Aim(false);
            return;
        }


        if (!fireHeld)
        {
            if (aimHeld != player.WeaponController.IsAiming)
                player.WeaponController.Aim(aimHeld);
            return;
        }

        var direction = Tools.DirectionFromYawPitch(playerInputs.Yaw, playerInputs.Pitch);
        direction.y = 0;

        Quaternion targetRotation = Quaternion.LookRotation(direction);
        player.Rigidbody.MoveRotation(targetRotation);
        player.PlayerStateMachine.ChangeState(new PlayerAttackState(player));
    }

    public bool NeedReload()
    {
        return false;
    }

    // TODO : do smth with this
    public void OnReload() { }

    public void OnShowVisual() { }
}
