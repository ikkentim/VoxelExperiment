using System;
using System.Runtime.InteropServices;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace MyGame.Rendering.Vertices;

[StructLayout(LayoutKind.Sequential, Pack = 1)]
public struct VertexPositionBlockFace : IVertexType
{
    private static readonly VertexDeclaration _declaration = new(
        new VertexElement(0, VertexElementFormat.Vector3, VertexElementUsage.Position, 0),
        new VertexElement(4 * 3, VertexElementFormat.Vector2, VertexElementUsage.TextureCoordinate, 0),
        new VertexElement(4 * 5, VertexElementFormat.Vector2, VertexElementUsage.TextureCoordinate, 1)
    );

    public Vector3 Position;
    public Vector2 TextureCoordinate;
    public Vector2 TextureBase;
    
    public VertexPositionBlockFace(Vector3 position, Vector2 textureCoordinate, Vector2 textureBase)
    {
        Position = position;
        TextureCoordinate = textureCoordinate;
        TextureBase = textureBase;
    }

    VertexDeclaration IVertexType.VertexDeclaration => _declaration;

    public bool Equals(VertexPositionBlockFace other) => Position.Equals(other.Position) && 
                                                         TextureCoordinate.Equals(other.TextureCoordinate) &&
                                                         TextureBase.Equals(other.TextureBase);

    public override bool Equals(object? obj) => obj is VertexPositionBlockFace other && Equals(other);

    public override int GetHashCode() => HashCode.Combine(Position, TextureCoordinate, TextureBase);

    public static bool operator ==(VertexPositionBlockFace lhs, VertexPositionBlockFace rhs) => lhs.Equals(rhs);
    public static bool operator !=(VertexPositionBlockFace lhs, VertexPositionBlockFace rhs) => !lhs.Equals(rhs);
}