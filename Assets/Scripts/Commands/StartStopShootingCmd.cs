using System;
using System.Linq;
using UnityEngine;

namespace Assets.Scripts.Network.Commands
{
    [Serializable]
    public class StartStopShootingCmd : SerializableClass, ICommand
    {
        [SerializeField]
        private int playerObjectId;
        [SerializeField]
        private bool state;

        public StartStopShootingCmd(int playerObjectId, bool state)
        {
            this.playerObjectId = playerObjectId;
            this.state = state;
        }

        public void Execute()
        {
            var networkObject = NetworkRepository.Current.NetworkObjectById.FirstOrDefault(x => x.Id == playerObjectId);

            if (networkObject == null)
                return;

            var player = (Player)networkObject.Predictable;

            if (player == null)
                return;

            if (state)
                player.WeaponController.OnStartShooting();
            else
                player.WeaponController.OnStopShooting();
        }

        public override string ToString()
        {
            return $"StartStopShootingCmd: playerId={playerObjectId}, state={state}";
        }
    }
}
