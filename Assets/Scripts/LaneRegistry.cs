using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Authoritative registry of every active ZombieAgent on the board, indexed by row.
///
/// Rules:
///   - ZombieAgent registers itself in Start() and unregisters in Die().
///   - Projectiles and PeashooterAgent query this registry instead of scene-searching.
///   - WaveManager queries GetActiveZombieCount() to detect when a wave is fully cleared.
/// </summary>
public class LaneRegistry : MonoBehaviour
{
    // -------------------------------------------------------------------------
    // Singleton
    // -------------------------------------------------------------------------

    public static LaneRegistry Instance { get; private set; }

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
    // Internal state
    // -------------------------------------------------------------------------

    // Row (0–4) → list of living zombies in that row.
    private readonly Dictionary<int, List<ZombieAgent>> zombiesByRow =
        new Dictionary<int, List<ZombieAgent>>();

    // -------------------------------------------------------------------------
    // Registration
    // -------------------------------------------------------------------------

    /// <summary>Called by ZombieAgent on Start to add itself to the registry.</summary>
    public void Register(ZombieAgent zombie)
    {
        if (zombie == null) return;

        if (!zombiesByRow.ContainsKey(zombie.Row))
            zombiesByRow[zombie.Row] = new List<ZombieAgent>();

        if (!zombiesByRow[zombie.Row].Contains(zombie))
        {
            zombiesByRow[zombie.Row].Add(zombie);
            Debug.Log($"[LaneRegistry] Registered {zombie.name} in row {zombie.Row}. " +
                      $"Row total: {zombiesByRow[zombie.Row].Count}");
        }
    }

    /// <summary>
    /// Called by ZombieAgent on Die() or when it leaves the board.
    /// Safe to call multiple times (idempotent).
    /// </summary>
    public void Unregister(ZombieAgent zombie)
    {
        if (zombie == null) return;

        if (zombiesByRow.TryGetValue(zombie.Row, out var list))
        {
            if (list.Remove(zombie))
            {
                Debug.Log($"[LaneRegistry] Unregistered {zombie.name} from row {zombie.Row}. " +
                          $"Row total: {list.Count}");
            }
        }
    }

    // -------------------------------------------------------------------------
    // Queries
    // -------------------------------------------------------------------------

    /// <summary>Returns true when at least one zombie is alive in the given row.</summary>
    public bool HasZombiesInRow(int row)
    {
        return zombiesByRow.TryGetValue(row, out var list) && list.Count > 0;
    }

    /// <summary>
    /// Returns the zombie with the smallest X position that is still to the right
    /// of <paramref name="afterX"/> in the specified row — i.e. the first zombie
    /// a projectile fired from <paramref name="afterX"/> would encounter.
    ///
    /// Returns null when no qualifying zombie exists.
    /// </summary>
    public ZombieAgent GetFirstZombieInRow(int row, float afterX)
    {
        if (!zombiesByRow.TryGetValue(row, out var list) || list.Count == 0)
            return null;

        ZombieAgent nearest = null;
        float nearestX = float.MaxValue;

        foreach (var z in list)
        {
            if (z == null) continue;
            float x = z.transform.position.x;
            // Smallest X that is still to the right of afterX.
            if (x > afterX && x < nearestX)
            {
                nearestX = x;
                nearest = z;
            }
        }

        return nearest;
    }

    /// <summary>Total number of zombies registered across all rows.</summary>
    public int GetActiveZombieCount()
    {
        int total = 0;
        foreach (var list in zombiesByRow.Values)
            total += list.Count;
        return total;
    }
}
