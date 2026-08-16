using Assets.Scripts.Network.Commands;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;
using UnityEngine;

public sealed class CmdRecorder : MonoBehaviour
{
    [Header("Recording")]
    [SerializeField] private bool record = true;
    [SerializeField] private string serverFileName = "server_cmds.csv";
    [SerializeField] private string clientFileName = "client_cmds.csv";

    private bool isServer;

    private class CmdSample : TickRecorder.TickSample
    {
        public ICommand command;
    }

    private readonly List<CmdSample> samples = new();

    public void SetIsServer()
    {
        isServer = true;
    }

    /// <summary>
    /// Вызывай ЭТО строго во время симуляции тика
    /// </summary>
    public void RecordCmd(ICommand command)
    {
        if (!record) return;

        float now = Time.realtimeSinceStartup;

        samples.Add(new CmdSample
        {
            tick = NetworkTime.CurrentTick,
            time = now,
            command = command
        });
    }

    /// <summary>
    /// Можно вызвать вручную (например по кнопке)
    /// </summary>
    public void Save()
    {
        string path = Path.Combine(Application.dataPath, isServer ? serverFileName : clientFileName);

        var sb = new StringBuilder();
        sb.AppendLine("tick,wall_time,command");

        foreach (var s in samples)
        {
            sb.AppendLine(string.Format(
                CultureInfo.InvariantCulture,
                "{0},{1:F6},{2}",
                s.tick,
                s.time,
                s.command.ToString()
            ));
        }

        File.WriteAllText(path, sb.ToString());

        Debug.Log($"[CmdRecorder] Saved {samples.Count} ticks to:\n{path}");
    }

    private void OnDestroy()
    {
        Save();
    }
}
