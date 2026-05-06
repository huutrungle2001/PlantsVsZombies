using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

/// <summary>
/// Editor utility that creates plant prefabs programmatically.
///
/// Interactive use  – Unity menu: PvZ > Prefabs > ...
/// CLI / batch use  – Unity -executeMethod PrefabFactory.CreateAllPlantPrefabs
///
/// Each method builds a temporary scene object with the correct components,
/// saves it as a prefab under Assets/Prefabs/, then destroys the temp object.
/// Re-running a method overwrites the existing prefab safely.
/// </summary>
public static class PrefabFactory
{
    private const string PrefabFolder    = "Assets/Prefabs";
    private const string PlantAnimRoot  = "Assets/Resources/Animations/Plants";
    private const string ZombieAnimRoot = "Assets/Resources/Animations/Zombies";
    private const string PeaSpritePath  = "Assets/Art/items/Pea.png";

    private const string ScenePath = "Assets/Scenes/Main.unity";

    // -------------------------------------------------------------------------
    // CLI / batch-mode entry points
    // -------------------------------------------------------------------------

    /// <summary>
    /// Creates all plant prefabs AND wires them into the GameManager scene
    /// object in a single batch-mode call:
    ///
    ///   Unity -batchmode -quit -nographics \
    ///         -projectPath &lt;path&gt; \
    ///         -executeMethod PrefabFactory.CreateAndWireAllPrefabs \
    ///         -logFile -
    ///
    /// Exits with code 1 on any failure.
    /// </summary>
    [MenuItem("PvZ/Prefabs/Create and Wire All Prefabs")]
    public static void CreateAndWireAllPrefabs()
    {
        CreateAllPlantPrefabs();       // Peashooter.prefab, Sunflower.prefab
        CreateAllZombiePrefabs();      // BasicZombie.prefab
        CreatePeaProjectilePrefab();   // PeaProjectile.prefab
        WireGameManagerPrefabs();      // GameManager <- plant prefabs
        WireZombieSpawnerPrefab();     // ZombieSpawner <- BasicZombie
        WirePeashooterAgent();         // Peashooter.prefab <- PeashooterAgent + PeaProjectile
    }

    /// <summary>
    /// Creates Peashooter.prefab and Sunflower.prefab under Assets/Prefabs/.
    /// Designed to be invoked via Unity's -executeMethod flag:
    ///
    ///   Unity -batchmode -quit -nographics \
    ///         -projectPath &lt;path&gt; \
    ///         -executeMethod PrefabFactory.CreateAllPlantPrefabs \
    ///         -logFile -
    ///
    /// Exits with code 1 if any prefab fails to save.
    /// </summary>
    [MenuItem("PvZ/Prefabs/Create All Plant Prefabs")]
    public static void CreateAllPlantPrefabs()
    {
        EnsurePrefabFolder();

        var ok = true;
        ok &= BuildAndSave("Peashooter", "Peashooter", maxHp: 3, sunCost: 100,
                           scale: new Vector3(0.9f, 0.9f, 1f));
        ok &= BuildAndSave("Sunflower",  "SunFlower",  maxHp: 2, sunCost: 50,
                           scale: new Vector3(0.8f, 0.8f, 1f));

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        if (ok)
        {
            Debug.Log("[PrefabFactory] All plant prefabs created successfully.");
        }
        else
        {
            Debug.LogError("[PrefabFactory] One or more prefabs failed to save.");
            EditorApplication.Exit(1);
        }
    }

    /// <summary>
    /// Wires the already-created plant prefabs into the GameManager component
    /// that lives in the Main scene, then saves the scene.
    ///
    ///   Unity -batchmode -quit -nographics \
    ///         -projectPath &lt;path&gt; \
    ///         -executeMethod PrefabFactory.WireGameManagerPrefabs \
    ///         -logFile -
    ///
    /// Exits with code 1 on any failure.
    /// </summary>
    [MenuItem("PvZ/Prefabs/Wire Prefabs into GameManager")]
    public static void WireGameManagerPrefabs()
    {
        // Load prefab assets.
        var peashooterPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(
            PrefabFolder + "/Peashooter.prefab");
        var sunflowerPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(
            PrefabFolder + "/Sunflower.prefab");

        if (peashooterPrefab == null || sunflowerPrefab == null)
        {
            Debug.LogError("[PrefabFactory] Prefabs not found in " + PrefabFolder +
                           ". Run 'make prefabs' first.");
            EditorApplication.Exit(1);
            return;
        }

        // Open the main scene.
        var scene = EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);

        // Find the GameManager component.
        var gmObject = GameObject.Find("GameManager");
        if (gmObject == null)
        {
            Debug.LogError("[PrefabFactory] No 'GameManager' GameObject found in " + ScenePath);
            EditorApplication.Exit(1);
            return;
        }

