using System.Linq;
using UnityEngine;

public class PredictablesSyncOnStart : MonoBehaviour
{
    private void Awake()
    {
        var predictables = FindObjectsOfType<Predictable>()
                                 .OrderBy(p => p.name)
                                 .ToArray();

        foreach (var predictable in predictables)
        {
            var networkObject = new NetworkObject(predictable);
            NetworkRepository.Current.NetworkObjectById.Add(networkObject);
        }
    }
}
