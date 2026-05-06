using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Owns global match state. Responsible for:
///   - Tracking the game state machine (NotStarted → Playing ↔ Paused, → Won / Lost).
///   - Holding and vending the sun economy (AddSun / SpendSun).
///   - Tracking which plant the player has selected from the card UI.
///   - Validating and executing plant placement (tile occupancy, sun cost, game state).
///   - Providing pause, resume, and restart.
///
/// Other systems call NotifyWin() / NotifyLose() to end the match.
/// UI reads State, Sun, and SelectedPlant to drive its display.
/// </summary>
public class GameManager : MonoBehaviour
{
    // -------------------------------------------------------------------------
    // Singleton
    // -------------------------------------------------------------------------

    public static GameManager Instance { get; private set; }

    // PlantType -> remaining cooldown seconds (0 = ready).
    private readonly Dictionary<PlantType, float> cooldownTimers =
        new Dictionary<PlantType, float>();

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    // -------------------------------------------------------------------------
    // Inspector fields
    // -------------------------------------------------------------------------

    [Header("Starting Resources")]
    [Tooltip("Sun the player starts each match with.")]
    [SerializeField] private int startingSun = 9999;

    [Header("Plant Costs")]
    [SerializeField] private int peashooterCost = 100;
    [SerializeField] private int sunflowerCost = 50;

    [Header("Plant Cooldowns")]
    [Tooltip("Seconds before Peashooter can be placed again.")]
    [SerializeField] private float peashooterCooldown = 7.5f;
    [Tooltip("Seconds before Sunflower can be placed again.")]
    [SerializeField] private float sunflowerCooldown  = 7.5f;

    [Header("Plant Prefabs")]
    [Tooltip("Assign the Peashooter prefab here once it is created.")]
    [SerializeField] private GameObject peashooterPrefab;
    [Tooltip("Assign the Sunflower prefab here once it is created.")]
    [SerializeField] private GameObject sunflowerPrefab;

    // Serialized so the current state is visible in the Inspector during play.
    [Header("State (read-only in play mode)")]
    [SerializeField] private GameState currentState = GameState.NotStarted;

    // -------------------------------------------------------------------------
    // Public read-only state
    // -------------------------------------------------------------------------

    /// <summary>Current match state. Change only via the state-transition helpers below.</summary>
    public GameState State => currentState;

    /// <summary>Current sun balance.</summary>
    public int Sun { get; private set; }

    /// <summary>Plant type the player has picked from the card UI. None = no selection.</summary>
    public PlantType SelectedPlant { get; private set; } = PlantType.None;

    /// <summary>True when the player has a plant selected and the game is running.</summary>
    public bool IsPlacementActive => SelectedPlant != PlantType.None && currentState == GameState.Playing;

    // -------------------------------------------------------------------------
    // Unity lifecycle
    // -------------------------------------------------------------------------

    private void Start()
    {
        StartGame();
    }

    private void Update()
    {
        // Drain cooldown timers.
        foreach (PlantType type in System.Enum.GetValues(typeof(PlantType)))
        {
            if (type == PlantType.None) continue;
            if (cooldownTimers.TryGetValue(type, out float remaining) && remaining > 0f)
                cooldownTimers[type] = Mathf.Max(0f, remaining - Time.deltaTime);
        }
    }

    // -------------------------------------------------------------------------
    // State transitions
    // -------------------------------------------------------------------------

    /// <summary>
    /// Initialises (or reinitialises) the match. Resets sun and clears any
    /// selection. Called automatically on Start; call manually after a scene
    /// reload to be safe.
    /// </summary>
    public void StartGame()
    {
        Sun = startingSun;
        SelectedPlant = PlantType.None;
        SetState(GameState.Playing);
        Debug.Log($"[GameManager] Game started. Starting sun: {Sun}");
    }

