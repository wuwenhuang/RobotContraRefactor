using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
using Lidgren.Network;

namespace GameStateManagement.SideScrollGame
{
    enum CharacterState
    {
        IDLE,
        JUMP,
        MOVERIGHT,
        MOVELEFT,
        MOVEUP,
        MOVEDOWN,
        SHOOT,
        BOOST,
        DEAD,

        // SOME ENEMY ATTACK
        ATTACK
    };

    abstract class Character : AnimateSprite
    {
        protected Vector2 networkPosition;

        public Vector2 position;
        public Vector2 lastPosition;
        public Vector2 velocity;
        public float speed = 250.0f;

        public float jumpSpeed = 10.0f;
        public float jumpHeight = 50.0f;
        public float jumpDistance = 25.0f;

        public float deadHeight = 25.0f;
        public float deadSpeed = 10.0f;
        public float deadDistance = 15.0f;

        public CharacterState currentState;
        public CharacterState lastState;
        public int health;
        public int healthMaximum = 500;
        private BulletType typeBullet;

        private bool isDead;

        public float attackDamage = 10.0f;

        protected List<GameObject> childObjects;

        private Vector2 screen = new Vector2(480, 800);
        
        public Character()
        {
            this.isDead = false;
            this.currentState = CharacterState.IDLE;
            this.lastState = currentState;
            this.childObjects = new List<GameObject>();
            health = healthMaximum;
        }

        public Character(Texture2D texture, Vector2 pos, Vector2 size)
            : base(texture,0, (int)size.X,(int)size.Y)
        {
            this.character = this;
            this.isDead = false;
            this.currentState = CharacterState.IDLE;
            this.lastState = CharacterState.MOVERIGHT;

            if (SideScrollGame.main.IsNetwork == false)
                this.position = pos;
            else
            {
                this.networkPosition = pos;
                this.position = networkPosition;
            }

            this.childObjects = new List<GameObject>();

            health = healthMaximum;
        }

        public void setAttackDamage(float damage)
        {
            this.attackDamage = damage;
        }

        public void setDead(bool dead)
        {
            this.isDead = dead;
        }

        public bool Dead
        {
            get { return this.isDead; }
        }


        public void setMaximumHealth(int health)
        {
            this.healthMaximum = health;
            this.health = healthMaximum;
        }

        public void getHit(float amountDamageToSubtractHealth)
        {
            this.health -= (int)amountDamageToSubtractHealth;
        }

        public void AddChild(GameObject newGameObject)
        {
            childObjects.Add(newGameObject);
        }

        public void RemoveChild(GameObject deleteObject)
        {
            foreach (GameObject child in childObjects)
            {
                if (child.Equals(deleteObject))
                {
                    childObjects.Remove(child);
                    break;
                }
            }
        }

        public void RemoveChild(string id)
        {
            foreach (GameObject child in childObjects)
            {
                if (child.ID.Equals(id))
                {
                    childObjects.Remove(child);
                    break;
                }
            }
        }

        public GameObject SearchChild(GameObject searchObject)
        {
            foreach (GameObject child in childObjects)
            {
                if (child.Equals(searchObject))
                {
                    return child;
                }
            }
            return null;
        }

        public GameObject SearchChild(string id)
        {
            foreach (GameObject child in childObjects)
            {
                if (child.ID.Equals(id))
                {
                    return child;
                }
            }
            return null;
        }

        public GameObject[] GetAllChildren()
        {
            return this.childObjects.ToArray();
        }

        public void setBulletType(BulletType bullettype, float attackDamage)
        {
            typeBullet = bullettype;
            this.attackDamage = attackDamage;
        }

        public BulletType getBulletType()
        {
            return typeBullet;
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

        public void Destroy()
        {
            this.texture = null;
            this.position = Vector2.Zero;
            this.speed = 0;
            this.setDead(true);
            this.sourceRect = new Rectangle();
        }

        public virtual void Draw(SpriteBatch spriteBatch)
        {
            if (this.texture != null)
            {
                spriteBatch.Draw(this.texture, Camera2D.main.WorldToScreenPoint(this.position), this.sourceRect, Color.White);

                if (childObjects != null)
                {
                    foreach (GameObject child in childObjects)
                    {
                        child.Draw(spriteBatch);
                    }
                }
            }
        }

        public virtual void Update(GameTime gameTime, Level level)
        {
            if (this.health <= 0 && this.currentState != CharacterState.DEAD)
            {
                lastPosition = position;
                this.frame = 0;
                this.velocity = new Vector2(deadDistance, deadHeight);
                this.currentState = CharacterState.DEAD;

                if (SideScrollGame.main.IsNetwork)
                {
                    NetOutgoingMessage outMsg = SideScrollGame.main.client.CreateMessage();

                    outMsg.Write((byte)PacketTypes.SENDPLAYERDEAD);

                    SideScrollGame.main.client.SendMessage(outMsg, NetDeliveryMethod.ReliableOrdered);
                }
            }

            if (this.currentState == CharacterState.DEAD)
            {
                this.position.Y -= this.velocity.Y * deadSpeed * (float)gameTime.ElapsedGameTime.TotalSeconds;
                this.velocity += SideScrollGame.GRAVITY * deadSpeed * (float)gameTime.ElapsedGameTime.TotalSeconds;

                if (this.lastState == CharacterState.MOVELEFT)
                {
                    if (this.position.X + this.sourceRect.Width < level.width)
                        this.position.X += this.velocity.X * deadSpeed * (float)gameTime.ElapsedGameTime.TotalSeconds;
                }
                else
                {
                    if (this.position.X > 0)
                        this.position.X -= this.velocity.X * deadSpeed * (float)gameTime.ElapsedGameTime.TotalSeconds;
                }

                if (this.lastPosition.Y < this.position.Y)
                {
                    this.position.Y = this.lastPosition.Y;
                    this.velocity.X = 0;
                    this.Destroy();
                }
            }

            if (childObjects != null)
            {
                foreach (GameObject child in childObjects)
                {
                    child.Update(gameTime);
                }
            }

            

            base.Update(gameTime);
        }
    }
}
