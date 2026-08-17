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

            var networkObject = NetworkRepository.Current.NetworkObjectById
                .FirstOrDefault(x => x.Id == client.ClientObjectId);

            if (networkObject?.Predictable == null)
                return;

            var player = networkObject.Predictable.GetComponent<Player>();

            if (player == null)
                return;

            player.WeaponController.OnReload();
        }

        public override string ToString()
        {
            return $"StartReloadingCmd: senderId={senderId}";
        }
    }
}