    /// <summary>Freezes time and reloads the active scene to perform a full restart.</summary>
    public void RestartGame()
    {
        // Always restore time before a scene reload so the new scene starts unfrozen.
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    /// <summary>Pauses the game. Only valid from the Playing state.</summary>
    public void Pause()
    {
        if (currentState != GameState.Playing)
        {
            return;
        }

        Time.timeScale = 0f;
        SetState(GameState.Paused);
        Debug.Log("[GameManager] Paused.");
    }

    /// <summary>Resumes play. Only valid from the Paused state.</summary>
    public void Resume()
    {
        if (currentState != GameState.Paused)
        {
            return;
        }

        Time.timeScale = 1f;
        SetState(GameState.Playing);
        Debug.Log("[GameManager] Resumed.");
    }

    /// <summary>
    /// Called by WaveManager (or any system) when all zombies are defeated.
    /// Agents should stop their update loops when State == Won.
    /// </summary>
    public void NotifyWin()
    {
        if (currentState != GameState.Playing)
        {
            return;
        }

        SetState(GameState.Won);
        Debug.Log("[GameManager] Player wins!");
    }

    /// <summary>
    /// Called by ZombieAgent when a zombie reaches the house side.
    /// Agents should stop their update loops when State == Lost.
    /// </summary>
    public void NotifyLose()
    {
        if (currentState != GameState.Playing)
        {
            return;
        }

        SetState(GameState.Lost);
        Debug.Log("[GameManager] Player loses!");
    }

    // -------------------------------------------------------------------------
    // Sun economy
    // -------------------------------------------------------------------------

    /// <summary>Adds sun (collected from pickups or starting grants).</summary>
    public void AddSun(int amount)
    {
        if (amount <= 0)
        {
            return;
        }

        Sun += amount;
        Debug.Log($"[GameManager] +{amount} sun. Total: {Sun}");
    }

    /// <summary>
    /// Removes sun. Returns false and makes no change if balance would go
    /// negative. Prefer TryPlacePlant over calling this directly.
    /// </summary>
    public bool SpendSun(int amount)
    {
        if (amount <= 0 || Sun < amount)
        {
            return false;
        }

        Sun -= amount;
        Debug.Log($"[GameManager] -{amount} sun. Total: {Sun}");
        return true;
    }

    // -------------------------------------------------------------------------
    // Placement cooldowns
    // -------------------------------------------------------------------------

    /// <summary>Returns true when the plant type is still on placement cooldown.</summary>
    public bool IsOnCooldown(PlantType type)
    {
        return cooldownTimers.TryGetValue(type, out float r) && r > 0f;
    }

    /// <summary>
    /// Returns the fraction of cooldown remaining (1 = just placed, 0 = ready).
    /// Useful for filling a cooldown overlay on the card UI.
    /// </summary>
    public float GetCooldownFraction(PlantType type)
    {
        float total = GetCooldownDuration(type);
        if (total <= 0f) return 0f;
        cooldownTimers.TryGetValue(type, out float remaining);
        return remaining / total;
    }

    /// <summary>Total cooldown duration for the given plant type in seconds.</summary>
    public float GetCooldownDuration(PlantType type)
    {
        switch (type)
        {
            case PlantType.Peashooter: return peashooterCooldown;
            case PlantType.Sunflower:  return sunflowerCooldown;
            default:                   return 0f;
        }
    }

    // -------------------------------------------------------------------------
    // Plant card selection
    // -------------------------------------------------------------------------

    /// <summary>Marks a plant type as selected. Ignored when the game is not playing.</summary>
    public void SelectPlant(PlantType type)
    {
        if (currentState != GameState.Playing)
        {
            return;
        }

        SelectedPlant = type;
        Debug.Log($"[GameManager] Selected plant: {type}");
    }

    /// <summary>Clears the active plant selection without changing game state.</summary>
    public void CancelSelection()
    {
        SelectedPlant = PlantType.None;
    }

    // -------------------------------------------------------------------------
    // Plant placement
    // -------------------------------------------------------------------------

    /// <summary>
    /// Validates and executes plant placement on the given tile.
    ///
    /// Validation order:
    ///   1. Game must be in the Playing state.
    ///   2. A plant type must be selected.
    ///   3. The tile must be unoccupied.
    ///   4. The player must have enough sun.
    ///   5. A prefab must be assigned for the selected type.
    ///
    /// On success: instantiates the prefab, initialises the Plant component,
    /// marks the tile occupied, and deducts sun.
    ///
    /// Returns true on success, false on any validation failure.
    /// </summary>
    public bool TryPlacePlant(Tile tile)
    {
        if (currentState != GameState.Playing)
        {
            Debug.Log("[GameManager] Placement rejected: game is not in the Playing state.");
            return false;
        }

        if (SelectedPlant == PlantType.None)
        {
            Debug.Log("[GameManager] Placement rejected: no plant selected.");
            return false;
        }

        if (tile.IsOccupied)
        {
            Debug.Log($"[GameManager] Placement rejected: tile ({tile.row},{tile.column}) is already occupied.");
            return false;
        }

        int cost = GetCost(SelectedPlant);
        if (Sun < cost)
        {
            Debug.Log($"[GameManager] Placement rejected: need {cost} sun, have {Sun}.");
            return false;
        }

        if (IsOnCooldown(SelectedPlant))
        {
            Debug.Log($"[GameManager] Placement rejected: {SelectedPlant} is on cooldown.");
            return false;
        }

        GameObject prefab = GetPrefab(SelectedPlant);
        if (prefab == null)
        {
            Debug.LogWarning($"[GameManager] Placement rejected: no prefab assigned for {SelectedPlant}. " +
                             "Assign it in the GameManager Inspector field.");
            return false;
        }

        // --- All checks passed: place the plant ---

        Vector3 worldPos = tile.transform.position;
        worldPos.z = 0f;

        GameObject plantObj = Instantiate(prefab, worldPos, Quaternion.identity);

        // Bind the Plant component to its tile (if present on the prefab).
        var plant = plantObj.GetComponent<Plant>();
        if (plant != null)
        {
            plant.Init(tile);
        }

        tile.OccupyWith(plantObj);
        SpendSun(cost);

        // Start placement cooldown.
        float cd = GetCooldownDuration(SelectedPlant);
        if (cd > 0f) cooldownTimers[SelectedPlant] = cd;

        Debug.Log($"[GameManager] Placed {SelectedPlant} at ({tile.row},{tile.column}). Sun remaining: {Sun}");
        return true;
    }

    // -------------------------------------------------------------------------
    // Private helpers
    // -------------------------------------------------------------------------

    private void SetState(GameState newState)
    {
        currentState = newState;
    }

    private int GetCost(PlantType type)
    {
        switch (type)
        {
            case PlantType.Peashooter: return peashooterCost;
            case PlantType.Sunflower:  return sunflowerCost;
            default:                   return 0;
        }
    }

    private GameObject GetPrefab(PlantType type)
    {
        switch (type)
        {
            case PlantType.Peashooter: return peashooterPrefab;
            case PlantType.Sunflower:  return sunflowerPrefab;
            default:                   return null;
        }
    }
}
