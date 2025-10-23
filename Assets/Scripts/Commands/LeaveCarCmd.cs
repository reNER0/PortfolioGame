using System;
using System.Linq;
using Assets.Scripts.Network.Commands;
using UnityEngine;

[Serializable]
public class LeaveCarCmd : SerializableClass, ICommand
{
    [SerializeField]
    private int _playerId;
    [SerializeField]
    private int _carId;
    [SerializeField]
    private int _seatId;

    public LeaveCarCmd(int playerId, int carId, int seatId)
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

        bool isSender = senderId == NetworkRepository.CurrentCliendId;

        if (NetworkRepository.IsServer)
        {
            if (isSender)
            {
                NetworkBus.OnCommandSendToClients(this);
            }
            else
            {
                var sender = NetworkRepository.ConnectedClients.FirstOrDefault(x => x.ClientId == senderId);
                NetworkBus.OnCommandSendToClientsExcept(this, sender);
            }
        }
        else if (isSender)
        {
            NetworkBus.OnCommandSendToServer?.Invoke(this);
        }

        seat.SetPlayer(null);
        player.PlayerStateMachine.ChangeState(new PlayerWalkingState(player));
    }
}