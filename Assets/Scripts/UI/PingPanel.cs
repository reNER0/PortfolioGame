using TMPro;
using UnityEngine;

public class PingPanel : MonoBehaviour
{
    [SerializeField]
    private TextMeshProUGUI pingText;

    private void Awake()
    {
        NetworkBus.OnPingUpdated += OnPing;
    }

    private void OnDestroy()
    {
        NetworkBus.OnPingUpdated -= OnPing;
    }

    private void OnPing(int ping)
    {
        pingText.text = ping.ToString();
    }
}
