using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using MyGame.Components;
using MyGame.Data;
using MyGame.Debugging;
using MyGame.Rendering;
using MyGame.World;
using MyGame.World.Blocks;

namespace MyGame;

public class VoxelGame : Game
{
    private const bool IsFullscreen = false;

    private readonly GraphicsDeviceManager _graphics;
    private RenderTarget2D? _renderTarget;
    private SpriteBatch? _spriteBatch;
    private KeyboardState _keyboardState;

    public VoxelGame()
    {
        Content.RootDirectory = "Content";
        IsMouseVisible = true;

        _graphics = new GraphicsDeviceManager(this);
        _graphics.PreparingDeviceSettings += OnPreparingDeviceSettings;

        BlockOutlineRenderer = new BlockOutlineRenderer(this);
        WorldManager = new WorldManager(this);
        WorldRender = new WorldRenderingGameComponent(this);
    }

    public Camera Camera { get; } = new();
    public WorldManager WorldManager { get; }
    public BlockRegistry BlockRegistry { get; } = new();
    public TextureRegistry TextureRegistry { get; } = new();
    public AssetManager AssetManager { get; } = new();
    public BlockOutlineRenderer BlockOutlineRenderer { get; }
    public WorldRenderingGameComponent WorldRender { get; }

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

    protected override void LoadContent()
    {
        TextureRegistry.CreateTextureAtlasesAndLockRegistry(GraphicsDevice);
        
        // TODO: should happen every few ticks
        WorldManager.UpdateLoadedChunks(IntVector3.Zero);
        
        _renderTarget = new RenderTarget2D(GraphicsDevice, GraphicsDevice.DisplayMode.Width, GraphicsDevice.DisplayMode.Height, false, SurfaceFormat.Rgba64, DepthFormat.Depth24Stencil8);

        _spriteBatch = new SpriteBatch(GraphicsDevice);
        
        base.LoadContent();
    }

    protected override void Update(GameTime gameTime)
    {
        PerformanceCounters.Update.Reset();
        PerformanceCounters.Update.StartMeasurement("update");
        base.Update(gameTime);

        var kb = Keyboard.GetState();
        if (kb.IsKeyDown(Keys.F11) && _keyboardState.IsKeyUp(Keys.F11))
        {
            if (_graphics.IsFullScreen)
            {
                _graphics.PreferredBackBufferWidth = GraphicsDevice.DisplayMode.Width - 400;
                _graphics.PreferredBackBufferHeight = GraphicsDevice.DisplayMode.Height - 400;
                _graphics.IsFullScreen = false;
                _graphics.ApplyChanges();
            }
            else
            {
                _graphics.PreferredBackBufferWidth = GraphicsDevice.DisplayMode.Width;
                _graphics.PreferredBackBufferHeight = GraphicsDevice.DisplayMode.Height;
                _graphics.IsFullScreen = true;
                _graphics.ApplyChanges();
            }
        }

        _keyboardState = kb;

        PerformanceCounters.Update.StopMeasurement();
    }

    protected override void Draw(GameTime gameTime)
    {
        PerformanceCounters.Drawing.StartMeasurement("draw");
        
        // Draw the scene to the render target.
        GlobalGameContext.Current.RenderTarget = _renderTarget!;
        GraphicsDevice.SetRenderTarget(_renderTarget);
        
        GraphicsDevice.Clear(Color.Black);
        base.Draw(gameTime);
        
        // Render the scene from the render target to the back buffer.
        GraphicsDevice.SetRenderTarget(null);
        GraphicsDevice.Clear(Color.Blue);

        _spriteBatch!.Begin( SpriteSortMode.Immediate, GraphicsDevice.BlendState, null, GraphicsDevice.DepthStencilState, RasterizerState.CullNone );
        _spriteBatch.Draw( _renderTarget, GraphicsDevice.Viewport.Bounds, Color.White );
        _spriteBatch.End();
        
        _spriteBatch.Begin(0, BlendState.Opaque, SamplerState.PointClamp, null, null);
        _spriteBatch.Draw(WorldRender.ShadowMap, new Rectangle(0, 0, 250, 250), new Color(0.25f, 0, 0, 1));
        _spriteBatch.End();
        
        // Reset graphics device changes made by the sprite batch.
        GraphicsDevice.DepthStencilState = DepthStencilState.Default;
        GraphicsDevice.RasterizerState = RasterizerState.CullCounterClockwise;
        GraphicsDevice.BlendState = BlendState.AlphaBlend;
        GraphicsDevice.SamplerStates[0] = SamplerState.LinearWrap;

        PerformanceCounters.Drawing.StopMeasurement();
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
        yield return new PlayerControllerComponent(this);
        yield return new TestDrawingComponent(this);
        yield return new DebuggingDrawingComponent(this);
        yield return WorldRender;
    }

    private void InitializeDisplay()
    {
        TargetElapsedTime = TimeSpan.FromMilliseconds(1);
        MaxElapsedTime = TimeSpan.FromMilliseconds(1000f / 30);

        _graphics.PreferredBackBufferWidth = GraphicsDevice.DisplayMode.Width - 400;
        _graphics.PreferredBackBufferHeight = GraphicsDevice.DisplayMode.Height - 400;
        _graphics.IsFullScreen = IsFullscreen;
        _graphics.ApplyChanges();
    }
    
    private void OnPreparingDeviceSettings(object? sender, PreparingDeviceSettingsEventArgs e)
    {
        _graphics.PreferMultiSampling = true;
        e.GraphicsDeviceInformation.PresentationParameters.MultiSampleCount = 8;
    }
}