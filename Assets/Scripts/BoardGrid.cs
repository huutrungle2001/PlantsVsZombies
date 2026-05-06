using UnityEngine;

public class BoardGrid : MonoBehaviour
{
    // -------------------------------------------------------------------------
    // Singleton
    // -------------------------------------------------------------------------

    public static BoardGrid Instance { get; private set; }

    // -------------------------------------------------------------------------
    // Inspector fields (unchanged)
    // -------------------------------------------------------------------------

    public int rowCount = 5;
    public int columnCount = 9;
    public float tileWidth = 1f;
    public float tileHeight = 1.4f;
    public Vector2 topLeftCellCenter = new Vector2(-4.5f, 2.8f);

    // -------------------------------------------------------------------------
    // Tile registry
    // -------------------------------------------------------------------------

    private Tile[,] tileGrid;

    /// <summary>Called by each Tile in its Start() to register itself.</summary>
    public void RegisterTile(Tile tile)
    {
        if (tile == null) return;
        if (!IsValidCell(tile.row, tile.column)) return;
        tileGrid[tile.row, tile.column] = tile;
    }

    /// <summary>Returns the Tile at (row, col), or null if out of bounds or not yet registered.</summary>
    public Tile GetTile(int row, int col)
    {
        if (!IsValidCell(row, col)) return null;
        return tileGrid[row, col];
    }

    // -------------------------------------------------------------------------
    // Blocking plant query
    // -------------------------------------------------------------------------

    /// <summary>
    /// Scans every column in <paramref name="row"/> and returns the living Plant
    /// whose world X is the largest value that is still &lt;= <paramref name="zombieX"/>.
    /// This is the first plant a left-walking zombie will encounter.
    /// Returns null when no blocking plant exists.
    /// </summary>
    public Plant GetBlockingPlant(int row, float zombieX)
    {
        Plant best = null;
        float bestX = float.NegativeInfinity;

        for (int col = 0; col < columnCount; col++)
        {
            Tile tile = GetTile(row, col);
            if (tile == null) continue;

            Plant plant = tile.OccupantPlant;
            if (plant == null || !plant.IsAlive) continue;

            float plantX = plant.transform.position.x;
            if (plantX <= zombieX && plantX > bestX)
            {
                bestX = plantX;
                best  = plant;
            }
        }

        return best;
    }

    // -------------------------------------------------------------------------
    // Unity lifecycle
    // -------------------------------------------------------------------------

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;

        tileGrid = new Tile[rowCount, columnCount];
    }

    // -------------------------------------------------------------------------
    // Coordinate helpers (unchanged)
    // -------------------------------------------------------------------------

    public Vector3 GetCellCenter(int row, int column)
    {
        return new Vector3(
            topLeftCellCenter.x + column * tileWidth,
            topLeftCellCenter.y - row * tileHeight,
            0f);
    }

    public bool IsValidCell(int row, int column)
    {
        return row >= 0 && row < rowCount && column >= 0 && column < columnCount;
    }

    public bool TryGetCellAtWorld(Vector3 worldPosition, out int row, out int column)
    {
        var localX = worldPosition.x - topLeftCellCenter.x + tileWidth * 0.5f;
        var localY = topLeftCellCenter.y - worldPosition.y + tileHeight * 0.5f;

        column = Mathf.FloorToInt(localX / tileWidth);
        row    = Mathf.FloorToInt(localY / tileHeight);

        return IsValidCell(row, column);
    }

    public bool TryGetCellAtScreen(Vector3 screenPosition, Camera camera, out int row, out int column)
    {
        if (camera == null)
        {
            camera = Camera.main;
        }

        if (camera == null)
        {
            row    = -1;
            column = -1;
            return false;
        }

        var worldPosition = camera.ScreenToWorldPoint(screenPosition);
        return TryGetCellAtWorld(worldPosition, out row, out column);
    }
}
