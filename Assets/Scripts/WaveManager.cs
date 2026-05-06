using UnityEngine;

/// <summary>
/// Schedules zombie spawns according to a wave table.
/// Replaces the endless random ZombieSpawner once configured.
///
/// If no entries are provided, a default wave is generated from
/// defaultZombiePrefab so the game is always playable out of the box.
///
/// Notifies GameManager.NotifyWin() when every scheduled zombie has been
/// spawned AND LaneRegistry reports zero active zombies.
/// </summary>
public class WaveManager : MonoBehaviour
{
    // -------------------------------------------------------------------------
    // Singleton
    // -------------------------------------------------------------------------

    public static WaveManager Instance { get; private set; }

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
    }

    // -------------------------------------------------------------------------
    // Inspector
    // -------------------------------------------------------------------------

    [Header("Default Zombie (used when no entries are configured)")]
    [Tooltip("Assign BasicZombie.prefab here.")]
    [SerializeField] private GameObject defaultZombiePrefab;

    [Header("Wave Schedule (leave empty to auto-generate)")]
    [SerializeField] private WaveEntry[] entries = new WaveEntry[0];

    [Header("Timing")]
    [Tooltip("Seconds before the first zombie spawns.")]
    [SerializeField] private float preWaveDelay = 5f;

    [Header("Audio")]
    [Tooltip("Optional sound played when the wave begins.")]
    [SerializeField] private AudioClip zombiesComingSound;

    // -------------------------------------------------------------------------
    // State
    // -------------------------------------------------------------------------

    private bool  waveStarted;
    private bool  waveComplete;
    private int   nextEntryIndex;
    private float elapsed;

    // -------------------------------------------------------------------------
    // Unity lifecycle
    // -------------------------------------------------------------------------

    private void Start()
    {
        if (entries == null || entries.Length == 0)
            entries = BuildDefaultWave();
    }

    private void Update()
    {
        if (GameManager.Instance == null ||
            GameManager.Instance.State != GameState.Playing) return;

        if (waveComplete) return;

        elapsed += Time.deltaTime;

        float waveTime = elapsed - preWaveDelay;
        if (waveTime < 0f) return;

        // Play "zombies are coming" cue once when wave kicks off.
        if (!waveStarted)
        {
            waveStarted = true;
            if (zombiesComingSound != null)
                AudioSource.PlayClipAtPoint(zombiesComingSound, Vector3.zero);

            Debug.Log("[WaveManager] Wave started.");
        }

        // Spawn every entry whose time has arrived.
        while (nextEntryIndex < entries.Length &&
               entries[nextEntryIndex].spawnTime <= waveTime)
        {
            SpawnEntry(entries[nextEntryIndex]);
            nextEntryIndex++;
        }

        // Win condition: all spawned and none left alive.
        bool allSpawned = nextEntryIndex >= entries.Length;
        bool noZombies  = LaneRegistry.Instance == null ||
                          LaneRegistry.Instance.GetActiveZombieCount() == 0;

        if (allSpawned && noZombies)
        {
            waveComplete = true;
            Debug.Log("[WaveManager] Wave complete – notifying win.");
            GameManager.Instance.NotifyWin();
        }
    }

    // -------------------------------------------------------------------------
    // Spawning
    // -------------------------------------------------------------------------

    private void SpawnEntry(WaveEntry entry)
    {
        if (entry.zombiePrefab == null || BoardGrid.Instance == null)
        {
            Debug.LogWarning("[WaveManager] Cannot spawn: missing prefab or BoardGrid.");
            return;
        }

        float spawnX = BoardGrid.Instance.GetCellCenter(0, BoardGrid.Instance.columnCount - 1).x + 3f;
        float y      = BoardGrid.Instance.GetCellCenter(entry.row, 0).y;

        var go    = Instantiate(entry.zombiePrefab, new Vector3(spawnX, y, 0f), Quaternion.identity);
        var agent = go.GetComponent<ZombieAgent>();
        if (agent != null) agent.Init(entry.row);

        Debug.Log($"[WaveManager] Spawned {entry.zombiePrefab.name} in row {entry.row}.");
    }

    // -------------------------------------------------------------------------
    // Default wave generator
    // -------------------------------------------------------------------------

    private WaveEntry[] BuildDefaultWave()
    {
        if (defaultZombiePrefab == null)
        {
            Debug.LogWarning("[WaveManager] defaultZombiePrefab not assigned; wave will be empty.");
            return new WaveEntry[0];
        }

        // 10 zombies cycling through rows 0-4, one every 4 seconds.
        int[]   rows  = { 2, 0, 4, 1, 3, 2, 3, 0, 4, 1 };
        var     wave  = new WaveEntry[rows.Length];

        for (int i = 0; i < rows.Length; i++)
        {
            wave[i] = new WaveEntry
            {
                spawnTime    = i * 4f,
                row          = rows[i],
                zombiePrefab = defaultZombiePrefab
            };
        }

        return wave;
    }
}

// ---------------------------------------------------------------------------
// Wave data
// ---------------------------------------------------------------------------

/// <summary>One zombie spawn event in a wave schedule.</summary>
[System.Serializable]
public class WaveEntry
{
    [Tooltip("Seconds after wave start when this zombie spawns.")]
    public float      spawnTime;

    [Tooltip("Board row (0–4) the zombie spawns in.")]
    [Range(0, 4)]
    public int        row;

    [Tooltip("Prefab to instantiate.")]
    public GameObject zombiePrefab;
}
