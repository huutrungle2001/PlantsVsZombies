using UnityEngine;

/// <summary>
/// Temporary zombie spawner used before WaveManager (Section 11) is implemented.
/// Instantiates the BasicZombie prefab at regular intervals in random lanes and
/// calls ZombieAgent.Init() so each zombie knows which row it belongs to.
///
/// Uses BoardGrid as the authoritative source for lane Y positions.
/// This component will be replaced by WaveManager, which uses a scheduled spawn
/// table instead of endless random spawning.
/// </summary>
public class ZombieSpawner : MonoBehaviour
{
    [Header("References")]
    [Tooltip("Assign the BasicZombie prefab here.")]
    [SerializeField] private GameObject zombiePrefab;
    [Tooltip("Authoritative board grid – set by SceneArtSetup or the Inspector.")]
    [SerializeField] private BoardGrid boardGrid;

    [Header("Timing")]
    [SerializeField] private float spawnInterval    = 2.5f;
    [SerializeField] private float minSpawnInterval = 1.2f;

    [Header("Board Limits")]
    [Tooltip("X position (right of the board) where zombies first appear.")]
    [SerializeField] private float spawnX = 8f;

    private float timer;

    private void Update()
    {
        if (boardGrid == null || zombiePrefab == null) return;

        // WaveManager takes over spawning when present – stand down.
        if (WaveManager.Instance != null) return;

        // Pause spawning while the game is not actively playing.
        if (GameManager.Instance != null &&
            GameManager.Instance.State != GameState.Playing)
        {
            return;
        }

        timer += Time.deltaTime;
        if (timer >= spawnInterval)
        {
            timer = 0f;
            SpawnZombie();
            // Gradually tighten the spawn interval, down to the configured minimum.
            spawnInterval = Mathf.Max(minSpawnInterval, spawnInterval * 0.98f);
        }
    }

    private void SpawnZombie()
    {
        int row = Random.Range(0, boardGrid.rowCount);

        // Use column 0's Y centre – all columns share the same Y for a given row.
        float y = boardGrid.GetCellCenter(row, 0).y;
        var pos = new Vector3(spawnX, y, 0f);

        var go = Instantiate(zombiePrefab, pos, Quaternion.identity);

        // Assign the lane row BEFORE Start() fires so the registry gets it right.
        var agent = go.GetComponent<ZombieAgent>();
        if (agent != null)
            agent.Init(row);
        else
            Debug.LogWarning("[ZombieSpawner] Spawned prefab has no ZombieAgent component.");
    }
}
