using UnityEngine;

/// <summary>
/// Periodically spawns a SunPickupAgent near the Sunflower.
/// Requires a Plant sibling component (provided after placement via Plant.Init).
/// </summary>
public class SunflowerAgent : MonoBehaviour
{
    [Header("Sun Generation")]
    [Tooltip("Seconds between sun spawns.")]
    [SerializeField] private float sunInterval = 7f;

    [Tooltip("Prefab for the sun orb pickup.")]
    [SerializeField] private GameObject sunPickupPrefab;

    [Tooltip("Random offset radius when spawning the orb.")]
    [SerializeField] private float spawnRadius = 0.4f;

    private Plant plant;
    private float timer;

    private void Awake()
    {
        plant = GetComponent<Plant>();
    }

    private void Start()
    {
        // Stagger the first spawn so multiple Sunflowers don't all fire at once.
        timer = Random.Range(0f, sunInterval * 0.5f);
    }

    private void Update()
    {
        if (GameManager.Instance == null ||
            GameManager.Instance.State != GameState.Playing) return;

        if (plant == null || !plant.IsAlive) return;

        timer += Time.deltaTime;
        if (timer >= sunInterval)
        {
            timer = 0f;
            SpawnSun();
        }
    }

    private void SpawnSun()
    {
        if (sunPickupPrefab == null)
        {
            Debug.LogWarning("[SunflowerAgent] sunPickupPrefab is not assigned.");
            return;
        }

        var offset = new Vector3(
            Random.Range(-spawnRadius, spawnRadius),
            Random.Range(-spawnRadius, spawnRadius),
            0f);

        Instantiate(sunPickupPrefab, transform.position + offset, Quaternion.identity);
    }
}
