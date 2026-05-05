using System;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;

public static class AnimationResourceExporter
{
    private const string SourceRoot = "Assets/Art/Animations";
    private const string TargetRoot = "Assets/Resources/Animations";

    public static void ExportAll()
    {
        if (!Directory.Exists(SourceRoot))
        {
            Debug.LogError("Animation source folder not found: " + SourceRoot);
            return;
        }

        Directory.CreateDirectory(TargetRoot);

        var controllers = Directory.GetFiles(SourceRoot, "*.controller", SearchOption.AllDirectories)
            .Select(NormalizePath)
            .ToArray();

        var copied = 0;
        foreach (var sourcePath in controllers)
        {
            var rel = NormalizePath(sourcePath.Substring(SourceRoot.Length + 1));
            var targetPath = NormalizePath(Path.Combine(TargetRoot, rel));
            var targetDir = NormalizePath(Path.GetDirectoryName(targetPath) ?? TargetRoot);
            Directory.CreateDirectory(targetDir);

            if (File.Exists(targetPath))
            {
                AssetDatabase.DeleteAsset(targetPath);
            }

            if (AssetDatabase.CopyAsset(sourcePath, targetPath))
            {
                copied++;
            }
            else
            {
                Debug.LogWarning("Failed to copy: " + sourcePath);
            }
        }

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        Debug.Log("Copied " + copied + " animation controllers into Resources.");
    }

    private static string NormalizePath(string path)
    {
        return path.Replace("\\", "/");
    }
}
