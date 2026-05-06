using UnityEngine;

/// <summary>
/// Agent component for a single zombie.
///
/// Responsibilities:
///   - Track HP and receive damage.
///   - Register with LaneRegistry on Start; unregister on death or board-exit.
///   - Walk left across the board while the game is in the Playing state.
///   - Notify GameManager when it crosses the house-side lose boundary.
///   - Die when HP reaches 0.
///
/// Blocking and attack behaviour (Section 6) will be layered on top of this base.
/// </summary>
public class ZombieAgent : MonoBehaviour
{
    // -------------------------------------------------------------------------
    // Inspector fields
    // -------------------------------------------------------------------------

    [Header("Stats")]
    [SerializeField] private int maxHp = 10;
    [SerializeField] private float speed = 0.9f;
    [SerializeField] private int attackDamage = 1;
    [SerializeField] private float attackInterval = 1f;

    [Header("Board Limits")]
    [Tooltip("X position at which the zombie reaches the house side and the player loses.")]
    [SerializeField] private float loseX = -6f;

    // -------------------------------------------------------------------------
    // Public read-only state
    // -------------------------------------------------------------------------

    /// <summary>Board row (0–4) this zombie occupies. Set by Init() before Start fires.</summary>
    public int Row { get; private set; }

    public int   MaxHp          => maxHp;
    public int   CurrentHp      { get; private set; }
    public bool  IsAlive        => CurrentHp > 0;
    public int   AttackDamage   => attackDamage;
    public float AttackInterval => attackInterval;

    // -------------------------------------------------------------------------
    // Private state
    // -------------------------------------------------------------------------

    private bool registered;

    // -------------------------------------------------------------------------
    // Unity lifecycle
    // -------------------------------------------------------------------------

    private void Awake()
    {
        CurrentHp = maxHp;
    }

    private void Start()
    {
        RegisterWithLaneRegistry();
    }

    private void Update()
    {
        if (GameManager.Instance == null ||
            GameManager.Instance.State != GameState.Playing)
        {
            return;
        }

        // Walk left.
        transform.Translate(Vector3.left * speed * Time.deltaTime);

        // Lose condition: zombie walked past the house side.
        if (transform.position.x <= loseX)
        {
            Debug.Log($"[ZombieAgent] {name} reached the house side in row {Row}.");
            UnregisterFromLaneRegistry();
            GameManager.Instance.NotifyLose();
            Destroy(gameObject);
        }
    }

    // -------------------------------------------------------------------------
    // Initialisation
    // -------------------------------------------------------------------------

    /// <summary>
    /// Assigns this zombie to a board row. Must be called by the spawner
    /// immediately after Instantiate, before the first Update.
    /// Also sets sorting order so lower (closer) rows render in front.
    /// </summary>
    public void Init(int row)
    {
        Row = row;

        // Row 0 = top (farthest from camera), row 4 = bottom (closest).
        // Higher sortingOrder renders in front.
        var sr = GetComponent<SpriteRenderer>();
        if (sr != null)
            sr.sortingOrder = 10 + row;
    }

    // -------------------------------------------------------------------------
    // Damage and death
    // -------------------------------------------------------------------------

    /// <summary>Reduces HP by <paramref name="amount"/>. Calls Die() when HP hits 0.</summary>
    public void TakeDamage(int amount)
    {
        if (!IsAlive || amount <= 0) return;

        CurrentHp -= amount;
        Debug.Log($"[ZombieAgent] {name} took {amount} damage. HP: {CurrentHp}/{maxHp}");

        if (CurrentHp <= 0)
            Die();
    }

    /// <summary>Kills this zombie: unregisters from LaneRegistry and destroys the GameObject.</summary>
    public void Die()
    {
        UnregisterFromLaneRegistry();
        Debug.Log($"[ZombieAgent] {name} died in row {Row}.");
        Destroy(gameObject);
    }

    // -------------------------------------------------------------------------
    // LaneRegistry helpers
    // -------------------------------------------------------------------------

    private void RegisterWithLaneRegistry()
    {
        if (registered) return;
        if (LaneRegistry.Instance != null)
        {
            LaneRegistry.Instance.Register(this);
            registered = true;
        }
        else
        {
            Debug.LogWarning("[ZombieAgent] LaneRegistry.Instance is null. " +
                             "Ensure a LaneRegistry object exists in the scene.");
        }
    }

    private void UnregisterFromLaneRegistry()
    {
        if (!registered) return;
        if (LaneRegistry.Instance != null)
            LaneRegistry.Instance.Unregister(this);
        registered = false;
    }
}
