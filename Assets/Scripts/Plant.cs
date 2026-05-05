using UnityEngine;

/// <summary>
/// Base plant component shared by every placeable plant.
///
/// Responsibilities:
///   - Store HP and receive damage.
///   - Die when HP reaches 0, clearing the tile and optionally playing a sound.
///   - Record grid position (row/column) and bind to an occupied Tile.
///   - Set SpriteRenderer sort order so lower rows appear in front.
///
/// Specific plant behaviour (shooting, sun generation, etc.) belongs in
/// separate agent components (PeashooterAgent, SunflowerAgent) attached
/// to the same GameObject.
/// </summary>
public class Plant : MonoBehaviour
{
    // -------------------------------------------------------------------------
    // Inspector fields
    // -------------------------------------------------------------------------

    [Header("Stats")]
    [Tooltip("Maximum hit points for this plant.")]
    [SerializeField] private int maxHp = 3;

    [Tooltip("Sun cost shown on the card UI. Must match GameManager's cost field.")]
    [SerializeField] private int sunCost = 100;

    [Header("Audio")]
    [Tooltip("Optional clip played when this plant is destroyed.")]
    [SerializeField] private AudioClip deathSound;

    // -------------------------------------------------------------------------
    // Public read-only state
    // -------------------------------------------------------------------------

    public int MaxHp  => maxHp;
    public int SunCost => sunCost;
    public int CurrentHp { get; private set; }
    public bool IsAlive   => CurrentHp > 0;

    // Grid position – written once by Init() and read by ZombieAgent / LaneRegistry.
    [HideInInspector] public int row;
    [HideInInspector] public int column;

    // -------------------------------------------------------------------------
    // Private state
    // -------------------------------------------------------------------------

    private Tile occupiedTile;

    // -------------------------------------------------------------------------
    // Unity lifecycle
    // -------------------------------------------------------------------------

    private void Awake()
    {
        CurrentHp = maxHp;
    }

    // -------------------------------------------------------------------------
    // Initialisation
    // -------------------------------------------------------------------------

    /// <summary>
    /// Called by GameManager immediately after instantiation.
    /// Binds this plant to its tile and records grid coordinates.
    /// Also sets SpriteRenderer sort order so plants in lower (closer)
    /// rows render in front of plants in higher (farther) rows.
    /// </summary>
    public void Init(Tile tile)
    {
        occupiedTile = tile;
        row    = tile.row;
        column = tile.column;

        // Row 0 = top of board, row 4 = bottom (closest to camera).
        // Higher sortingOrder renders in front, so bottom rows get the highest value.
        var sr = GetComponent<SpriteRenderer>();
        if (sr != null)
        {
            sr.sortingOrder = 10 + row;
        }
    }

    // -------------------------------------------------------------------------
    // Damage and death
    // -------------------------------------------------------------------------

    /// <summary>
    /// Reduces current HP by <paramref name="amount"/>.
    /// Calls Die() automatically when HP reaches 0.
    /// </summary>
    public void TakeDamage(int amount)
    {
        if (!IsAlive || amount <= 0)
        {
            return;
        }

        CurrentHp -= amount;
        Debug.Log($"[Plant] {name} took {amount} damage. HP: {CurrentHp}/{maxHp}");

        if (CurrentHp <= 0)
        {
            Die();
        }
    }

    /// <summary>
    /// Kills this plant: clears its tile, plays the death sound, and destroys
    /// the GameObject. Safe to call directly (e.g. shovel tool in a later phase).
    /// </summary>
    public void Die()
    {
        if (deathSound != null)
        {
            AudioSource.PlayClipAtPoint(deathSound, transform.position);
        }

        ClearTile();
        Debug.Log($"[Plant] {name} died at ({row},{column}).");
        Destroy(gameObject);
    }

    // -------------------------------------------------------------------------
    // Tile management
    // -------------------------------------------------------------------------

    /// <summary>
    /// Releases the occupied tile so another plant can be placed there.
    /// Called automatically by Die(); also available to subclass agents.
    /// </summary>
    protected void ClearTile()
    {
        if (occupiedTile != null)
        {
            occupiedTile.ClearOccupant();
            occupiedTile = null;
        }
    }
}
