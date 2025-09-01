using System.Collections;
using System.Collections.Generic;
using Assets.Scripts.Network.Commands;
using UnityEngine;

[CreateAssetMenu(menuName = "State/LocalPlayerState", order = 1)]
public class LocalPlayerState : State
{
    [SerializeField]
    private State nextState;
    [SerializeField]
    private string playerPrefabName;

    public override void OnEnter()
    {
        Debug.Log("Starting local player...");

        SpawnLocalPlayer();
        MoveToNextState();
    }

    public override void OnUpdate()
    {

    }

    public override void OnExit()
    {

    }

    private void SpawnLocalPlayer()
    {
        var spawnTransform = SpawnController.Instance.GetSpawnByPlayerId(NetworkRepository.CurrentCliendId);

        if (spawnTransform == null)
        {
            Debug.LogError($"{nameof(LocalPlayerState)}: no spawn!");
            return;
        }

        var spawnPlayerCmd = new SpawnCmd(playerPrefabName, -1, spawnTransform.position, spawnTransform.rotation);

        NetworkBus.OnPerformCommand?.Invoke(spawnPlayerCmd);
    }

    private void MoveToNextState()
    {
        var cmd = new StateCmd(nextState.name);
        NetworkBus.OnCommandSendToClients?.Invoke(cmd);

        NetworkBus.OnStateChanged?.Invoke(nextState);
    }
}
