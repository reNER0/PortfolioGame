#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEngine;
using System.IO;
using System.Linq;

public static class BuildTool
{
    private static string outDir = "Builds";


    [MenuItem("Build/MacOS_Client")]
    public static void BuildClientMacDev()
    {
        var target = BuildTarget.StandaloneOSX;

        var options = BuildOptions.Development | BuildOptions.AllowDebugging;

        var scenes = EditorBuildSettings.scenes
            .Where(s => s.enabled)
            .Select(s => s.path)
            .ToArray();

        var outPath = Path.Combine(outDir, "ClientBuild.app");

        var report = BuildPipeline.BuildPlayer(new BuildPlayerOptions
        {
            scenes = scenes,
            locationPathName = outPath,
            target = target,
            options = options
        });


        if (report.summary.result == BuildResult.Succeeded)
            Debug.Log($"MacOS Client Build OK: {report.summary.outputPath} - {report.summary.totalSize / (1024 * 1024)} MB");
        else
            Debug.LogError($"MacOS Client Build FAILED: {report.summary.result}");
    }


    [MenuItem("Build/MacOS_Server")]
    public static void BuildServerMacHeadlessDev()
    {
        var target = BuildTarget.StandaloneOSX;

        var options = BuildOptions.Development | BuildOptions.AllowDebugging;

        var subtarget = (int)StandaloneBuildSubtarget.Server;

        var scenes = EditorBuildSettings.scenes
            .Where(s => s.enabled)
            .Select(s => s.path)
            .ToArray();

        var outPath = Path.Combine(outDir, "ServerBuild");

        var report = BuildPipeline.BuildPlayer(new BuildPlayerOptions
        {
            scenes = scenes,
            locationPathName = outPath,
            target = target,
            subtarget = subtarget,
            options = options
        });


        if (report.summary.result == BuildResult.Succeeded)
        {
            Debug.Log($"MacOS Server Build OK: {report.summary.outputPath} - {report.summary.totalSize / (1024 * 1024)} MB");
        }
        else
        {
            Debug.LogError($"MacOS Server Build FAILED: {report.summary.result}");
        }
    }
}
#endif
