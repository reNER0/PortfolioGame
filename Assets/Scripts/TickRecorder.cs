using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;
using UnityEngine;

public sealed class TickRecorder : MonoBehaviour
{
    [Header("Recording")]
    [SerializeField] private bool record = true;
    [SerializeField] private string serverFileName = "server_ticks.csv";
    [SerializeField] private string clientFileName = "client_ticks.csv";

    private bool isServer;

    private struct TickSample
    {
        public int tick;
        public float time;
    }

    private readonly List<TickSample> samples = new();

    public void SetIsServer()
    {
        isServer = true;
    }

    /// <summary>
    /// Вызывай ЭТО строго во время симуляции тика
    /// </summary>
    public void RecordTick(int tickNumber)
    {
        if (!record) return;

        float now = Time.realtimeSinceStartup;

        samples.Add(new TickSample
        {
            tick = tickNumber,
            time = now,
        });
    }

    /// <summary>
    /// Можно вызвать вручную (например по кнопке)
    /// </summary>
    public void Save()
    {
        string path = Path.Combine(Application.dataPath, isServer ? serverFileName : clientFileName);

        var sb = new StringBuilder();
        sb.AppendLine("tick,wall_time");

        foreach (var s in samples)
        {
            sb.AppendLine(string.Format(
                CultureInfo.InvariantCulture,
                "{0},{1:F6}",
                s.tick,
                s.time
            ));
        }

        File.WriteAllText(path, sb.ToString());

        Debug.Log($"[TickRecorder] Saved {samples.Count} ticks to:\n{path}");
    }

    private void OnApplicationQuit()
    {
        Save();
    }
}
