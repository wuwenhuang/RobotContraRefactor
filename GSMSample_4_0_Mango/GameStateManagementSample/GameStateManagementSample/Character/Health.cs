using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
using GameStateManagementSample;

namespace GameStateManagement.SideScrollGame
{
    class Health : GameObject
    {
        public Health(Character character)
            : base(character, "health", GameplayScreen.main.content.Load<Texture2D>("Character/health"), new Vector2(0, -20), new Vector2(60, 15))
        {
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            if (character.currentState != CharacterState.DEAD)
            {
                //draw the negative space for the health bar
                spriteBatch.Draw(this.texture,
                    new Rectangle((int)Camera2D.main.WorldToScreenPoint(this.position).X, (int)Camera2D.main.WorldToScreenPoint(this.position).Y, texture.Width, this.texture.Height / 2),
                    new Rectangle(0, 0, texture.Width, texture.Height / 2),
                    Color.White);

                // draw the current health level
                spriteBatch.Draw(this.texture,
                    new Rectangle((int)Camera2D.main.WorldToScreenPoint(this.position).X, (int)Camera2D.main.WorldToScreenPoint(this.position).Y, (int)(this.texture.Width * ((double)character.health / character.healthMaximum)), this.texture.Height / 2),
                    new Rectangle(0, texture.Height / 2, texture.Width, texture.Height / 2),
                    Color.Red);
            }
        }
        
        public override void Update(GameTime gameTime)
        {
            
            base.Update(gameTime);
        }
    }
}
