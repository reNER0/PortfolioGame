using System;
using System.Linq;
using Assets.Scripts.Network.Commands;
using UnityEngine;

[Serializable]
public class JumpInCarCmd : SerializableClass, ICommand
{
    [SerializeField]
    private int _playerId;
    [SerializeField]
    private int _carId;
    [SerializeField]
    private int _seatId;

    public JumpInCarCmd(int playerId, int carId, int seatId)
    {
        _playerId = playerId;
        _carId = carId;
        _seatId = seatId;
    }

    public void Execute()
    {
        var carPredictable = NetworkRepository.NetworkObjectById.FirstOrDefault(x => x.Id == _carId).Predictable;

        var car = (Car)carPredictable;

        var seat = car.GetSeat(_seatId);


        var player = (Player)NetworkRepository.NetworkObjectById.First(x => x.Id == _playerId).Predictable;


        if (NetworkRepository.IsServer)
        {
            // If server its validate first

            var distanceBetweebPlayerAndSeat = Vector3.Distance(seat.transform.position, player.transform.position);

            var sender = NetworkRepository.ConnectedClients.FirstOrDefault(x => x.ClientId == senderId);

            var canSeat = distanceBetweebPlayerAndSeat < seat.SeatableRadius;

            if (canSeat)
            {
                NetworkBus.OnCommandSendToClientsExcept(this, sender);
            }
            else
            {
                var leaveCarCmd = new LeaveCarCmd(_playerId, _carId, _seatId);
                NetworkBus.OnCommandSendToClient?.Invoke(leaveCarCmd, sender);
                return;
            }
        }


        seat.SetPlayer(player);
        player.PlayerStateMachine.ChangeState(new PlayerDrivingState(player, car, seat));
    }
}