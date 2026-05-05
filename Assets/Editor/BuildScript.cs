using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEngine;

public static class BuildScript
{
    public static void Build()
    {
        var scenes = EditorBuildSettings.scenes
            .Where(scene => scene.enabled)
            .Select(scene => scene.path)
            .ToArray();

        if (scenes.Length == 0)
        {
            scenes = AssetDatabase.FindAssets("t:Scene", new[] { "Assets/Scenes" })
                .Select(AssetDatabase.GUIDToAssetPath)
                .ToArray();
        }

        if (scenes.Length == 0)
        {
            Debug.LogError("No scenes found to build. Create a scene under Assets/Scenes.");
            EditorApplication.Exit(1);
            return;
        }

        const string outputDir = "Builds";
        Directory.CreateDirectory(outputDir);

        var buildOptions = new BuildPlayerOptions
        {
            scenes = scenes,
            locationPathName = GetOutputPath(outputDir),
            target = EditorUserBuildSettings.activeBuildTarget,
            options = BuildOptions.None
        };

        var report = BuildPipeline.BuildPlayer(buildOptions);
        if (report.summary.result != BuildResult.Succeeded)
        {
            EditorApplication.Exit(1);
        }
    }

    private static string GetOutputPath(string outputDir)
    {
#if UNITY_EDITOR_OSX
        return Path.Combine(outputDir, "PVZ.app");
#elif UNITY_EDITOR_WIN
        return Path.Combine(outputDir, "PVZ.exe");
#elif UNITY_EDITOR_LINUX
        return Path.Combine(outputDir, "PVZ.x86_64");
#else
        return Path.Combine(outputDir, "PVZ");
#endif
    }
}
