using System.Linq;
using Assets.Scripts.Network.Commands;
using UnityEngine;

[CreateAssetMenu(menuName = "State/MainState", order = 1)]
public class MainState : State
{
    [SerializeField]
    private bool spawnServerPlayer;

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
        NetworkBus.OnClientDisconnected += DestroyPlayer;

        if (!spawnServerPlayer)
            return;

        SpawnPlayer();
    }

    public override void OnUpdate()
    {

    }

    public override void OnExit()
    {
        NetworkBus.OnClientConnected -= CheckForReady;
        NetworkBus.OnClientDisconnected -= DestroyPlayer;
    }


    private void DestroyPlayer(NetworkClient client)
    {
        var destroyPlayerCmd = new DestroyCmd(client.ClientObjectId);

        NetworkBus.OnPerformCommand?.Invoke(destroyPlayerCmd);
    }


    private void CheckForReady(NetworkClient client)
    {
        SpawnPlayer(client);
    }


    private void SpawnPlayer(NetworkClient client = null)
    {
        var spawnTransform = SpawnController.Instance.GetSpawnByPlayerId(NetworkRepository.CurrentCliendId);

        if (spawnTransform == null)
        {
            Debug.LogError($"{nameof(MainState)}: no spawn!");
            return;
        }

        var spawnPlayerCmd = new SpawnCmd(playerObjectName, spawnTransform.position, spawnTransform.rotation);

        NetworkBus.OnPerformCommand?.Invoke(spawnPlayerCmd);

        int clientId = -1;

        if (client != null)
            clientId = client.ClientId;

        var setPlayerObjectCmd = new SetPlayerObjectCmd(clientId, NetworkRepository.NetworkObjectById.Last().Id);

        NetworkBus.OnPerformCommand?.Invoke(setPlayerObjectCmd);

        if (client != null)
            NetworkBus.OnCommandSendToClient?.Invoke(setPlayerObjectCmd, client);
    }
}
