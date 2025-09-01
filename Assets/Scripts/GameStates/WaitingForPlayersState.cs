using Assets.Scripts.Network.Commands;
using UnityEngine;

[CreateAssetMenu(menuName = "State/WaitingState", order = 1)]
public class WaitingForPlayersState : State
{
    [SerializeField]
    private State nextServerState;
    [SerializeField]
    private State nextClientsState;
    [SerializeField]
    private int playersCount;
    [SerializeField]
    private string playerObjectName;

    public override void OnEnter()
    {
        Debug.Log("Waiting for players...");

        NetworkBus.OnClientConnected += CheckForReady;
    }

    public override void OnUpdate()
    {

    }

    public override void OnExit()
    {
        NetworkBus.OnClientConnected -= CheckForReady;
    }

    private void CheckForReady(NetworkClient client)
    {
        var spawnTransform = SpawnController.Instance.GetSpawnByPlayerId(NetworkRepository.CurrentCliendId);

        if (spawnTransform == null)
        {
            Debug.LogError($"{nameof(WaitingForPlayersState)}: no spawn!");
            return;
        }

        var spawnPlayerCmd = new SpawnCmd(playerObjectName, client.ClientId, spawnTransform.position, spawnTransform.rotation);

        NetworkBus.OnPerformCommand?.Invoke(spawnPlayerCmd);

        if (NetworkRepository.ConnectedClients.Count < playersCount)
            return;

        Debug.Log($"{playersCount} players connected, moving to {nextServerState?.name}");


        var cmd = new StateCmd(nextClientsState.name);
        NetworkBus.OnCommandSendToClients?.Invoke(cmd);

        NetworkBus.OnStateChanged?.Invoke(nextServerState);
    }
}
