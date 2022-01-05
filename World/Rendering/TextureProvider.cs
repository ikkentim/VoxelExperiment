using System.Collections.Generic;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace MyGame.World.Rendering
{
    public class TextureProvider
    {
        private readonly Dictionary<string, Texture2D> _textures = new();

        public void LoadContent(ContentManager content)
        {
            void Load(string name)
            {
                _textures[name] = content.Load<Texture2D>(name);
            }

            Load("checkered");
            Load("cobble");
            Load("notex");
            Load("arrow");
        }

        public Texture2D GetTexture(string name)
        {
            return _textures.TryGetValue(name, out var tex) ? tex : _textures["notex"];
        }
    }
}
