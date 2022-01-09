using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using MyGame.Components;
using MyGame.Data;
using MyGame.Platform;
using MyGame.Rendering;
using MyGame.World;
using MyGame.World.Blocks;

namespace MyGame;

public class VoxelGame : Game
{
    public Camera Camera { get; } = new();
    public WorldManager WorldManager { get; } = new();

    private readonly GraphicsDeviceManager _graphics;

    public BlockRegistry BlockRegistry { get; } = new();
    public TextureRegistry TextureRegistry { get; } = new();

    public VoxelGame()
    {
        Content.RootDirectory = "Content";

        _graphics = new GraphicsDeviceManager(this);

        IsMouseVisible = true;
    }
    
    protected override void Initialize()
    {
        GlobalGameContext.Initialize(this);
        
        InitializeDisplay();
        
        foreach (var component in GetComponents())
        {
            Components.Add(component);
        }

        foreach (var block in GetBlocks())
        {
            BlockRegistry.Register(block);
        }

        BlockRegistry.Lock();

        TextureRegistry.RegisterBlockTextures(BlockRegistry.GetBlockTypes());
        
        base.Initialize();
    }

    private static IEnumerable<Block> GetBlocks()
    {
        yield return AirBlock.Instance;
        yield return new CobbleBlock();
        yield return new DefaultBlock();
        yield return new TestBlock();
        yield return new SolidBlock("cotton_blue", "cotton_blue");
        yield return new SolidBlock("cotton_green", "cotton_green");
        yield return new SolidBlock("cotton_red", "cotton_red");
        yield return new SolidBlock("cotton_tan", "cotton_tan");
        yield return new SolidBlock("dirt", "dirt");
        yield return new GrassBlock();
    }

    private IEnumerable<IGameComponent> GetComponents()
    {
        yield return new WorldRendererGameComponent(this);
        yield return new PlayerControllerComponent(this);
        yield return new TestDrawingComponent(this);
        yield return new DebuggingDrawingComponent(this);
    }

    private Chunk GetTestChunk()
    {
        var chunk = new Chunk(WorldManager, new IntVector3(0, 0, 0));

        void Set(int x, int y, int z, string block) => chunk.SetBlock(new IntVector3(x, y, z), new BlockData
        {
            Kind = BlockRegistry.GetBlock(block)
        });


        for (var x = 0; x < Chunk.Size; x++)
        for (var y = 0; y < 3; y++)
        for (var z = 0; z < Chunk.Size; z++)
            Set(x, y, z, y == 2 ? "grass" : "dirt");

        Set(7, 3, 7, "cobblestone");
        Set(7, 7, 7, "cobblestone");
        
        Set(7, 7, 8, "dirt");
        Set(7, 7, 9, "dirt");
        Set(7, 7, 10, "dirt");

        return chunk;
    }

    private void InitializeDisplay()
    {
        TargetElapsedTime = TimeSpan.FromMilliseconds(1);
        MaxElapsedTime = TimeSpan.FromMilliseconds(1000f/30);

        _graphics.PreferredBackBufferWidth = GraphicsDevice.DisplayMode.Width - 400;
        _graphics.PreferredBackBufferHeight = GraphicsDevice.DisplayMode.Height - 400;
        // _graphics.IsFullScreen = true;
        _graphics.ApplyChanges();
    }

    protected override void LoadContent()
    {
        TextureRegistry.CreateTextureAtlasesAndLockRegistry(GraphicsDevice);

        // Load chunks after render is initialized so chunkrenderers are created.
        WorldManager.LoadChunk(GetTestChunk());

        base.LoadContent();
    }
    
    protected override void Draw(GameTime gameTime)
    {
        GraphicsDevice.Clear(Color.CornflowerBlue);
        
        base.Draw(gameTime);
    }
}