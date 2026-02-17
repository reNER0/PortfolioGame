using System;
using UnityEngine;

namespace Assets.Scripts.Network.Commands
{
    [Serializable]
    public class SetPlayerAnimatorBoolCmd : SerializableClass, ICommand
    {
        [SerializeField]
        private int playerId;
        [SerializeField]
        private string parameterName;
        [SerializeField]
        private bool state;

        public SetPlayerAnimatorBoolCmd(int playerId, string parameterName, bool state)
        {
            this.playerId = playerId;
            this.parameterName = parameterName;
            this.state = state;
        }

        public void Execute()
        {
            var gameObject = NetworkRepository.Current.NetworkObjectById[playerId].Predictable;

            var player = gameObject.GetComponent<Player>();

            if (player == null)
                return;

            player.Animator.SetBool(parameterName, state);
        }
    }
}
