using UnityEngine;

public static class Bootstrap
{
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    private static void Init()
    {
        Application.targetFrameRate = 60;

        var camera = Camera.main;
        if (camera == null)
        {
            var cameraObject = new GameObject("Main Camera");
            camera = cameraObject.AddComponent<Camera>();
            cameraObject.tag = "MainCamera";
        }

        camera.orthographic = true;
        camera.orthographicSize = 4.5f;
        camera.clearFlags = CameraClearFlags.SolidColor;
        camera.backgroundColor = new Color(0.85f, 0.93f, 0.85f);
        camera.transform.position = new Vector3(0f, 0f, -10f);

        SetupBackground(camera);

        var world = new GameObject("World");
        var laneGrid = world.AddComponent<LaneGrid>();

        SetupLaneOverlays(camera, laneGrid);

        var spawnerObject = new GameObject("ZombieSpawner");
        spawnerObject.transform.SetParent(world.transform);
        var spawner = spawnerObject.AddComponent<ZombieSpawner>();
        spawner.laneGrid = laneGrid;

        CreatePlants(world.transform, laneGrid);
    }

    private static void SetupBackground(Camera camera)
    {
        var background = GameObject.Find("Background");
        if (background == null)
        {
            return;
        }

        var renderer = background.GetComponent<SpriteRenderer>();
        if (renderer == null || renderer.sprite == null)
        {
            return;
        }

        var viewHeight = camera.orthographicSize * 2f;
        var viewWidth = viewHeight * camera.aspect;
        var spriteSize = renderer.sprite.bounds.size;

        renderer.sortingOrder = -100;
        renderer.drawMode = SpriteDrawMode.Simple;
        background.transform.position = new Vector3(camera.transform.position.x, camera.transform.position.y, 0f);
        background.transform.localScale = new Vector3(
            viewWidth / spriteSize.x,
            viewHeight / spriteSize.y,
            1f);
    }

    private static void SetupLaneOverlays(Camera camera, LaneGrid laneGrid)
    {
        var overlayRoot = GameObject.Find("LaneOverlays");
        if (overlayRoot == null)
        {
            overlayRoot = new GameObject("LaneOverlays");
        }

        var viewHeight = camera.orthographicSize * 2f;
        var viewWidth = viewHeight * camera.aspect;
        var lineWidth = viewWidth * 1.02f;
        var lineHeight = 0.08f;
        var lineColor = new Color(1f, 1f, 1f, 0.22f);

        for (var i = 0; i < laneGrid.laneCount - 1; i++)
        {
            var line = GameObject.Find("LaneLine_" + i);
            if (line == null)
            {
                line = new GameObject("LaneLine_" + i);
                line.transform.SetParent(overlayRoot.transform);
            }

            var renderer = line.GetComponent<SpriteRenderer>();
            if (renderer == null)
            {
                renderer = line.AddComponent<SpriteRenderer>();
            }

            renderer.sprite = SimpleSprite.Create(lineColor);
            renderer.sortingOrder = -50;

            var y = (laneGrid.GetLaneY(i) + laneGrid.GetLaneY(i + 1)) * 0.5f;
            line.transform.position = new Vector3(0f, y, -1f);
            line.transform.localScale = new Vector3(lineWidth, lineHeight, 1f);
        }
    }

    private static void CreatePlants(Transform parent, LaneGrid laneGrid)
    {
        var plantColor = new Color(0.2f, 0.8f, 0.2f);
        var projectileColor = new Color(1f, 0.9f, 0.2f);

        for (var i = 0; i < laneGrid.laneCount; i++)
        {
            var plant = new GameObject("Plant_Lane_" + i);
            plant.transform.SetParent(parent);
            plant.transform.position = new Vector3(laneGrid.plantX, laneGrid.GetLaneY(i), 0f);
            plant.transform.localScale = new Vector3(0.9f, 0.9f, 1f);

            var renderer = plant.AddComponent<SpriteRenderer>();
            var controller = ArtLibrary.GetPlantController("Peashooter");
            if (controller != null)
            {
                var animator = plant.AddComponent<Animator>();
                animator.runtimeAnimatorController = controller;
            }
            else
            {
                renderer.sprite = SimpleSprite.Create(plantColor);
            }

            var shooter = plant.AddComponent<PlantShooter>();
            shooter.projectileColor = projectileColor;
        }
    }
}
