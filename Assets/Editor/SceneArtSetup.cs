using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

public static class SceneArtSetup
{
    private const string ScenePath = "Assets/Scenes/Main.unity";
    private const string LawnPath = "Assets/Art/items/lawn.png";
    private const string PeashooterControllerPath = "Assets/Resources/Animations/Plants/Peashooter.controller";

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

    public static void ApplyPrototypeFoundation()
    {
        var scene = EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);

        var camera = EnsureCamera();
        ConfigureCamera(camera);
        ApplyBackground();
        scene = EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);

        var world = FindOrCreateRoot("World");
        var laneGrid = world.GetComponent<LaneGrid>();
        if (laneGrid == null)
        {
            laneGrid = world.AddComponent<LaneGrid>();
        }

        laneGrid.laneCount = 5;
        laneGrid.laneSpacing = 1.4f;
        laneGrid.centerY = 0f;
        laneGrid.plantX = -4.5f;
        laneGrid.zombieSpawnX = 7.5f;
        laneGrid.despawnX = -8.5f;

        var spawnerObject = FindOrCreateChild(world.transform, "ZombieSpawner");
        var spawner = spawnerObject.GetComponent<ZombieSpawner>();
        if (spawner == null)
        {
            spawner = spawnerObject.AddComponent<ZombieSpawner>();
        }

        spawner.laneGrid = laneGrid;

        var plantRoot = FindOrCreateChild(world.transform, "PrototypePlants");
        CreatePrototypePlants(plantRoot.transform, laneGrid);

        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene);
        AssetDatabase.SaveAssets();

        Debug.Log("Applied prototype scene foundation to Main scene.");
    }

    private static Camera EnsureCamera()
    {
        var cameraObject = GameObject.Find("Main Camera");
        if (cameraObject == null)
        {
            cameraObject = new GameObject("Main Camera");
            cameraObject.tag = "MainCamera";
        }

        var camera = cameraObject.GetComponent<Camera>();
        if (camera == null)
        {
            camera = cameraObject.AddComponent<Camera>();
        }

        if (cameraObject.GetComponent<AudioListener>() == null)
        {
            cameraObject.AddComponent<AudioListener>();
        }

        return camera;
    }

    private static void ConfigureCamera(Camera camera)
    {
        camera.orthographic = true;
        camera.orthographicSize = 4.5f;
        camera.clearFlags = CameraClearFlags.SolidColor;
        camera.backgroundColor = new Color(0.85f, 0.93f, 0.85f);
        camera.transform.position = new Vector3(0f, 0f, -10f);
    }

    private static GameObject FindOrCreateRoot(string name)
    {
        var gameObject = GameObject.Find(name);
        if (gameObject == null)
        {
            gameObject = new GameObject(name);
        }

        gameObject.transform.SetParent(null);
        gameObject.transform.position = Vector3.zero;
        gameObject.transform.localScale = Vector3.one;
        return gameObject;
    }

    private static GameObject FindOrCreateChild(Transform parent, string name)
    {
        var child = parent.Find(name);
        if (child != null)
        {
            return child.gameObject;
        }

        var gameObject = new GameObject(name);
        gameObject.transform.SetParent(parent);
        gameObject.transform.localPosition = Vector3.zero;
        gameObject.transform.localScale = Vector3.one;
        return gameObject;
    }

    private static void CreatePrototypePlants(Transform parent, LaneGrid laneGrid)
    {
        var controller = AssetDatabase.LoadAssetAtPath<RuntimeAnimatorController>(PeashooterControllerPath);

        for (var i = 0; i < laneGrid.laneCount; i++)
        {
            var plant = FindOrCreateChild(parent, "Plant_Lane_" + i);
            plant.transform.position = new Vector3(laneGrid.plantX, laneGrid.GetLaneY(i), 0f);
            plant.transform.localScale = new Vector3(0.9f, 0.9f, 1f);

            var renderer = plant.GetComponent<SpriteRenderer>();
            if (renderer == null)
            {
                renderer = plant.AddComponent<SpriteRenderer>();
            }

            renderer.sortingOrder = 10 + i;

            var animator = plant.GetComponent<Animator>();
            if (controller != null)
            {
                if (animator == null)
                {
                    animator = plant.AddComponent<Animator>();
                }

                animator.runtimeAnimatorController = controller;
            }

            if (plant.GetComponent<PlantShooter>() == null)
            {
                plant.AddComponent<PlantShooter>();
            }
        }
    }
}
