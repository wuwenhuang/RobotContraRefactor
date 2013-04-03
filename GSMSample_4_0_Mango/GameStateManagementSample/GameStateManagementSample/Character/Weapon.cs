using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;

namespace GameStateManagement.SideScrollGame
{
    class Weapon : GameObject
    {
        protected float rotation = 0;

        public int LEFTOFFSET = 35;

        private float _shootInterval;
        protected float shootInterval;

        public List<Bullet> bullets = new List<Bullet>();
  
        public Weapon(Character character, Texture2D texture, Vector2 position, Vector2 size)
            : base(character, "weapon", texture, position, size)
        {
            shootInterval = 0.20f;

        }

        public void SetShootInterval(float newInterval = 0.5f)
        {
            this.shootInterval = newInterval;
        }

        public override void Update(GameTime gameTime)
        {
            base.Update(gameTime);

            if (bullets != null)
            {
                for (int i = 0; i < bullets.Count; i++)
                {
                    if (bullets[i].Texture == null)
                    {
                        bullets.RemoveAt(i);
                    }
                    else
                    {
                        bullets[i].Update(gameTime);
                    }
                }
            }

            switch (character.currentState)
            {
                case CharacterState.MOVELEFT:
                    frame = 1;
                    sourceRect = new Rectangle(spriteWidth, frame * spriteWidth, spriteWidth, spriteHeight);
                    this.position.X -= LEFTOFFSET;
                    break;

                case CharacterState.MOVERIGHT:
                    frame = 1;
                    sourceRect = new Rectangle(0, frame * spriteWidth, spriteWidth, spriteHeight);              
                    break;

                case CharacterState.MOVEUP:
                case CharacterState.MOVEDOWN:
                    frame = 1;
                    if (this.character.lastState == CharacterState.MOVELEFT)
                    {
                        sourceRect = new Rectangle(spriteWidth, frame * spriteWidth, spriteWidth, spriteHeight);
                        this.position.X -= LEFTOFFSET;
                    }
                    else
                        sourceRect = new Rectangle(0, frame * spriteWidth, spriteWidth, spriteHeight);     
                    break;

                case CharacterState.SHOOT:
                    frame = 1;
                    if (_shootInterval < gameTime.TotalGameTime.TotalSeconds)
                    {
                        ShootBullet();

                        _shootInterval = (float)gameTime.TotalGameTime.TotalSeconds + shootInterval;
                    }

                    if (character.lastState == CharacterState.MOVELEFT)
                    {
                        sourceRect = new Rectangle(spriteWidth, frame * spriteWidth, spriteWidth, spriteHeight);
                        this.position.X -= LEFTOFFSET;
                    }
                    else
                    {
                        sourceRect = new Rectangle(0, frame * spriteWidth, spriteWidth, spriteHeight);
                    }
                    break;

                case CharacterState.IDLE:
                    if (character.lastState == CharacterState.MOVELEFT)
                    {
                        sourceRect = new Rectangle(1 * spriteHeight, 0, spriteHeight, spriteWidth);
                    }
                    else
                    {
                        sourceRect = new Rectangle(0, 0, spriteHeight, spriteWidth);
                    }
                    _shootInterval = 0;
                    break;

                case CharacterState.JUMP:
                case CharacterState.BOOST:
                case CharacterState.DEAD:
                    sourceRect = new Rectangle(0, 0, 0, 0);
                    break;
            }
        }
        
        public override void Draw(SpriteBatch spriteBatch)
        {
            base.Draw(spriteBatch);

            if (bullets != null)
            {
                foreach (Bullet bullet in bullets)
                {
                    if (bullet.Texture !=null)
                        bullet.Draw(spriteBatch);
                }
            }
        }

        #region SETTING BULLET TYPE
        public void ShootBullet()
        {
            if (character.getBulletType().ToString().Equals(BulletType.NORMAL.ToString()))
            {
                bullets.Add(new BulletNormal(character));
            }
            if (character.getBulletType().ToString().Equals(BulletType.LASER.ToString()))
            {
                bullets.Add(new BulletLaser(character));
            }
        }
        #endregion
    }
}
