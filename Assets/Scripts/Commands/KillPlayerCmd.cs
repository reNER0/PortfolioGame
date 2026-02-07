using System;
using System.Linq;
using Assets.Scripts.Network.Commands;
using UnityEngine;

[Serializable]
public class KillPlayerCmd : SerializableClass, ICommand
{
    [SerializeField]
    private int _playerObjectId;

    public KillPlayerCmd(int playerId)
    {
        _playerObjectId = playerId;
    }

    public void Execute()
    {
        var player = (Player)NetworkRepository.Current.NetworkObjectById.First(x => x.Id == _playerObjectId).Predictable;

        if (NetworkRepository.Current.IsServer)
                NetworkBus.OnCommandSendToClients(this);

        player.PlayerStateMachine.ChangeState(new PlayerDeadState(player));

        GameBus.OnPlayerDead?.Invoke(player);
    }
}