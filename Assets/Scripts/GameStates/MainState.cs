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

        NetworkBus.OnClientConnected += SpawnPlayer;
        NetworkBus.OnClientDisconnected += DestroyPlayer;
        GameBus.OnPlayerDead += OnPlayerDead;

        if (!spawnServerPlayer)
            return;

        SpawnPlayer(null);
    }

    public override void OnUpdate()
    {

    }

    public override void OnExit()
    {
        NetworkBus.OnClientConnected -= SpawnPlayer;
        NetworkBus.OnClientDisconnected -= DestroyPlayer;
        GameBus.OnPlayerDead -= OnPlayerDead;
    }


    private void DestroyPlayer(NetworkClient client)
    {
        var destroyPlayerCmd = new DestroyCmd(client.ClientObjectId);

        NetworkBus.OnPerformCommand?.Invoke(destroyPlayerCmd);
        NetworkBus.OnCommandSendToClients?.Invoke(destroyPlayerCmd);

        CheckForGameOver();
    }


    private void SpawnPlayer(NetworkClient client)
    {
        var spawnTransform = SpawnController.Instance.GetSpawnByPlayerId(client.ClientId);

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

        var setPlayerObjectCmd = new SetPlayerObjectCmd(clientId, NetworkRepository.Current.NetworkObjectById.Last().Id);

        NetworkBus.OnPerformCommand?.Invoke(setPlayerObjectCmd);

        if (client != null)
            NetworkBus.OnCommandSendToClient?.Invoke(setPlayerObjectCmd, client);
    }


    private void OnPlayerDead(Player player)
    {
        CheckForGameOver();
    }

    private void CheckForGameOver() 
    {
        var countOfAlivePlayers = NetworkRepository.Current.NetworkObjectById.Where(x => x.Predictable.GetType() == typeof(Player))
                                                            .Select(x => (Player)x.Predictable)
                                                            .Count(x => x.GetHealth() > 0);

        if (countOfAlivePlayers > 1)
            return;

        GameOver();
    }

    private void GameOver() 
    {
        Debug.Log("Game Over!");

        ServerHub.DisconnectAllClients();

        Debug.Log("[DEDIC] Match finished, shutting down");
        Application.Quit(0);
    }
}
