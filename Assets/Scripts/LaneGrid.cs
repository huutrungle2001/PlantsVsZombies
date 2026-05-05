using UnityEngine;

public class LaneGrid : MonoBehaviour
{
    public int laneCount = 5;
    public float laneSpacing = 1.4f;
    public float centerY = 0f;
    public float plantX = -4.5f;
    public float zombieSpawnX = 7.5f;
    public float despawnX = -8.5f;

    public float GetLaneY(int laneIndex)
    {
        if (laneCount <= 1)
        {
            return centerY;
        }

        var top = centerY + (laneCount - 1) * laneSpacing * 0.5f;
        return top - laneIndex * laneSpacing;
    }
}
