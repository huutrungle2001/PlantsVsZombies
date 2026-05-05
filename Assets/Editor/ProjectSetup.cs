using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

public static class ProjectSetup
{
    private const string ScenePath = "Assets/Scenes/Main.unity";

    public static void CreateDefaultScene()
    {
        Directory.CreateDirectory("Assets/Scenes");

        if (File.Exists(ScenePath))
        {
            Debug.Log("Main scene already exists at " + ScenePath);
            return;
        }

        var scene = EditorSceneManager.NewScene(NewSceneSetup.DefaultGameObjects, NewSceneMode.Single);
        var camera = Object.FindObjectOfType<Camera>();
        if (camera != null)
        {
            camera.orthographic = true;
            camera.orthographicSize = 4.5f;
            camera.transform.position = new Vector3(0f, 0f, -10f);
        }

        EditorSceneManager.SaveScene(scene, ScenePath);

        EditorBuildSettings.scenes = new[]
        {
            new EditorBuildSettingsScene(ScenePath, true)
        };

        EditorSettings.defaultBehaviorMode = EditorBehaviorMode.Mode2D;
        AssetDatabase.SaveAssets();

        Debug.Log("Created default scene at " + ScenePath);
    }
}
