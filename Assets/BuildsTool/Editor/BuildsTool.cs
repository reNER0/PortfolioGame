#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEngine;
using System.IO;
using System.Linq;

public static class BuildTool
{
    private static string serverOutDir = "Builds/Server";
    private static string clientOutDir = "Builds/Client";

    [MenuItem("Build/MacOS_Client")]
    public static void BuildClientMacDev()
        => BuildClient(BuildTarget.StandaloneOSX, Path.Combine(clientOutDir, "MacOS", "ClientBuild.app"));

    [MenuItem("Build/MacOS_Server")]
    public static void BuildServerMacHeadlessDev()
        => BuildServer(BuildTarget.StandaloneOSX, Path.Combine(serverOutDir, "MacOS", "ServerBuild"));

    [MenuItem("Build/Windows_Client")]
    public static void BuildClientWindowsDev()
        => BuildClient(BuildTarget.StandaloneWindows64, Path.Combine(clientOutDir, "Windows", "ClientBuild.exe"));

    [MenuItem("Build/Windows_Server")]
    public static void BuildServerWindowsHeadlessDev()
        => BuildServer(BuildTarget.StandaloneWindows64, Path.Combine(serverOutDir, "Windows", "ServerBuild.exe"));

    private static void BuildClient(BuildTarget target, string outPath)
    {
        Directory.CreateDirectory(Path.GetDirectoryName(outPath)!);

        var options = BuildOptions.Development | BuildOptions.AllowDebugging;

        var scenes = EditorBuildSettings.scenes
            .Where(s => s.enabled)
            .Select(s => s.path)
            .ToArray();

        var report = BuildPipeline.BuildPlayer(new BuildPlayerOptions
        {
            scenes = scenes,
            locationPathName = outPath,
            target = target,
            subtarget = (int)StandaloneBuildSubtarget.Player,
            options = options
        });

        LogResult(report, "Client");
    }

    private static void BuildServer(BuildTarget target, string outPath)
    {
        Directory.CreateDirectory(Path.GetDirectoryName(outPath)!);

        var options = BuildOptions.Development | BuildOptions.AllowDebugging;

        var scenes = EditorBuildSettings.scenes
            .Where(s => s.enabled)
            .Select(s => s.path)
            .ToArray();

        var report = BuildPipeline.BuildPlayer(new BuildPlayerOptions
        {
            scenes = scenes,
            locationPathName = outPath,
            target = target,
            subtarget = (int)StandaloneBuildSubtarget.Server,
            options = options
        });

        LogResult(report, "Server");
    }

    private static void LogResult(BuildReport report, string label)
    {
        if (report.summary.result == BuildResult.Succeeded)
            Debug.Log($"{label} Build OK: {report.summary.outputPath} - {report.summary.totalSize / (1024 * 1024)} MB");
        else
            Debug.LogError($"{label} Build FAILED: {report.summary.result}");
    }
}
#endif
