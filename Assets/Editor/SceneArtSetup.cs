using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

public static class SceneArtSetup
{
    private const string ScenePath = "Assets/Scenes/Main.unity";
    private const string BackgroundPath = "Assets/Art/items/Frontyard.png";
    private const string PeashooterControllerPath = "Assets/Resources/Animations/Plants/Peashooter.controller";
    private const float TargetAspectWidth = 16f;
    private const float TargetAspectHeight = 9f;
    private const float CameraOrthographicSize = 4.5f;
    private const float BoardTileWidth = 1.2569444f;
    private const float BoardTileHeight = 1.4089457f;
    private static readonly Vector2 BoardTopLeftCellCenter = new Vector2(-3.4027778f, 2.7891374f);

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

        var sprite = AssetDatabase.LoadAssetAtPath<Sprite>(BackgroundPath);
        if (sprite == null)
        {
            Debug.LogError("Could not load background sprite at " + BackgroundPath);
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

        FindOrCreateRoot("GameManager");
        FindOrCreateRoot("LaneRegistry");
        FindOrCreateRoot("WaveManager");
        FindOrCreateRoot("UI");

        var board = FindOrCreateRoot("Board");
        var boardGrid = ConfigureBoard(board);
        CreateTiles(board.transform, boardGrid);

        var world = FindOrCreateRoot("World");
        var laneGrid = world.GetComponent<LaneGrid>();
        if (laneGrid == null)
        {
            laneGrid = world.AddComponent<LaneGrid>();
        }

        laneGrid.laneCount = 5;
        laneGrid.laneSpacing = BoardTileHeight;
        laneGrid.centerY = BoardTopLeftCellCenter.y - BoardTileHeight * 2f;
        laneGrid.plantX = BoardTopLeftCellCenter.x;
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

    public static void ValidateEnvironmentFoundation()
    {
        EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);

        var failed = false;
        var requiredRoots = new[] { "GameManager", "Board", "LaneRegistry", "WaveManager", "UI", "Background" };
        foreach (var root in requiredRoots)
        {
            if (GameObject.Find(root) == null)
            {
                Debug.LogError("Missing scene root: " + root);
                failed = true;
            }
        }

        var board = GameObject.Find("Board");
        var boardGrid = board != null ? board.GetComponent<BoardGrid>() : null;
        if (boardGrid == null)
        {
            Debug.LogError("Board is missing BoardGrid.");
            failed = true;
        }
        else
        {
            failed |= !ValidateBoardGrid(boardGrid);
        }

        if (failed)
        {
            EditorApplication.Exit(1);
            return;
        }

        Debug.Log("Environment foundation validation passed.");
    }

    private static bool ValidateBoardGrid(BoardGrid boardGrid)
    {
        var valid = true;
        if (boardGrid.rowCount != 5 || boardGrid.columnCount != 9)
        {
            Debug.LogError("BoardGrid should be 5 rows by 9 columns.");
            valid = false;
        }

        var tileRoot = boardGrid.transform.Find("Tiles");
        if (tileRoot == null)
        {
            Debug.LogError("Board is missing Tiles child.");
            return false;
        }

        for (var row = 0; row < boardGrid.rowCount; row++)
        {
            for (var column = 0; column < boardGrid.columnCount; column++)
            {
                var tileObject = tileRoot.Find("Tile_" + row + "_" + column);
                if (tileObject == null)
                {
                    Debug.LogError("Missing tile: " + row + ", " + column);
                    valid = false;
                    continue;
                }

                valid &= ValidateTile(boardGrid, tileObject.gameObject, row, column);
            }
        }

        return valid;
    }

    private static bool ValidateTile(BoardGrid boardGrid, GameObject tileObject, int expectedRow, int expectedColumn)
    {
        var valid = true;
        var tile = tileObject.GetComponent<Tile>();
        if (tile == null)
        {
            Debug.LogError(tileObject.name + " is missing Tile component.");
            valid = false;
        }
        else if (tile.row != expectedRow || tile.column != expectedColumn)
        {
            Debug.LogError(tileObject.name + " has wrong coordinate data.");
            valid = false;
        }

        if (tileObject.GetComponent<SpriteRenderer>() == null)
        {
            Debug.LogError(tileObject.name + " is missing SpriteRenderer.");
            valid = false;
        }

        if (tileObject.GetComponent<BoxCollider2D>() == null)
        {
            Debug.LogError(tileObject.name + " is missing BoxCollider2D.");
            valid = false;
        }

        if (!boardGrid.TryGetCellAtWorld(tileObject.transform.position, out var row, out var column)
            || row != expectedRow
            || column != expectedColumn)
        {
            Debug.LogError(tileObject.name + " does not map back to its expected coordinate.");
            valid = false;
        }

        tileObject.SendMessage("OnMouseDown", SendMessageOptions.RequireReceiver);

        return valid;
    }

    private static BoardGrid ConfigureBoard(GameObject board)
    {
        var boardGrid = board.GetComponent<BoardGrid>();
        if (boardGrid == null)
        {
            boardGrid = board.AddComponent<BoardGrid>();
        }

        boardGrid.rowCount = 5;
        boardGrid.columnCount = 9;
        boardGrid.tileWidth = BoardTileWidth;
        boardGrid.tileHeight = BoardTileHeight;
        boardGrid.topLeftCellCenter = BoardTopLeftCellCenter;
        return boardGrid;
    }

    private static void CreateTiles(Transform boardRoot, BoardGrid boardGrid)
    {
        var tileRoot = FindOrCreateChild(boardRoot, "Tiles");

        for (var row = 0; row < boardGrid.rowCount; row++)
        {
            for (var column = 0; column < boardGrid.columnCount; column++)
            {
                var tileObject = FindOrCreateChild(tileRoot.transform, "Tile_" + row + "_" + column);
                tileObject.transform.position = boardGrid.GetCellCenter(row, column);
                tileObject.transform.localScale = new Vector3(boardGrid.tileWidth, boardGrid.tileHeight, 1f);

                var renderer = tileObject.GetComponent<SpriteRenderer>();
                if (renderer == null)
                {
                    renderer = tileObject.AddComponent<SpriteRenderer>();
                }

                renderer.sortingOrder = -30;

                var collider = tileObject.GetComponent<BoxCollider2D>();
                if (collider == null)
                {
                    collider = tileObject.AddComponent<BoxCollider2D>();
                }

                collider.size = Vector2.one;
                collider.isTrigger = true;

                var tile = tileObject.GetComponent<Tile>();
                if (tile == null)
                {
                    tile = tileObject.AddComponent<Tile>();
                }

                tile.boardGrid = boardGrid;
                tile.row = row;
                tile.column = column;
            }
        }
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
        camera.orthographicSize = CameraOrthographicSize;
        camera.clearFlags = CameraClearFlags.SolidColor;
        camera.backgroundColor = new Color(0.85f, 0.93f, 0.85f);
        camera.transform.position = new Vector3(0f, 0f, -10f);

        var fixedAspect = camera.GetComponent<FixedAspectCamera>();
        if (fixedAspect == null)
        {
            fixedAspect = camera.gameObject.AddComponent<FixedAspectCamera>();
        }

        fixedAspect.targetWidth = TargetAspectWidth;
        fixedAspect.targetHeight = TargetAspectHeight;
        fixedAspect.ApplyAspect();
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
