using System.Collections.Generic;
using UnityEngine;

public static class SimpleSprite
{
    private static readonly Dictionary<Color, Sprite> Sprites = new Dictionary<Color, Sprite>();

    public static Sprite Create(Color color)
    {
        if (Sprites.TryGetValue(color, out var sprite))
        {
            return sprite;
        }

        var texture = new Texture2D(1, 1, TextureFormat.RGBA32, false);
        texture.SetPixel(0, 0, color);
        texture.Apply();
        texture.filterMode = FilterMode.Point;
        texture.wrapMode = TextureWrapMode.Clamp;

        sprite = Sprite.Create(texture, new Rect(0f, 0f, 1f, 1f), new Vector2(0.5f, 0.5f), 1f);
        Sprites[color] = sprite;
        return sprite;
    }
}
