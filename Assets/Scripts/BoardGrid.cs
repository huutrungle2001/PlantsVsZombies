using UnityEngine;

public class BoardGrid : MonoBehaviour
{
    public int rowCount = 5;
    public int columnCount = 9;
    public float tileWidth = 1f;
    public float tileHeight = 1.4f;
    public Vector2 topLeftCellCenter = new Vector2(-4.5f, 2.8f);

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
        row = Mathf.FloorToInt(localY / tileHeight);

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
            row = -1;
            column = -1;
            return false;
        }

        var worldPosition = camera.ScreenToWorldPoint(screenPosition);
        return TryGetCellAtWorld(worldPosition, out row, out column);
    }
}
