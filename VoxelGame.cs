using System.Collections.Generic;
using Microsoft.Xna.Framework;
using MyGame.Components;
using MyGame.Rendering;
using MyGame.World;

namespace MyGame;

public class VoxelGame : Game
{
    public Camera Camera { get; } = new();
    public WorldManager WorldManager { get; } = new();
    public TextureProvider TextureProvider { get; } = new();

    private readonly GraphicsDeviceManager _graphics;
    

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
        
        base.Initialize();
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
        _graphics.PreferredBackBufferWidth = GraphicsDevice.DisplayMode.Width - 400;
        _graphics.PreferredBackBufferHeight = GraphicsDevice.DisplayMode.Height - 400;
        // _graphics.IsFullScreen = true;
        _graphics.ApplyChanges();

    }

    protected override void LoadContent()
    {
        TextureProvider.LoadContent(Content);

        // Load chunks after render is initialized so chunkrenderers are created.
        WorldManager.LoadInitialChunks();

        base.LoadContent();
    }
    
    protected override void Draw(GameTime gameTime)
    {
        GraphicsDevice.Clear(Color.CornflowerBlue);
        
        base.Draw(gameTime);
    }
}