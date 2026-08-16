using System;
using System.Linq;
using UnityEngine;

namespace Assets.Scripts.Network.Commands
{
    [Serializable]
    public class SetPlayerAnimatorBoolCmd : SerializableClass, ICommand
    {
        [SerializeField]
        private int playerObjectId;
        [SerializeField]
        private string parameterName;
        [SerializeField]
        private bool state;

        public SetPlayerAnimatorBoolCmd(int playerId, string parameterName, bool state)
        {
            this.playerObjectId = playerId;
            this.parameterName = parameterName;
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

            player.Animator.SetBool(parameterName, state);
        }

        public override string ToString()
        {
            return $"SetPlayerAnimatorBoolCmd: playerId={playerObjectId}, parameterName={parameterName}, state={state}";
        }
    }
}
