using System.Linq;
using UnityEngine;

public class SpawnController : Singleton<SpawnController>
{
    [SerializeField]
    private Transform[] spawnPoints;

    public Transform GetSpawnByPlayerId(int id)
    {
        if (spawnPoints == null || !spawnPoints.Any())
        {
            Debug.LogError($"{nameof(SpawnController)}: no spawning points!");
            return null;
        }

        while (id < 0)
        {
            id += spawnPoints.Length;
        }

        while (id >= spawnPoints.Length)
        {
            id -= spawnPoints.Length;
        }

        return spawnPoints[id];
    }
}
