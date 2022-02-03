using System;
using System.Collections.Generic;
using System.Diagnostics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using MyGame.Extensions;
using MyGame.Rendering;
using MyGame.Rendering.Meshing;
using MyGame.World;

namespace MyGame;

public class WorldRenderingGameComponent : DrawableGameComponent
{
    private static readonly Vector4[] FrustumCorners =
    {
        new(-1.0f, -1.0f, 1.0f, 1.0f),
        new(-1.0f, -1.0f, 0, 1.0f),
        new(-1.0f, 1.0f, 1.0f, 1.0f),
        new(-1.0f, 1.0f, 0, 1.0f),
        new(1.0f, -1.0f, 1.0f, 1.0f),
        new(1.0f, -1.0f, 0, 1.0f),
        new(1.0f, 1.0f, 1.0f, 1.0f),
        new(1.0f, 1.0f, 0, 1.0f)
    };

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
    
    private RenderTarget2D? _shadowMap;

    public RenderTarget2D ShadowMap => _shadowMap!;

    protected override void LoadContent()
    {
        _rendererResources = new ChunkRendererResources(Game.TextureRegistry);
        _rendererResources.LoadContent(Game);

        _shadowMap = new RenderTarget2D(GraphicsDevice,
            4000, 4000,
            false,
            SurfaceFormat.Single,
            DepthFormat.Depth24, 0, RenderTargetUsage.DiscardContents, false, 4);

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

    // private float[] ttt = new float[]
    // {
    //     0.0001f,
    //     10f,
    //     25f,
    //     60f,
    //     128f
    // };
    private float[] ttt2 = new float[]
    {
        0.0001f,
        10f,
        9f,
        25f,
        24f,
        60f,
        59f,
        128f
    };
    private (Matrix view, Matrix projection) GetShadowMapMatrix(Vector3 lightDirection, int mapIndex)
    {

        var tinyProj = Matrix.CreatePerspectiveFieldOfView(MathHelper.PiOver4, // 90 fov
            GlobalGameContext.Current.Window.ClientBounds.Width / (float)GlobalGameContext.Current.Window.ClientBounds.Height,
            ttt2[mapIndex * 2], ttt2[mapIndex * 2 + 1]);

        var inverseVp = Matrix.Invert(Game.Camera.ViewMatrix * tinyProj);
        
        Span<Vector3> cornersWs = stackalloc Vector3[8];
        for (var i = 0; i < 8; i++)
        {
            var vec = Vector4.Transform(FrustumCorners[i], inverseVp);
            vec /= vec.W;
            cornersWs[i] = vec.ToXyz();
        }

        // Find the centroid
        var frustumCentroid = VectorHelper.Centeroid(ref cornersWs);

        // Position the shadow-caster camera so that it's looking at the centroid,
        // and backed up in the direction of the sunlight
        const float nearClipOffset = 64;
        const float farZ = 20f;
        const float distFromCentroid = farZ + nearClipOffset;
        var lightPoint = frustumCentroid - lightDirection.Normalized() * distFromCentroid;

        var viewMatrix = Matrix.CreateLookAt(lightPoint, frustumCentroid, new Vector3(0,1,0));
        
        // Determine the position of the frustum corners in light space
        Span<Vector3> cornersLs = stackalloc Vector3[8];

        for (var i = 0; i < 8; i++)
        {
            cornersLs[i] = Vector3.Transform(cornersWs[i], viewMatrix);
        }
        
        // Calculate an orthographic projection by sizing a bounding box
        // to the frustum coordinates in light space
        var (minX, minY, minZ) = cornersLs[0];
        var (maxX, maxY, maxZ) = cornersLs[0];

        for (var i = 0; i < 8; i++)
        {
            if (cornersLs[i].X > maxX)		
                maxX = cornersLs[i].X;	
            else if (cornersLs[i].X < minX)		
                minX = cornersLs[i].X;
            if (cornersLs[i].Y > maxY)	
                maxY = cornersLs[i].Y;	
            else if (cornersLs[i].Y < minY)	
                minY = cornersLs[i].Y;
            if (cornersLs[i].Z > maxZ)	
                maxZ = cornersLs[i].Z;	
            else if (cornersLs[i].Z < minZ)	
                minZ = cornersLs[i].Z;
        }
        
        // Create an orthographic camera for use as a shadow caster
        var projectionMatrix = Matrix.CreateOrthographicOffCenter(minX, maxX, minY, maxY, -maxZ - nearClipOffset, -minZ);

        return (viewMatrix, projectionMatrix);
    }

    public override void Draw(GameTime gameTime)
    {
        GraphicsDevice.SamplerStates[0] = SamplerState.PointWrap;

        // Draw shadow map first
        var lightDirection = new Vector3(1, -1, 1).Normalized();
        
        _rendererResources!.BlockFaceEffect.IsShadowMap = true;

        for (var i = 0; i < 4; i++)
        {
            var (view, proj) = GetShadowMapMatrix(lightDirection, i);

            GraphicsDevice.SetRenderTarget(_shadowMap, i);

            GraphicsDevice.Clear(Color.White);

            _rendererResources.BlockFaceEffect.View = view;
            _rendererResources.BlockFaceEffect.Projection = proj;

            _rendererResources.BlockFaceEffect.SetLightViewProjection(i, view * proj);
            RenderChunks();
        }

        _rendererResources.BlockFaceEffect.IsShadowMap = false;

        GraphicsDevice.SetRenderTarget(GlobalGameContext.Current.RenderTarget);
        
        // Render scene
        _rendererResources.BlockFaceEffect.View = _camera.ViewMatrix;
        _rendererResources.BlockFaceEffect.Projection = GlobalGameContext.Current.Projection;
        
        _rendererResources.BlockFaceEffect.ShadowMap = _shadowMap;
        _rendererResources.BlockFaceEffect.ShadowMapSize = new Vector2(4000, 4000);
        _rendererResources.BlockFaceEffect.LightDirection = lightDirection;
        
        RenderChunks();
        
        _rendererResources.BlockFaceEffect.ShadowMap = null;
    }

    private void RenderChunks()
    {
        foreach (var (chunk, mesh) in _meshes)
        {
            _rendererResources.BlockFaceEffect.World = Matrix.CreateTranslation(chunk.WorldPosition);
            mesh.Render(GraphicsDevice, _rendererResources!);
        }
    }
}