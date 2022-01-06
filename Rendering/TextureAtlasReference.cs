using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace MyGame.Rendering;

public struct TextureAtlasReference
{
    public Texture2D Texture;
    public Vector2 Uv;
    public Vector2 UvSize;

    public TextureAtlasReference(Texture2D texture, Vector2 uv, Vector2 uvSize)
    {
        Texture = texture;
        Uv = uv;
        UvSize = uvSize;
    }
}