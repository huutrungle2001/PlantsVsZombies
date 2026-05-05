using UnityEngine;

public class Tile : MonoBehaviour
{
    public BoardGrid boardGrid;
    public int row;
    public int column;
    public Color baseColor = new Color(1f, 1f, 1f, 0.12f);
    public Color hoverColor = new Color(1f, 0.95f, 0.35f, 0.35f);

    private SpriteRenderer spriteRenderer;

    private void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        if (spriteRenderer != null)
        {
            spriteRenderer.sprite = SimpleSprite.Create(Color.white);
            ApplyColor(baseColor);
        }
    }

    private void OnMouseEnter()
    {
        ApplyColor(hoverColor);
    }

    private void OnMouseExit()
    {
        ApplyColor(baseColor);
    }

    private void OnMouseDown()
    {
        Debug.Log("Clicked tile row " + row + ", column " + column);
    }

    private void ApplyColor(Color color)
    {
        if (spriteRenderer != null)
        {
            spriteRenderer.color = color;
        }
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = baseColor;
        Gizmos.DrawCube(transform.position, transform.lossyScale);
    }
}
