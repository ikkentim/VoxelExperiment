using System;
using System.Collections.Generic;
using System.Diagnostics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MyGame.Extensions;
using MyGame.Rendering;
using MyGame.Rendering.Meshing;
using MyGame.World;

namespace MyGame;

public class WorldRenderingGameComponent : DrawableGameComponent
{
    private ChunkRendererResources? _rendererResources;
    private readonly HashSet<Chunk> _dirtyChunksLookup = new();
    private readonly Queue<Chunk> _dirtyChunks = new();
    private readonly GreedyMeshGenerator _meshGenerator;
    private readonly Camera _camera;
    private readonly Dictionary<Chunk, IChunkMesh> _meshes = new();

    public WorldRenderingGameComponent(VoxelGame game) : base(game)
    {
        _meshGenerator = new GreedyMeshGenerator(game.TextureRegistry, false);
        _camera = game.Camera;
    }

    public new VoxelGame Game => (VoxelGame)base.Game;

    protected override void LoadContent()
    {
        _rendererResources = new ChunkRendererResources(Game.TextureRegistry);
        _rendererResources.LoadContent(Game);
        base.LoadContent();
    }
    
    public void MarkChunkDirty(Chunk chunk)
    {
        if (_dirtyChunksLookup.Add(chunk))
        {
            _dirtyChunks.Enqueue(chunk);
        }
    }

    private int _counter;

    public override void Update(GameTime gameTime)
    {
        if (_counter++ == 10)
        {
            // only render chunk meshes ever so often.
            _counter = 0;
        }
        else
        {
            return;
        }

        var maxTimeConsumption = TimeSpan.FromSeconds(1f / 144); // at least 60 fps
        var sw = new Stopwatch();
        sw.Start();

        while (sw.Elapsed < maxTimeConsumption)
        {
            if (!_dirtyChunks.TryDequeue(out var chunk))
            {
                break;
            }

            _dirtyChunksLookup.Remove(chunk);

            // render chunk
            var mesh = _meshGenerator.Create(chunk, GraphicsDevice);

            if (_meshes.TryGetValue(chunk, out var oldMesh))
            {
                oldMesh.Dispose();
            }
            
            _meshes[chunk] = mesh;
        }

        // todo: on chunk unload remove chunk mesh
    }

    public override void Draw(GameTime gameTime)
    {   
        _rendererResources!.BlockFaceEffect.View = _camera.ViewMatrix;
        _rendererResources.BlockFaceEffect.Projection = GlobalGameContext.Current.Projection;
        
        _rendererResources.BlockFaceEffect.LightDirection = new Vector3(-0.5f, 0.5f, -0.6f).Normalized();
        GraphicsDevice.SamplerStates[0] = SamplerState.PointWrap;

        foreach (var kv in _meshes)
        {
            _rendererResources.BlockFaceEffect.World = Matrix.CreateTranslation(kv.Key.WorldPosition);

            kv.Value.Render(GraphicsDevice, _rendererResources!);
        }

        base.Draw(gameTime);
    }
}