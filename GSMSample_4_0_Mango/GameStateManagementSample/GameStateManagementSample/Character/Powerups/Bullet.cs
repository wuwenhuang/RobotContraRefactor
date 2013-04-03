using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using GameStateManagementSample;

namespace GameStateManagement.SideScrollGame
{
    enum BulletType
    {
        NORMAL,
        LASER
    };


    class Bullet : GameObject
    {
        public float lifeSpan;
        private float _timer;
        private bool _faceRight;

        public Bullet(): base(){}

        public Bullet(Character character, string id, Texture2D texture, Vector2 size, float speed, float lifeSpan)
            : base()
        {
            this.character = character;

            if (character.SearchChild("weapon") != null)
            {
                this.texture = texture;
                this.spriteWidth = (int)size.X;
                this.spriteHeight = (int)size.Y;
                this.speed = speed;
                this.lifeSpan = lifeSpan;
                this.SetId(id);
                if (this.character.lastState == CharacterState.MOVELEFT)
                    this._faceRight = false;
                else
                    this._faceRight = true;

                if (_faceRight)
                {
                    this.position = (character.SearchChild("weapon") as Weapon).position;
                    this.position.X += (character.SearchChild("weapon")as Weapon).SourceRect.Width;
                    this.sourceRect = new Rectangle(0, 0, spriteWidth, spriteHeight);
                }
                else
                {
                    this.position = (character.SearchChild("weapon") as Weapon).position;
                    this.position.X -= (character.SearchChild("weapon") as Weapon).LEFTOFFSET + spriteWidth;
                    this.sourceRect = new Rectangle(spriteWidth, 0, spriteWidth, spriteHeight);
                }
            }
            

        }

        public void SetLifeSpan(float newLifeSpan)
        {
            this.lifeSpan = newLifeSpan;
        }

        public override void  Draw(SpriteBatch spriteBatch)
        {
            base.Draw(spriteBatch);
        }

        public override void  Update(GameTime gameTime)
        {
            _timer += (float)gameTime.ElapsedGameTime.TotalSeconds;

            if (lifeSpan > _timer)
            {
                if (_faceRight)
                    this.position.X += speed * (float)gameTime.ElapsedGameTime.TotalSeconds;
                
                else
                    this.position.X -= speed * (float)gameTime.ElapsedGameTime.TotalSeconds;
            }
            else
            {
                this.Destroy();
            }
        }

        
    }

    class BulletNormal : Bullet
    {
        public BulletNormal(Character character)
            : base(character, "normal", GameplayScreen.main.content.Load<Texture2D>("Weapon/Powerups/bulletNormal"), new Vector2(20, 15), 500.0f, 1.0f)
        {
        }

        public override void Update(GameTime gameTime)
        {
            base.Update(gameTime);
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            base.Draw(spriteBatch);
        }
    }

    class BulletLaser : Bullet
    {
        public BulletLaser(Character character)
            : base(character, "normal", GameplayScreen.main.content.Load<Texture2D>("Weapon/Powerups/bulletLaser"), new Vector2(30, 10), 200.0f, 1.0f)
        {
        }

        public override void Update(GameTime gameTime)
        {
            base.Update(gameTime);
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            base.Draw(spriteBatch);
        }
    }
}
