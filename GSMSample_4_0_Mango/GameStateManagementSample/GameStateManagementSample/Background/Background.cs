using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;

namespace GameStateManagement.SideScrollGame
{
    class Background
    {
        public Texture2D texture;
        public Vector2 position;
        public char id;

        public bool walkable;

        public Background()
        {
            texture = null;
            position = new Vector2();
        }

        public Background(char id, Texture2D texture, Vector2 pos, bool walk)
        {
            this.id = id;
            this.texture = texture;
            position = pos;
            this.walkable = walk;
        }

        public Background(Texture2D texture, Vector2 pos, bool walkable)
        {
            this.texture = texture;
            this.position = pos;
            this.walkable = walkable;
        }

        public void Draw(GameTime gameTime, SpriteBatch spriteBatch)
        {
            spriteBatch.Draw(this.texture, this.position, Color.White);
        }
    }
}
