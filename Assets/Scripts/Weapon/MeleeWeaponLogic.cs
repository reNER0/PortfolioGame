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
        var direction = Tools.DirectionFromYawPitch(playerInputs.Yaw, playerInputs.Pitch);
        direction.y = 0;

        bool fireHeld = playerInputs.Fire;

        if (!playerInputs.Fire)
            return;

        if (player.PlayerStateMachine.currentState.GetType() == typeof(PlayerAttackState))
            return;

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
}
