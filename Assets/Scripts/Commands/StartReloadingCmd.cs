using System;
using System.Linq;
using UnityEngine;

namespace Assets.Scripts.Network.Commands
{
    [Serializable]
    public class StartReloadingCmd : SerializableClass, ICommand
    {
        public void Execute()
        {
            var client = NetworkRepository.Current.ConnectedClients.FirstOrDefault(x => x.ClientId == senderId);

            if (client == null)
                return;

            var gameObject = NetworkRepository.Current.NetworkObjectById[client.ClientObjectId].Predictable;

            var player = gameObject.GetComponent<Player>();

            if (player == null)
                return;

            player.WeaponController.OnReload();
        }
    }
}
