﻿using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MyGame.Rendering;

namespace MyGame.Components;

public class TestDrawingComponent : DrawableGameComponent
{
    private BasicEffect? _effect;

    private BlockFaceEffect? _test;

    public TestDrawingComponent(Game game) : base(game)
    {
    }
    
    private new VoxelGame Game => (VoxelGame)base.Game;

    public override void Initialize()
    {
        _effect = new BasicEffect(GraphicsDevice);


        _test = new BlockFaceEffect(Game.Content.Load<Effect>("BlockFaceEffect"));

        base.Initialize();
    }
    
    public override void Draw(GameTime gameTime)
    {
        _effect!.View = Game.Camera.ViewMatrix;
        _effect.Projection = GlobalGameContext.Current.Projection;
        _effect.World = Matrix.CreateTranslation(Vector3.One * 2);
        _effect.TextureEnabled = false;
        _effect.VertexColorEnabled = true;
        
        DrawLine(Vector3.Up, Color.Purple);



        var tx = Game.TextureRegistry.GetTexture("cotton_blue"); // one texture of the bigger atlas


        _test!.World = Matrix.CreateTranslation(Vector3.One * 2);
        _test.View = Game.Camera.ViewMatrix;
        _test.Projection = GlobalGameContext.Current.Projection;
        _test.Texture = tx.Texture;
        _test.TextureUv = new Vector2(1f / 5, 1);
        _test.TextureSize = new Vector2(1f / 5, 1);
        
        foreach (var pass in _test!.CurrentTechnique.Passes)
        {
            pass.Apply();
        }
        
        GraphicsDevice.DrawUserIndexedPrimitives(PrimitiveType.TriangleList, new[]
            {
                new VertexPositionTexture(new Vector3(0, 0, 0), new Vector2(4, 1)),
                new VertexPositionTexture(new Vector3(4, 0, 0), new Vector2(0, 1)),
                new VertexPositionTexture(new Vector3(0, 1, 0), new Vector2(4, 0)),
                new VertexPositionTexture(new Vector3(4, 1, 0), new Vector2(0, 0))
            },
            0, 4, new short[] { 0, 3, 2, 0, 1, 3 }, 0, 2);

        
        _effect.World = Matrix.CreateTranslation(new Vector3(4, 6, 4));
        _effect.Texture = tx.Texture;
        _effect.TextureEnabled = true;
        _effect.VertexColorEnabled = false;
        _effect.CurrentTechnique.Passes[0].Apply();
        
        GraphicsDevice.DrawUserIndexedPrimitives(PrimitiveType.TriangleList, new[]
            {
                new VertexPositionTexture(new Vector3(0, 0, 0), new Vector2(1, 1)),
                new VertexPositionTexture(new Vector3(4, 0, 0), new Vector2(0, 1)),
                new VertexPositionTexture(new Vector3(0, 1, 0), new Vector2(1, 0)),
                new VertexPositionTexture(new Vector3(4, 1, 0), new Vector2(0, 0))
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