        var gm = gmObject.GetComponent<GameManager>();
        if (gm == null)
        {
            // Component not yet attached – add it now so wiring can proceed.
            Debug.Log("[PrefabFactory] Adding GameManager component to 'GameManager' object.");
            gm = gmObject.AddComponent<GameManager>();
        }

        // Write the private serialized prefab fields via SerializedObject.
        var so = new SerializedObject(gm);
        so.FindProperty("peashooterPrefab").objectReferenceValue = peashooterPrefab;
        so.FindProperty("sunflowerPrefab").objectReferenceValue  = sunflowerPrefab;

        // Mirror the startingSun default so the scene value always matches the
        // value defined in GameManager.cs (serialized scene overrides win over
        // code defaults, so we must write it explicitly here).
        var startingSunProp = so.FindProperty("startingSun");
        if (startingSunProp != null)
        {
            var tempGm       = new GameObject("__TempGM").AddComponent<GameManager>();
            var tempSo       = new SerializedObject(tempGm);
            int codeDefault  = tempSo.FindProperty("startingSun").intValue;
            Object.DestroyImmediate(tempGm.gameObject);
            startingSunProp.intValue = codeDefault;
            Debug.Log($"[PrefabFactory] startingSun set to {codeDefault} in scene.");
        }

        so.ApplyModifiedPropertiesWithoutUndo();

