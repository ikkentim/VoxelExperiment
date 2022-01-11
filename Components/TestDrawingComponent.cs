using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MyGame.Data;
using MyGame.Rendering.Effects;
using MyGame.Rendering.Vertices;
using MyGame.World;
using MyGame.World.Blocks;

namespace MyGame.Components;

public class TestDrawingComponent : DrawableGameComponent
{
    private BasicEffect? _effect;
    private BlockFaceEffect? _blockFaceEffect;

    public TestDrawingComponent(Game game) : base(game)
    {
    }
    
    private new VoxelGame Game => (VoxelGame)base.Game;

    public override void Initialize()
    {
        _effect = new BasicEffect(GraphicsDevice);


        _blockFaceEffect = new BlockFaceEffect(Game.Content.Load<Effect>("BlockFaceEffect"));

        base.Initialize();
    }

    public override void Update(GameTime gameTime)
    {
        var ray = new Ray(Game.Camera.Transform.Position, Game.Camera.Transform.Forward);

        Cast(ray);
    }

    private IntVector3 _drawPos;

    private void Cast(Ray ray)
    {
        var hit = Raycast(ray.Position, ray.Direction, 100f);
        _drawPos = hit.Position;
    }

    public struct RayHitInfo
    {
        public bool IsHit { get; }
        public float Distance { get; }
        public BlockData Block { get; }
        public IntVector3 Position { get; }

        public RayHitInfo(bool isHit, float distance, BlockData block, IntVector3 position)
        {
            IsHit = isHit;
            Distance = distance;
            Block = block;
            Position = position;
        }
    }

    RayHitInfo Raycast(Vector3 origin, Vector3 direction, float radius)
    {
        // http://www.cse.yorku.ca/~amana/research/grid.pdf

        var xyz = IntVector3.FromVector(origin);
        var step = new IntVector3(
            direction.X < 0 ? -1 : direction.X > 0 ? 1 : 0,
            direction.Y < 0 ? -1 : direction.Y > 0 ? 1 : 0,
            direction.Z < 0 ? -1 : direction.Z > 0 ? 1 : 0
        );

        var tDelta = Vector3.One / VectorHelper.Abs(direction);

        var dist = new Vector3(
            step.X > 0 ? xyz.X + 1 - origin.X : origin.X - xyz.X,
            step.Y > 0 ? xyz.Y + 1 - origin.Y : origin.Y - xyz.Y,
            step.Z > 0 ? xyz.Z + 1 - origin.Z : origin.Z - xyz.Z
        );

        var tMax = tDelta * dist;
        var distance = 0f;

        do
        {
            if (tMax.X < tMax.Y)
            {
                if (tMax.X < tMax.Z)
                {
                    xyz.X += step.X;
                    tMax.X += tDelta.X;
                    distance += tDelta.X;
                }
                else
                {
                    xyz.Z += step.Z;
                    tMax.Z += tDelta.Z;
                    distance += tDelta.Z;
                }
            }
            else
            {
                if (tMax.Y < tMax.Z)
                {
                    xyz.Y += step.Y;
                    tMax.Y += tDelta.Y;
                    distance += tDelta.Y;
                }
                else
                {
                    xyz.Z += step.Z;
                    tMax.Z += tDelta.Z;
                    distance += tDelta.Z;
                }
            }

            var block = Game.WorldManager.GetBlock(xyz);

            if (block.Kind is not AirBlock)
            {
                return new RayHitInfo(true, distance, block, xyz);
            }
        } while (distance < radius);

        return new RayHitInfo(false, 0, default, default);
    }

    private void ApplyTranslation(IEffectMatrices effect, Vector3 translation)
    {
        effect!.View = Game.Camera.ViewMatrix;
        effect.Projection = GlobalGameContext.Current.Projection;
        effect.World = Matrix.CreateTranslation(translation);
    }

