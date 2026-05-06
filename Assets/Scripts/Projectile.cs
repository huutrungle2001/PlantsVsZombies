using UnityEngine;

/// <summary>
/// Moves a projectile to the right and damages the first ZombieAgent it collides with
/// in the same lane row.
///
/// Row is set by the plant or spawner immediately after Instantiate.
/// If row is left at -1 (no lane assigned) the row check is skipped so that
/// legacy callers (PlantShooter) keep working until replaced by PeashooterAgent.
/// </summary>
public class Projectile : MonoBehaviour
{
    [Header("Stats")]
    public float speed      = 6f;
    public int   damage     = 1;
    public float maxLifetime = 4f;

    /// <summary>
    /// Board row this projectile belongs to.
    /// Set by the owning plant immediately after Instantiate.
    /// -1 means "no row filter" (hits any zombie).
    /// </summary>
    [HideInInspector] public int row = -1;

    private float age;

    private void Update()
    {
        transform.Translate(Vector3.right * speed * Time.deltaTime);

        age += Time.deltaTime;
        if (age >= maxLifetime)
            Destroy(gameObject);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        var zombie = other.GetComponent<ZombieAgent>();
        if (zombie == null) return;

        // Only damage zombies in the same lane.
        if (row >= 0 && zombie.Row != row) return;

        zombie.TakeDamage(damage);
        Destroy(gameObject);
    }
}
