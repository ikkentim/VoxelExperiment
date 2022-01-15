using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using MyGame.Components;
using MyGame.Data;
using MyGame.Debugging;
using MyGame.Rendering;
using MyGame.World;
using MyGame.World.Blocks;

namespace MyGame;

public class VoxelGame : Game
{
    private readonly GraphicsDeviceManager _graphics;

    public VoxelGame()
    {
        Content.RootDirectory = "Content";

        _graphics = new GraphicsDeviceManager(this);

        IsMouseVisible = true;

        BlockOutlineRenderer = new BlockOutlineRenderer(this);
        WorldManager = new WorldManager(this);
    }

    public Camera Camera { get; } = new();
    public WorldManager WorldManager { get; }

    public BlockRegistry BlockRegistry { get; } = new();
    public TextureRegistry TextureRegistry { get; } = new();
    public AssetManager AssetManager { get; } = new();
    public BlockOutlineRenderer BlockOutlineRenderer { get; }

    protected override void Initialize()
    {
        GlobalGameContext.Initialize(this);

        InitializeDisplay();

        AssetManager.LoadContent(Content);
        BlockOutlineRenderer.LoadContent();

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
        yield return new SolidBlock("stone", "stone");
        yield return new GrassBlock();
    }

    private IEnumerable<IGameComponent> GetComponents()
    {
        yield return new WorldRendererGameComponent(this);
        yield return new PlayerControllerComponent(this);
        yield return new TestDrawingComponent(this);
        yield return new DebuggingDrawingComponent(this);
    }

    private void InitializeDisplay()
    {
        TargetElapsedTime = TimeSpan.FromMilliseconds(1);
        MaxElapsedTime = TimeSpan.FromMilliseconds(1000f / 30);

        _graphics.PreferredBackBufferWidth = GraphicsDevice.DisplayMode.Width - 400;
        _graphics.PreferredBackBufferHeight = GraphicsDevice.DisplayMode.Height - 400;
        //_graphics.IsFullScreen = true;
        _graphics.ApplyChanges();
    }

    protected override void LoadContent()
    {
        TextureRegistry.CreateTextureAtlasesAndLockRegistry(GraphicsDevice);

        // TODO: should happen every few ticks
        WorldManager.UpdateLoadedChunks(IntVector3.Zero);

        base.LoadContent();
    }

    protected override void Update(GameTime gameTime)
    {
        PerformanceCounters.Update.Reset();
        PerformanceCounters.Update.StartMeasurement("update");
        base.Update(gameTime);
        PerformanceCounters.Update.StopMeasurement();
    }

    protected override void Draw(GameTime gameTime)
    {
        PerformanceCounters.Drawing.StartMeasurement("draw");
        GraphicsDevice.Clear(Color.CornflowerBlue);

        base.Draw(gameTime);
        PerformanceCounters.Drawing.StopMeasurement();
    }
}