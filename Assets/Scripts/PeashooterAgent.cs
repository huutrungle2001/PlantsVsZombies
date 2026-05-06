using UnityEngine;

/// <summary>
/// Shooting behaviour for the Peashooter plant.
///
/// Fires a PeaProjectile at a fixed interval, but ONLY while at least one
/// ZombieAgent is registered in the same lane row via LaneRegistry.
/// Stops firing once the game is no longer in the Playing state.
///
/// Requires a Plant component on the same GameObject (set by GameManager
/// through Plant.Init after placement).
/// </summary>
public class PeashooterAgent : MonoBehaviour
{
    // -------------------------------------------------------------------------
    // Inspector fields
    // -------------------------------------------------------------------------

    [Header("Shooting")]
    [Tooltip("Seconds between shots.")]
    [SerializeField] private float fireInterval = 1.5f;

    [Tooltip("World-unit offset to the right of the plant centre where peas spawn.")]
    [SerializeField] private float spawnOffsetX = 0.65f;

    [Tooltip("Assign the PeaProjectile prefab here.")]
    [SerializeField] private GameObject projectilePrefab;

    // -------------------------------------------------------------------------
    // Private state
    // -------------------------------------------------------------------------

    private Plant plant;
    private float fireTimer;

    // -------------------------------------------------------------------------
    // Unity lifecycle
    // -------------------------------------------------------------------------

    private void Awake()
    {
        plant = GetComponent<Plant>();
    }

    private void Update()
    {
        if (GameManager.Instance  == null ||
            GameManager.Instance.State != GameState.Playing) return;

        if (plant == null || !plant.IsAlive) return;

        // Only shoot while at least one zombie is in the same lane.
        if (LaneRegistry.Instance == null ||
            !LaneRegistry.Instance.HasZombiesInRow(plant.row)) return;

        fireTimer += Time.deltaTime;
        if (fireTimer >= fireInterval)
        {
            fireTimer = 0f;
            Fire();
        }
    }

    // -------------------------------------------------------------------------
    // Shooting
    // -------------------------------------------------------------------------

    private void Fire()
    {
        if (projectilePrefab == null)
        {
            Debug.LogWarning("[PeashooterAgent] projectilePrefab is not assigned.");
            return;
        }

        var spawnPos = transform.position + new Vector3(spawnOffsetX, 0f, 0f);
        var go       = Instantiate(projectilePrefab, spawnPos, Quaternion.identity);

        var proj = go.GetComponent<ProjectileAgent>();
        if (proj != null)
            proj.Init(plant.row);
    }
}
