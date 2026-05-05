using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

public static class SceneArtSetup
{
    private const string ScenePath = "Assets/Scenes/Main.unity";
    private const string LawnPath = "Assets/Art/items/lawn.png";

    public static void ApplyBackground()
    {
        var scene = EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);

        var cameraObject = GameObject.Find("Main Camera");
        var camera = cameraObject != null ? cameraObject.GetComponent<Camera>() : null;

        var background = GameObject.Find("Background");
        if (background == null)
        {
            background = new GameObject("Background");
        }

        var renderer = background.GetComponent<SpriteRenderer>();
        if (renderer == null)
        {
            renderer = background.AddComponent<SpriteRenderer>();
        }

        var sprite = AssetDatabase.LoadAssetAtPath<Sprite>(LawnPath);
        if (sprite == null)
        {
            Debug.LogError("Could not load background sprite at " + LawnPath);
            return;
        }

        renderer.sprite = sprite;
        renderer.sortingOrder = -100;
        renderer.drawMode = SpriteDrawMode.Simple;

        background.transform.position = Vector3.zero;

        var viewWidth = 19.2f;
        var viewHeight = 10.8f;
        if (camera != null && camera.orthographic)
        {
            viewHeight = camera.orthographicSize * 2f;
            viewWidth = viewHeight * camera.aspect;
        }

        var spriteSize = sprite.bounds.size;
        background.transform.localScale = new Vector3(
            viewWidth / spriteSize.x,
            viewHeight / spriteSize.y,
            1f);

        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene);

        Debug.Log("Applied lawn background to Main scene.");
    }
}
