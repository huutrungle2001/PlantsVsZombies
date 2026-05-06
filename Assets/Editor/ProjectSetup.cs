using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

public static class ProjectSetup
{
    private const string ScenePath = "Assets/Scenes/Main.unity";

    // -------------------------------------------------------------------------
    // CLI entry point
    // -------------------------------------------------------------------------

    /// <summary>
    /// Full one-shot project bootstrap. Intended to be run once on a clean
    /// clone via:
    ///
    ///   make setup
    ///
    /// which calls:
    ///   Unity -batchmode -quit -nographics -executeMethod ProjectSetup.FullSetup
    ///
    /// Steps (each is idempotent – safe to re-run):
    ///   1. Create the Main scene if it does not exist.
    ///   2. Apply the prototype scene foundation (camera, board, tiles, GameManager, etc.).
    ///   3. Create plant prefabs (Peashooter, Sunflower) under Assets/Prefabs/.
    ///   4. Wire prefab references into the GameManager scene object and save.
    /// </summary>
    public static void FullSetup()
    {
        CreateDefaultScene();
        SceneArtSetup.ApplyPrototypeFoundation();   // scene objects + WaveManager + ZombieSpawner
        PrefabFactory.CreateAllPlantPrefabs();       // Peashooter.prefab, Sunflower.prefab
        PrefabFactory.CreateAllZombiePrefabs();      // BasicZombie.prefab
        PrefabFactory.CreatePeaProjectilePrefab();   // PeaProjectile.prefab
        PrefabFactory.CreateSunPickupPrefab();       // SunPickup.prefab
        PrefabFactory.WireGameManagerPrefabs();      // GameManager <- plant prefabs + startingSun
        PrefabFactory.WireZombieSpawnerPrefab();     // ZombieSpawner <- BasicZombie.prefab
        PrefabFactory.WirePeashooterAgent();         // Peashooter.prefab <- PeashooterAgent
        PrefabFactory.WireSunflowerAgent();          // Sunflower.prefab  <- SunflowerAgent
        PrefabFactory.WireWaveManager();             // WaveManager <- BasicZombie.prefab
        UIFactory.BuildHud();                        // Sun display, cards, win/lose panels

        Debug.Log("[ProjectSetup] FullSetup complete.");
    }

    // -------------------------------------------------------------------------
    // Individual steps (kept public for editor scripting convenience)
    // -------------------------------------------------------------------------

    public static void CreateDefaultScene()
    {
        Directory.CreateDirectory("Assets/Scenes");

        if (File.Exists(ScenePath))
        {
            Debug.Log("[ProjectSetup] Main scene already exists – skipping creation.");
            return;
        }

        var scene = EditorSceneManager.NewScene(NewSceneSetup.DefaultGameObjects, NewSceneMode.Single);

        var camera = Object.FindFirstObjectByType<Camera>();
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

        Debug.Log("[ProjectSetup] Created Main scene at " + ScenePath);
    }
}