    public override void Draw(GameTime gameTime)
    {
        var cottonBlue = Game.TextureRegistry.GetTexture("cotton_blue"); // one texture of the bigger atlas

        // Draw a simple line
        ApplyTranslation(_effect!, Vector3.One * 3);
        _effect!.TextureEnabled = false;
        _effect.VertexColorEnabled = true;

        DrawLine(Vector3.Up, Color.Purple);
        
        // Draw with custom effect
        ApplyTranslation(_blockFaceEffect!, Vector3.One * 3);
        _blockFaceEffect!.Texture = cottonBlue.Texture;
        _blockFaceEffect.TextureSize = new Vector2(1f / 7, 1);
        
        foreach (var pass in _blockFaceEffect!.CurrentTechnique.Passes)
        {
            pass.Apply();
            Draw4X1(new Vector2(4, 1));
        }


        // Draw selection effect
        _blockFaceEffect.DrawLines = true;
        _blockFaceEffect.LineColor = Color.Black;
        ApplyTranslation(_blockFaceEffect!, _drawPos);

        foreach (var pass in _blockFaceEffect!.CurrentTechnique.Passes)
        {
            pass.Apply();
            const float min = -0.001f;
            const float max = 1.001f;
            GraphicsDevice.DrawUserIndexedPrimitives(PrimitiveType.LineList, new[]
            {
                new VertexPosition(new Vector3(min, min, min)),
                new VertexPosition(new Vector3(max, min, min)),
                new VertexPosition(new Vector3(min, max, min)),
                new VertexPosition(new Vector3(max, max, min)),
                new VertexPosition(new Vector3(min, min, max)),
                new VertexPosition(new Vector3(max, min, max)),
                new VertexPosition(new Vector3(min, max, max)),
                new VertexPosition(new Vector3(max, max, max)),
            }, 0, 8, new short[]
            {
                0, 1, 1, 5, 5, 4, 4, 0, 0, 2, 1, 3, 5, 7, 4, 6, 2, 3, 3, 7, 7, 6, 6, 2
            }, 0, 12);
        }

        _blockFaceEffect.DrawLines = false;


        
        // Draw texture atlas with basic effect
        ApplyTranslation(_effect!, new Vector3(4, 6, 4));
        _effect!.Texture = cottonBlue.Texture;
        _effect.TextureEnabled = true;
        _effect.VertexColorEnabled = false;
        _effect.CurrentTechnique.Passes[0].Apply();
        Draw4X1Basic(Vector2.One);
    }

    private void Draw4X1(Vector2 uv)
    {
        GraphicsDevice.DrawUserIndexedPrimitives(PrimitiveType.TriangleList, new[]
            {
                new VertexPositionBlockFace(new Vector3(0, 0, 0), uv * new Vector2(1, 1), new Vector2(4f/7, 0)),
                new VertexPositionBlockFace(new Vector3(4, 0, 0), uv * new Vector2(0, 1), new Vector2(4f/7, 0)),
                new VertexPositionBlockFace(new Vector3(0, 1, 0), uv * new Vector2(1, 0), new Vector2(4f/7, 0)),
                new VertexPositionBlockFace(new Vector3(4, 1, 0), uv * new Vector2(0, 0), new Vector2(4f/7, 0))
            },
            0, 4, new short[] { 0, 3, 2, 0, 1, 3 }, 0, 2);
    }

    private void Draw4X1Basic(Vector2 uv)
    {
        GraphicsDevice.DrawUserIndexedPrimitives(PrimitiveType.TriangleList, new[]
            {
                new VertexPositionTexture(new Vector3(0, 0, 0), uv * new Vector2(1, 1)),
                new VertexPositionTexture(new Vector3(7, 0, 0), uv * new Vector2(0, 1)),
                new VertexPositionTexture(new Vector3(0, 1, 0), uv * new Vector2(1, 0)),
                new VertexPositionTexture(new Vector3(7, 1, 0), uv * new Vector2(0, 0))
            },
            0, 4, new short[] { 0, 3, 2, 0, 1, 3 }, 0, 2);
    }

    private void DrawLine(Vector3 normal, Color c)
    {
        _effect!.CurrentTechnique.Passes[0].Apply();

        GraphicsDevice.DrawUserIndexedPrimitives(PrimitiveType.LineList,
            new[]
            {
                new VertexPositionColor(Vector3.Zero, c),
                new VertexPositionColor(normal, c),
            },
            0,
            2,
            new short[] { 0, 1 }, 0, 1);
    }

}