        // Save the scene.
        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene);
        AssetDatabase.SaveAssets();

        Debug.Log("[PrefabFactory] GameManager prefab references wired and scene saved.");
    }

    // -------------------------------------------------------------------------
    // Individual menu items (editor convenience)
    // -------------------------------------------------------------------------

    [MenuItem("PvZ/Prefabs/Create Peashooter Prefab")]
    public static void CreatePeashooterPrefab()
    {
        EnsurePrefabFolder();
        BuildAndSave("Peashooter", "Peashooter", maxHp: 3, sunCost: 100,
                     scale: new Vector3(0.9f, 0.9f, 1f));
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
    }

    [MenuItem("PvZ/Prefabs/Create Sunflower Prefab")]
    public static void CreateSunflowerPrefab()
    {
        EnsurePrefabFolder();
        BuildAndSave("Sunflower", "SunFlower", maxHp: 2, sunCost: 50,
                     scale: new Vector3(0.8f, 0.8f, 1f));
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
    }

    /// <summary>
    /// Creates PeaProjectile.prefab using Assets/Art/items/Pea.png.
    /// </summary>
    [MenuItem("PvZ/Prefabs/Create PeaProjectile Prefab")]
    public static void CreatePeaProjectilePrefab()
    {
        EnsurePrefabFolder();
        bool ok = BuildProjectileAndSave(
            prefabName:  "PeaProjectile",
            spritePath:  PeaSpritePath,
            speed:       7f,
            damage:      1,
            scale:       Vector3.one * 0.35f);

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        if (ok) Debug.Log("[PrefabFactory] PeaProjectile.prefab created.");
        else    Debug.LogError("[PrefabFactory] Failed to create PeaProjectile.prefab.");
    }

    /// <summary>
    /// Edits the existing Peashooter.prefab in-place:
    /// adds PeashooterAgent and wires the PeaProjectile prefab reference.
    /// </summary>
    [MenuItem("PvZ/Prefabs/Wire PeashooterAgent into Peashooter")]
    public static void WirePeashooterAgent()
    {
        var peaPath        = PrefabFolder + "/PeaProjectile.prefab";
        var shooterPath    = PrefabFolder + "/Peashooter.prefab";

        var peaProjectile  = AssetDatabase.LoadAssetAtPath<GameObject>(peaPath);
        if (peaProjectile == null)
        {
            Debug.LogError("[PrefabFactory] PeaProjectile.prefab not found. Run CreatePeaProjectilePrefab first.");
            return;
        }

        // Load prefab contents, modify in memory, save back.
        var contents = PrefabUtility.LoadPrefabContents(shooterPath);

        var agent = contents.GetComponent<PeashooterAgent>();
        if (agent == null)
            agent = contents.AddComponent<PeashooterAgent>();

        var so = new SerializedObject(agent);
        so.FindProperty("projectilePrefab").objectReferenceValue = peaProjectile;
        so.ApplyModifiedPropertiesWithoutUndo();

        PrefabUtility.SaveAsPrefabAsset(contents, shooterPath);
        PrefabUtility.UnloadPrefabContents(contents);

        AssetDatabase.SaveAssets();
        Debug.Log("[PrefabFactory] PeashooterAgent wired into Peashooter.prefab.");
    }

    /// <summary>
    /// Creates all zombie prefabs. Called by CreateAndWireAllPrefabs.
    /// </summary>
    [MenuItem("PvZ/Prefabs/Create All Zombie Prefabs")]
    public static void CreateAllZombiePrefabs()
    {
        EnsurePrefabFolder();

        bool ok = BuildZombieAndSave(
            prefabName:     "BasicZombie",
            controllerName: "NormalZombie",
            maxHp:          10,
            speed:          0.9f,
            scale:          new Vector3(0.9f, 0.9f, 1f));

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        if (ok)
            Debug.Log("[PrefabFactory] All zombie prefabs created successfully.");
        else
        {
            Debug.LogError("[PrefabFactory] One or more zombie prefabs failed to save.");
            EditorApplication.Exit(1);
        }
    }

    /// <summary>
    /// Wires the BasicZombie prefab into the ZombieSpawner found in the Main scene.
    /// </summary>
    [MenuItem("PvZ/Prefabs/Wire BasicZombie into ZombieSpawner")]
    public static void WireZombieSpawnerPrefab()
    {
        var zombiePrefab = AssetDatabase.LoadAssetAtPath<GameObject>(
            PrefabFolder + "/BasicZombie.prefab");

        if (zombiePrefab == null)
        {
            Debug.LogError("[PrefabFactory] BasicZombie.prefab not found in " + PrefabFolder +
                           ". Run 'Create All Zombie Prefabs' first.");
            EditorApplication.Exit(1);
            return;
        }

        var scene = EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);

        var spawnerObject = GameObject.Find("ZombieSpawner");
        if (spawnerObject == null)
        {
            Debug.LogWarning("[PrefabFactory] No 'ZombieSpawner' GameObject found in " + ScenePath +
                             ". Skipping ZombieSpawner wiring.");
            return;
        }

        var spawner = spawnerObject.GetComponent<ZombieSpawner>();
        if (spawner == null)
        {
            Debug.LogWarning("[PrefabFactory] 'ZombieSpawner' object has no ZombieSpawner component. Skipping.");
            return;
        }

        var so = new SerializedObject(spawner);
        so.FindProperty("zombiePrefab").objectReferenceValue = zombiePrefab;
        so.ApplyModifiedPropertiesWithoutUndo();

        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene);
        AssetDatabase.SaveAssets();

        Debug.Log("[PrefabFactory] ZombieSpawner.zombiePrefab wired and scene saved.");
    }

    // -------------------------------------------------------------------------
    // Core builders
    // -------------------------------------------------------------------------

    /// <summary>
    /// Builds a PeaProjectile-style prefab with ProjectileAgent, Sprite,
    /// CircleCollider2D (trigger), and kinematic Rigidbody2D.
    /// </summary>
    private static bool BuildProjectileAndSave(
        string  prefabName,
        string  spritePath,
        float   speed,
        int     damage,
        Vector3 scale)
    {
        var go = new GameObject(prefabName);
        go.transform.localScale = scale;

        // SpriteRenderer
        var sr     = go.AddComponent<SpriteRenderer>();
        var sprite = AssetDatabase.LoadAssetAtPath<Sprite>(spritePath);
        if (sprite != null)
            sr.sprite = sprite;
        else
            Debug.LogWarning($"[PrefabFactory] Sprite not found at '{spritePath}'.");
        sr.sortingOrder = 20; // Init() will adjust per-lane at runtime

        // CircleCollider2D – trigger so OnTriggerEnter2D fires on contact
        var col    = go.AddComponent<CircleCollider2D>();
        col.isTrigger = true;
        col.radius    = 0.45f;

        // Kinematic Rigidbody2D – required for trigger callbacks from static colliders
        var rb            = go.AddComponent<Rigidbody2D>();
        rb.bodyType       = RigidbodyType2D.Kinematic;
        rb.gravityScale   = 0f;

        // ProjectileAgent – write private serialized fields
        var agent = go.AddComponent<ProjectileAgent>();
        var so    = new SerializedObject(agent);
        so.FindProperty("speed").floatValue  = speed;
        so.FindProperty("damage").intValue   = damage;
        so.ApplyModifiedPropertiesWithoutUndo();

        var path  = $"{PrefabFolder}/{prefabName}.prefab";
        var saved = PrefabUtility.SaveAsPrefabAsset(go, path);
        Object.DestroyImmediate(go);

        if (saved == null) { Debug.LogError($"[PrefabFactory] Failed to save {path}"); return false; }
        Debug.Log($"[PrefabFactory] Saved {path}");
        return true;
    }

    /// <summary>
    /// Builds a BasicZombie-style prefab with ZombieAgent and the given
    /// animation controller, saves it under Assets/Prefabs/, returns true on success.
    /// </summary>
    private static bool BuildZombieAndSave(
        string  prefabName,
        string  controllerName,
        int     maxHp,
        float   speed,
        Vector3 scale)
    {
        var go = new GameObject(prefabName);
        go.transform.localScale = scale;

        // SpriteRenderer
        var sr = go.AddComponent<SpriteRenderer>();
        sr.sortingOrder = 10;

        // Animator + controller
        var animator   = go.AddComponent<Animator>();
        var controller = LoadZombieController(controllerName);
        if (controller != null)
            animator.runtimeAnimatorController = controller;
        else
            Debug.LogWarning($"[PrefabFactory] Zombie controller '{controllerName}' not found. " +
                             "Prefab saved without one.");

        // BoxCollider2D (non-trigger so projectile triggers can detect it)
        var col  = go.AddComponent<BoxCollider2D>();
        col.size = new Vector2(0.8f, 1.2f);

        // ZombieAgent — write private serialized fields via SerializedObject.
        var agent = go.AddComponent<ZombieAgent>();
        var so    = new SerializedObject(agent);
        so.FindProperty("maxHp").intValue    = maxHp;
        so.FindProperty("speed").floatValue  = speed;
        so.ApplyModifiedPropertiesWithoutUndo();

        // Save prefab asset.
        var path  = $"{PrefabFolder}/{prefabName}.prefab";
        var saved = PrefabUtility.SaveAsPrefabAsset(go, path);
        Object.DestroyImmediate(go);

        if (saved == null)
        {
            Debug.LogError($"[PrefabFactory] Failed to save prefab at {path}");
            return false;
        }

        Debug.Log($"[PrefabFactory] Saved {path}");
        return true;
    }

    /// <summary>
    /// Builds a temporary scene object, saves it as a prefab, then destroys
    /// the temp object. Returns true on success.
    /// </summary>
    /// <param name="prefabName">File name (no extension) written to Assets/Prefabs/.</param>
    /// <param name="controllerName">
    ///   Name of the .controller asset under Assets/Resources/Animations/Plants/.
    /// </param>
    private static bool BuildAndSave(
        string prefabName,
        string controllerName,
        int    maxHp,
        int    sunCost,
        Vector3 scale)
    {
        // Build the temp object.
        var go = new GameObject(prefabName);
        go.transform.localScale = scale;

        // SpriteRenderer
        var sr = go.AddComponent<SpriteRenderer>();
        sr.sortingOrder = 10;

        // Animator + controller
        var animator = go.AddComponent<Animator>();
        var controller = LoadController(controllerName);
        if (controller != null)
        {
            animator.runtimeAnimatorController = controller;
        }
        else
        {
            // Non-fatal: prefab is still usable; assign controller manually later.
            Debug.LogWarning($"[PrefabFactory] Controller '{controllerName}' not found. " +
                             $"Prefab saved without one.");
        }

        // Collider (trigger – used for click/debug selection)
        var col = go.AddComponent<BoxCollider2D>();
        col.isTrigger = true;
        col.size      = Vector2.one;

        // Plant component – write private serialized fields via SerializedObject
        // so they are correctly baked into the prefab asset.
        var plant = go.AddComponent<Plant>();
        var so    = new SerializedObject(plant);
        so.FindProperty("maxHp").intValue   = maxHp;
        so.FindProperty("sunCost").intValue = sunCost;
        so.ApplyModifiedPropertiesWithoutUndo();

        // Save as prefab asset.
        var path   = $"{PrefabFolder}/{prefabName}.prefab";
        var saved  = PrefabUtility.SaveAsPrefabAsset(go, path);
        Object.DestroyImmediate(go);

        if (saved == null)
        {
            Debug.LogError($"[PrefabFactory] Failed to save prefab at {path}");
            return false;
        }

        Debug.Log($"[PrefabFactory] Saved {path}");
        return true;
    }

    // -------------------------------------------------------------------------
    // Helpers
    // -------------------------------------------------------------------------

    private static void EnsurePrefabFolder()
    {
        if (!AssetDatabase.IsValidFolder(PrefabFolder))
        {
            AssetDatabase.CreateFolder("Assets", "Prefabs");
        }
    }

    private static RuntimeAnimatorController LoadController(string name)
    {
        var path = $"{PlantAnimRoot}/{name}.controller";
        return AssetDatabase.LoadAssetAtPath<RuntimeAnimatorController>(path);
    }

    private static RuntimeAnimatorController LoadZombieController(string name)
    {
        var path = $"{ZombieAnimRoot}/{name}.controller";
        return AssetDatabase.LoadAssetAtPath<RuntimeAnimatorController>(path);
    }
}
