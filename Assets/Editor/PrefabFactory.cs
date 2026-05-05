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
    private const string PrefabFolder  = "Assets/Prefabs";
    private const string PlantAnimRoot = "Assets/Resources/Animations/Plants";

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
        CreateAllPlantPrefabs();
        WireGameManagerPrefabs();
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

    // -------------------------------------------------------------------------
    // Core builder
    // -------------------------------------------------------------------------

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
}
