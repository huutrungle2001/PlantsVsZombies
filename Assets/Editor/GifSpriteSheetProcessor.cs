using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.Animations;
using UnityEngine;

public static class GifSpriteSheetProcessor
{
    private const string SheetRoot = "Assets/Art/SpriteSheets";
    private const string AnimRoot = "Assets/Art/Animations";

    [Serializable]
    private class SpriteSheetInfo
    {
        public int frameWidth;
        public int frameHeight;
        public int frameCount;
        public int[] durationsMs;
    }

    public static void ProcessAll()
    {
        if (!Directory.Exists(SheetRoot))
        {
            Debug.LogError("Sprite sheet folder not found: " + SheetRoot);
            return;
        }

        AssetDatabase.Refresh();

        var sheetPaths = Directory.GetFiles(SheetRoot, "*_sheet.png", SearchOption.AllDirectories)
            .Select(NormalizePath)
            .ToArray();

        foreach (var sheetPath in sheetPaths)
        {
            try
            {
                ProcessSheet(sheetPath);
            }
            catch (Exception ex)
            {
                Debug.LogError("Failed to process " + sheetPath + ": " + ex.Message);
            }
        }

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        Debug.Log("Processed " + sheetPaths.Length + " sprite sheets.");
    }

    private static void ProcessSheet(string sheetPath)
    {
        var infoPath = Path.ChangeExtension(sheetPath, ".json");
        if (!File.Exists(infoPath))
        {
            Debug.LogWarning("Missing sprite sheet metadata: " + infoPath);
            return;
        }

        var info = JsonUtility.FromJson<SpriteSheetInfo>(File.ReadAllText(infoPath));
        if (info == null || info.frameCount <= 0)
        {
            Debug.LogWarning("Invalid metadata: " + infoPath);
            return;
        }

        EnsureImporter(sheetPath, info);

        var sprites = AssetDatabase.LoadAllAssetsAtPath(sheetPath)
            .OfType<Sprite>()
            .OrderBy(sprite => sprite.name, StringComparer.Ordinal)
            .ToArray();

        if (sprites.Length == 0)
        {
            Debug.LogWarning("No sprites found for: " + sheetPath);
            return;
        }

        var rel = NormalizePath(sheetPath.Substring(SheetRoot.Length + 1));
        var relDir = NormalizePath(Path.GetDirectoryName(rel) ?? string.Empty);
        var baseName = Path.GetFileNameWithoutExtension(rel);
        if (baseName.EndsWith("_sheet", StringComparison.OrdinalIgnoreCase))
        {
            baseName = baseName.Substring(0, baseName.Length - "_sheet".Length);
        }

        var animDir = NormalizePath(Path.Combine(AnimRoot, relDir));
        Directory.CreateDirectory(animDir);

        var clipPath = NormalizePath(Path.Combine(animDir, baseName + ".anim"));
        var clip = AssetDatabase.LoadAssetAtPath<AnimationClip>(clipPath);
        if (clip == null)
        {
            clip = new AnimationClip();
            AssetDatabase.CreateAsset(clip, clipPath);
        }

        BuildClip(clip, sprites, info);

        var controllerPath = NormalizePath(Path.Combine(animDir, baseName + ".controller"));
        if (!File.Exists(controllerPath))
        {
            var controller = AnimatorController.CreateAnimatorControllerAtPath(controllerPath);
            controller.AddMotion(clip);
        }
    }

    private static void EnsureImporter(string sheetPath, SpriteSheetInfo info)
    {
        var importer = AssetImporter.GetAtPath(sheetPath) as TextureImporter;
        if (importer == null)
        {
            Debug.LogWarning("Texture importer not found for: " + sheetPath);
            return;
        }

        importer.textureType = TextureImporterType.Sprite;
        importer.spriteImportMode = SpriteImportMode.Multiple;
        importer.mipmapEnabled = false;
        importer.filterMode = FilterMode.Point;
        importer.spritePixelsPerUnit = 100f;

        var meta = new List<SpriteMetaData>();
        for (var i = 0; i < info.frameCount; i++)
        {
            var rect = new Rect(i * info.frameWidth, 0, info.frameWidth, info.frameHeight);
            meta.Add(new SpriteMetaData
            {
                name = GetSpriteName(sheetPath, i),
                rect = rect,
                alignment = (int)SpriteAlignment.Center,
                pivot = new Vector2(0.5f, 0.5f)
            });
        }

        importer.spritesheet = meta.ToArray();
        EditorUtility.SetDirty(importer);
        importer.SaveAndReimport();
    }

    private static void BuildClip(AnimationClip clip, Sprite[] sprites, SpriteSheetInfo info)
    {
        var binding = new EditorCurveBinding
        {
            type = typeof(SpriteRenderer),
            path = string.Empty,
            propertyName = "m_Sprite"
        };

        var durations = info.durationsMs ?? Array.Empty<int>();
        var keyframes = new ObjectReferenceKeyframe[sprites.Length];
        var time = 0f;

        for (var i = 0; i < sprites.Length; i++)
        {
            keyframes[i] = new ObjectReferenceKeyframe
            {
                time = time,
                value = sprites[i]
            };

            var durationMs = durations.Length > i ? durations[i] : 100;
            time += Mathf.Max(0.01f, durationMs / 1000f);
        }

        AnimationUtility.SetObjectReferenceCurve(clip, binding, keyframes);
        clip.frameRate = 60f;

        var settings = AnimationUtility.GetAnimationClipSettings(clip);
        settings.loopTime = true;
        AnimationUtility.SetAnimationClipSettings(clip, settings);

        EditorUtility.SetDirty(clip);
    }

    private static string GetSpriteName(string sheetPath, int index)
    {
        var baseName = Path.GetFileNameWithoutExtension(sheetPath);
        return baseName + "_" + index.ToString("D2");
    }

    private static string NormalizePath(string path)
    {
        return path.Replace("\\", "/");
    }
}
