using System;
using UnityEngine;

namespace Assets.Scripts.Network.Commands
{
    [Serializable]
    public class StartStopShootingCmd : SerializableClass, ICommand
    {
        [SerializeField]
        private int playerId;
        [SerializeField]
        private bool state;

        public StartStopShootingCmd(int playerId, bool state)
        {
            this.playerId = playerId;
            this.state = state;
        }

        public void Execute()
        {
            var gameObject = NetworkRepository.Current.NetworkObjectById[playerId].Predictable;

            var player = gameObject.GetComponent<Player>();

            if (player == null)
                return;

            if (state)
                player.WeaponController.OnStartShooting();
            else
                player.WeaponController.OnStopShooting();
        }
    }
}
