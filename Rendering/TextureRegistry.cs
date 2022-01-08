using System;
using System.CodeDom;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MyGame.World;
using Color = Microsoft.Xna.Framework.Color;
using Image = System.Drawing.Image;
using Rectangle = Microsoft.Xna.Framework.Rectangle;

namespace MyGame.Rendering;

public class TextureRegistry
{
    private const int AtlasWithInTextures = 16;

    private bool _isLocked;
    private readonly List<string> _registeredTextures = new();
    private readonly Dictionary<string, TextureAtlasReference> _assocTable = new();
    
    public void Register(string name)
    {
        if (_isLocked)
        {
            throw new InvalidOperationException("registry is locked");
        }

        if (_registeredTextures.Contains(name))
            return;

        if (!File.Exists($"Tiles/{name}.png"))
        {
            throw new InvalidOperationException("texture not found: " + name);
        }

        _registeredTextures.Add(name);
    }

    public void RegisterBlockTextures(IEnumerable<Block> blocks)
    {
        foreach (var block in blocks)
        {
            foreach (var texture in block.GetTextures())
            {
                Register(texture.Name);
            }
        }
    }

    public TextureAtlasReference GetTexture(string name)
    {
        return _assocTable.TryGetValue(name, out var value)
            ? value
            : _assocTable[Block.DefaultTexture];
    }

    public void CreateTextureAtlasesAndLockRegistry(GraphicsDevice graphicsDevice)
    {
        _isLocked = true;
        _assocTable.Clear();
        
        foreach (var textureGroup in GetTexturesToAddToTable().GroupBy(x => x.image.Size.Width))
        {
            var width = textureGroup.Key;
            var images = textureGroup.ToList();

            if (images.Any(i => i.image.Height != width))
            {
                throw new InvalidOperationException("image not square");
            }

            var atlasWidth = Math.Min(images.Count, AtlasWithInTextures) * width;
            var atlasRows = images.Count / AtlasWithInTextures + (images.Count % AtlasWithInTextures == 0 ? 0 : 1);
            var atlasHeight = atlasRows * width;

            Debug.WriteLine($"Creating texture atlas for textures@{width} with {atlasRows} rows and {images.Count} textures");

            var imageBuffer = new Color[width * width];
            var atlasTexture = new Texture2D(graphicsDevice, atlasWidth, atlasHeight);

            var index = 0;
            var uvSize = new Vector2(width, width) / new Vector2(atlasWidth, atlasHeight);
            
            foreach (var (name, image) in images)
            {
                var bmp = WriteImageToBuffer(image, width, imageBuffer);

                bmp.Dispose();
                
                var indexX = index % AtlasWithInTextures;
                var indexY = index / AtlasWithInTextures;
                var tilePosX = indexX * width;
                var tilePosY = indexY * width;
                atlasTexture.SetData(0, new Rectangle(tilePosX, tilePosY, width, width), imageBuffer, 0, imageBuffer.Length);

                var uv = uvSize * new Vector2(indexX, indexY);

                _assocTable[name] = new TextureAtlasReference(atlasTexture, uv, uvSize);

                index++;
            }
        }
    }

    private static Bitmap WriteImageToBuffer(Image image, int size, Color[] imageBuffer)
    {
        var bmp = (Bitmap)image;

        for (var x = 0; x < size; x++)
        {
            for (var y = 0; y < size; y++)
            {
                var pix = bmp.GetPixel(x, y);
                imageBuffer[y * size + x] = new Color(pix.R, pix.G, pix.B, pix.A);
            }
        }

        return bmp;
    }

    private IEnumerable<(string name, Image image)> GetTexturesToAddToTable()
    {
        return _registeredTextures.Select(name => (name, Image.FromFile($"Tiles/{name}.png")));
    }
}