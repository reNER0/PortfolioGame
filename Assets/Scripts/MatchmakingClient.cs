using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Networking;

public class MatchmakingClient : MonoBehaviour
{
    [Header("ASP Server")]
    [SerializeField] private string baseUrl = "http://95.105.7.90:7001/";
    [SerializeField] private float pollIntervalSeconds = 0.5f;

    public event Action<string> StatusChanged;               // "Queued", "Matched", ...
    public event Action<ConnectInfo, string> MatchFound;     // connectInfo, matchId
    public event Action<string> Error;

    private Coroutine _pollRoutine;
    private string _ticketId;

    public void StartMatchmaking()
    {
        StopMatchmaking();
        _pollRoutine = StartCoroutine(JoinAndPoll());
    }

    public void StopMatchmaking()
    {
        if (_pollRoutine != null)
        {
            StopCoroutine(_pollRoutine);
            _pollRoutine = null;
        }
        _ticketId = null;
    }

    private IEnumerator JoinAndPoll()
    {
        OnLog("Started searching match");

        // 1) Join (POST /queue/join)
        var joinUrl = $"{baseUrl.TrimEnd('/')}/queue/join";

        if (LaunchFlags.IsBot && !string.IsNullOrWhiteSpace(LaunchFlags.BotReservationId))
        {
            joinUrl += $"?botReservationId={UnityWebRequest.EscapeURL(LaunchFlags.BotReservationId)}";
        }

        using (var req = new UnityWebRequest(joinUrl, UnityWebRequest.kHttpVerbPOST))
        {
            req.uploadHandler = new UploadHandlerRaw(Array.Empty<byte>());
            req.downloadHandler = new DownloadHandlerBuffer(); // ВАЖНО
            req.SetRequestHeader("Content-Type", "application/json");
            req.timeout = 5;

            yield return req.SendWebRequest();

            var body = req.downloadHandler.text;
            Debug.Log($"[MM] JOIN code={req.responseCode} err={req.error} body='{body}'");

            if (req.result != UnityWebRequest.Result.Success)
            {
                Fail($"Join failed: {req.responseCode} {req.error} body='{body}'");
                yield break;
            }

            if (string.IsNullOrWhiteSpace(body))
            {
                Fail($"Join returned empty body (code {req.responseCode}). Check ASP returns JSON.");
                yield break;
            }

            var join = JsonUtility.FromJson<JoinQueueResponse>(body);
            if (join == null || string.IsNullOrEmpty(join.ticketId))
            {
                Fail($"Join returned invalid json: '{body}'");
                yield break;
            }

            _ticketId = join.ticketId;
        }

        StatusChanged?.Invoke("Queued");

        // 2) Poll (GET /queue/status/{ticketId})
        while (true)
        {
            var statusUrl = $"{baseUrl.TrimEnd('/')}/queue/status/{_ticketId}";

            using (var req = UnityWebRequest.Get(statusUrl))
            {
                req.downloadHandler = new DownloadHandlerBuffer(); // ВАЖНО

                yield return req.SendWebRequest();

                var body = req.downloadHandler.text;

                if (req.result != UnityWebRequest.Result.Success)
                {
                    Fail($"Status failed: {req.responseCode} {req.error} body='{body}'");
                    yield break;
                }

                if (string.IsNullOrWhiteSpace(body))
                {
                    Fail($"Status returned empty body (code {req.responseCode}).");
                    yield break;
                }

                var status = JsonUtility.FromJson<QueueStatusResponse>(body);
                if (status == null || string.IsNullOrEmpty(status.status))
                {
                    Fail($"Status returned invalid json: '{body}'");
                    yield break;
                }

                StatusChanged?.Invoke(status.status);

                if (string.Equals(status.status, "Matched", StringComparison.OrdinalIgnoreCase))
                {
                    if (status.connect == null || string.IsNullOrEmpty(status.connect.ip) || status.connect.port <= 0)
                    {
                        Fail($"Matched but connect info invalid: '{body}'");
                        yield break;
                    }

                    MatchFound?.Invoke(status.connect, status.matchId);
                    Debug.Log($"[MM] Match found! Connecting to {status.connect.ip}:{status.connect.port}");
                    OnLog("Match found!");

                    NetworkSettings.ServerIP = status.connect.ip;
                    NetworkSettings.ServerPort = status.connect.port;
                    SceneLoader.LoadClientScene();
                    yield break;
                }
            }

            yield return new WaitForSeconds(pollIntervalSeconds);
        }
    }

    private void Fail(string msg)
    {
        Debug.LogError(msg);
        OnLog(msg);
        Error?.Invoke(msg);
        StopMatchmaking();

        if (LaunchFlags.IsBot)
            Application.Quit();
    }

    private void OnLog(string log) 
    {
        UIBus.OnChatMessage?.Invoke(new ChatMessage()
        {
            sender = "Matchmaker",
            text = log
        });
    }
}
