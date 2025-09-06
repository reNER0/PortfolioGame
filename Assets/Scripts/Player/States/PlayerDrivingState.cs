using System.Linq;
using UnityEngine;

public class PlayerDrivingState : PlayerState
{
    private Car _car;
    private Seat _seat;


    public PlayerDrivingState(Player player, Car car, Seat seat) : base(player)
    {
        _car = car;
        _seat = seat;
    }


    public override void OnEnter()
    {
        _player.transform.parent = _seat.transform;
        _player.transform.localPosition = Vector3.zero;
        _player.transform.localRotation = Quaternion.identity;
    }

    public override void OnUpdate()
    {

    }

    public override void OnInput(PlayerInputs playerInputs)
    {
        _car.Input(playerInputs);

        if (!playerInputs.Jump)
            return;

        var leaveCar = new LeaveCarCmd(NetworkRepository.CurrentObjectId, NetworkRepository.NetworkObjectById.First(x => x.Predictable == _car).Id, _car.GetSeatId(_seat));
        NetworkBus.OnPerformCommand?.Invoke(leaveCar);
        NetworkBus.OnCommandSendToServer?.Invoke(leaveCar);
    }

    public override void OnExit()
    {
        _player.transform.parent = null;
        _player.transform.position = _seat.GetExitPoint();
    }
}
