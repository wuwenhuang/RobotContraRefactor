using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;

namespace GameStateManagement.SideScrollGame
{
    abstract class GameObject : AnimateSprite
    {
        private string id;
        public Vector2 position;
        protected float speed;
        private Vector2 parentPosition;
        private Vector2 offsetPosition;

        public GameObject()
            : base()
        {
            this.id = "";
            this.position = Vector2.Zero;
            this.speed = 0;
            this.parentPosition = Vector2.Zero;
            this.offsetPosition = Vector2.Zero;
            this.character = null;
        }

        public GameObject(Character character, string id, Texture2D texture, Vector2 position, Vector2 size)
            : base(texture,0, (int)size.X, (int)size.Y)
        {
            this.character = character;
            this.id = id;
            this.speed = character.speed;
            this.parentPosition = character.position;
            this.offsetPosition = position;
            this.position = offsetPosition + parentPosition;
        }

        public void SetTexture(Texture2D newTexture)
        {
            this.texture = newTexture;
        }

        public Texture2D GetTexture()
        {
            return this.texture;
        }

        public void SetId(string newID)
        {
            this.id = newID;
        }

        public string ID { get { return this.id; } }

        public Vector2 Position { get { return this.position; } }

        public void SetPosition(Vector2 newPosition)
        {
            this.position = newPosition;
        }

        public void SetSpeed(float newSpeed)
        {
            this.speed = newSpeed;
        }

        public Rectangle BoundingBox
        {
            get
            {
                return new Rectangle(
                    (int)position.X,
                    (int)position.Y,
                    sourceRect.Width,
                    sourceRect.Height);
            }
        }

        public Vector2 GetPosition()
        {
            return this.position;
        }

        public float Speed{ get { return this.speed; } }

        public void Destroy()
        {
            this.texture = null;
            this.position = Vector2.Zero;
            this.speed = 0;
            this.sourceRect = new Rectangle();
            this.id = "";
        }

        public virtual void Draw(SpriteBatch spriteBatch)
        {
            spriteBatch.Draw(this.texture, Camera2D.main.WorldToScreenPoint(this.position), this.sourceRect, Color.White);
        }

        public virtual void Update(GameTime gameTime)
        {
            this.parentPosition = character.position;

            switch (character.currentState)
            {
                
                default:
                    this.position = parentPosition + offsetPosition;
                    break;

            }
        }
    }
}
