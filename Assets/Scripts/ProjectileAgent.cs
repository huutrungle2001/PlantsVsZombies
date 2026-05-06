using UnityEngine;

/// <summary>
/// Moves a pea projectile to the right and damages the first ZombieAgent
/// it collides with in the same lane row.
///
/// Init(laneRow) must be called by the spawning plant immediately after
/// Instantiate so the row filter and sorting order are set correctly.
/// </summary>
public class ProjectileAgent : MonoBehaviour
{
    [Header("Stats")]
    [SerializeField] private float speed       = 7f;
    [SerializeField] private int   damage      = 1;
    [SerializeField] private float maxLifetime = 4f;

    [Header("Audio")]
    [Tooltip("Optional sound played when the projectile hits a zombie.")]
    [SerializeField] private AudioClip hitSound;

    // Lane row – set by Init(). -1 = no lane filter (legacy fallback).
    private int   row = -1;
    private float age;

    // -------------------------------------------------------------------------
    // Initialisation
    // -------------------------------------------------------------------------

    /// <summary>
    /// Assigns this projectile to a lane row and sets its sorting order so it
    /// renders in front of sprites in the same row.
    /// Must be called immediately after Instantiate, before the first Update.
    /// </summary>
    public void Init(int laneRow)
    {
        row = laneRow;
        var sr = GetComponent<SpriteRenderer>();
        if (sr != null)
            sr.sortingOrder = 20 + laneRow; // above plants (10+row) and zombies (10+row)
    }

    // -------------------------------------------------------------------------
    // Unity lifecycle
    // -------------------------------------------------------------------------

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

        // Lane filter: skip zombies in a different row (when row is assigned).
        if (row >= 0 && zombie.Row != row) return;

        zombie.TakeDamage(damage);

        if (hitSound != null)
            AudioSource.PlayClipAtPoint(hitSound, transform.position);

        Destroy(gameObject);
    }
}
