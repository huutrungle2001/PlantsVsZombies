using UnityEngine;

/// <summary>
/// Agent component for a single zombie.
///
/// Responsibilities:
///   - Track HP and receive damage.
///   - Register with LaneRegistry on Start; unregister on death or board-exit.
///   - Walk left across the board while the game is in the Playing state.
///   - Stop and attack a blocking Plant (Section 6).
///   - Notify GameManager when it crosses the house-side lose boundary.
///   - Die when HP reaches 0.
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

    [Header("Blocking")]
    [Tooltip("Distance (world units) at which the zombie stops and attacks a plant.")]
    [SerializeField] private float attackRange = 0.9f;

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
    private Plant  targetPlant;
    private float  attackTimer;
    private RuntimeAnimatorController walkController;
    private RuntimeAnimatorController eatController;
    private Animator zombieAnimator;

    // -------------------------------------------------------------------------
    // Unity lifecycle
    // -------------------------------------------------------------------------

    private void Awake()
    {
        CurrentHp = maxHp;
        zombieAnimator = GetComponent<Animator>();
    }

    private void Start()
    {
        walkController = ArtLibrary.GetZombieController("NormalZombie");
        eatController  = ArtLibrary.GetZombieController("NormalZombieEat");
        RegisterWithLaneRegistry();
    }

    private void Update()
    {
        if (!IsAlive || GameManager.Instance == null || GameManager.Instance.State != GameState.Playing) return;

        // Clear dead or destroyed target
        if (targetPlant != null && (!targetPlant.IsAlive || targetPlant == null))
            targetPlant = null;

        // Search for a blocking plant when we have no current target
        if (targetPlant == null && BoardGrid.Instance != null)
        {
            var candidate = BoardGrid.Instance.GetBlockingPlant(Row, transform.position.x);
            if (candidate != null && IsWithinAttackRange(candidate))
                targetPlant = candidate;
        }

        if (targetPlant != null)
        {
            // Attacking – stay still, damage plant on interval
            SetAnimatorAttacking(true);
            attackTimer += Time.deltaTime;
            if (attackTimer >= attackInterval)
            {
                attackTimer = 0f;
                targetPlant.TakeDamage(attackDamage);
            }
        }
        else
        {
            // Walking
            SetAnimatorAttacking(false);
            transform.Translate(Vector3.left * speed * Time.deltaTime);
            if (transform.position.x <= loseX)
            {
                Debug.Log($"[ZombieAgent] {name} reached the house side in row {Row}.");
                UnregisterFromLaneRegistry();
                GameManager.Instance.NotifyLose();
                Destroy(gameObject);
            }
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
    // Attack helpers
    // -------------------------------------------------------------------------

    private bool IsWithinAttackRange(Plant plant) =>
        transform.position.x <= plant.transform.position.x + attackRange;

    private void SetAnimatorAttacking(bool attacking)
    {
        if (zombieAnimator == null) return;
        var target = attacking ? eatController : walkController;
        if (target != null && zombieAnimator.runtimeAnimatorController != target)
            zombieAnimator.runtimeAnimatorController = target;
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
