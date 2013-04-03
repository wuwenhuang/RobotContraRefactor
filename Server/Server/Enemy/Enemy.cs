using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
using GameStateManagementSample;

namespace GameStateManagement.SideScrollGame
{
    enum KindEnemy
    {
        Normal
    };

    #region BASE CLASS ENEMY
    class Enemy : Character
    {
        public Character targetPlayer;
        public char id;
        public Health healthBar;
        private bool isAlive; // should enemy show and move
        private float distancePlayerEnemyAttack = 40.0f;
        

        public Enemy(): base(){}

        public Enemy(Texture2D texture, Vector2 Position, Vector2 Size, float speed)
            : base(texture, Position, Size)
        {
            this.speed = speed;
            this.currentState = CharacterState.MOVELEFT;

            healthBar = new Health(this);
            this.AddChild(healthBar);
            this.isAlive = false;
            this.setDead(false);
        }

        public void SetTargetPlayer(Character targetPerson)
        {
            this.targetPlayer = targetPerson;
        }

        

        public void setAlive(bool alive)
        {
            this.isAlive = true;
        }

        public bool Alive
        {
            get { return this.isAlive; }
        }

        public void FindPlayerYPosition(GameTime gameTime)
        {
            float distanceY = targetPlayer.position.Y - this.position.Y;
            if (distanceY < 0)
                this.position.Y -= speed * (float)gameTime.ElapsedGameTime.TotalSeconds;
            else if (distanceY > 2)
                this.position.Y += speed * (float)gameTime.ElapsedGameTime.TotalSeconds;
            else
            {
                if (currentState == CharacterState.IDLE)
                    currentState = CharacterState.ATTACK;
            }

            if (targetPlayer.position.Y + targetPlayer.SourceRect.Height> GameplayScreen.main.ScreenManager.GraphicsDevice.Viewport.Height)
            {
                targetPlayer.position.Y = GameplayScreen.main.ScreenManager.GraphicsDevice.Viewport.Height - targetPlayer.SourceRect.Height;
            }
          
        }

        public void AttackPlayer(GameTime gameTime)
        {
            if (targetPlayer.currentState != CharacterState.JUMP)
                targetPlayer.getHit(this.attackDamage);
            else
                currentState = CharacterState.IDLE;
        }

        public void GetEnemyMovingToPlayer(float distanceX)
        {
            if (distanceX + targetPlayer.SourceRect.Width < 0)
            {
                currentState = CharacterState.MOVELEFT;
            }
            if (distanceX > targetPlayer.SourceRect.Width)
            {
                currentState = CharacterState.MOVERIGHT;
            }
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            base.Draw(spriteBatch);
        }

        public virtual void  Update(GameTime gameTime, Level level)
        {
            if (this.currentState != CharacterState.DEAD && targetPlayer != null)
            {
                FindPlayerYPosition(gameTime);

                float distanceX = targetPlayer.position.X - this.position.X;

                switch (currentState)
                {
                    case CharacterState.ATTACK:
                        AttackPlayer(gameTime);
                        GetEnemyMovingToPlayer(distanceX);
                        break;

                    case CharacterState.IDLE:
                        GetEnemyMovingToPlayer(distanceX);
                        break;

                    case CharacterState.MOVELEFT:
                        this.lastState = currentState;

                        this.position.X -= speed * (float)gameTime.ElapsedGameTime.TotalSeconds;

                        if (distanceX + targetPlayer.SourceRect.Width - distancePlayerEnemyAttack >= 0 && distanceX < targetPlayer.SourceRect.Width)
                        {
                            currentState = CharacterState.IDLE;
                        }
                        break;

                    case CharacterState.MOVERIGHT:
                        this.lastState = currentState;
                        this.position.X += speed * (float)gameTime.ElapsedGameTime.TotalSeconds;

                        if (distanceX >= 0 && distanceX < targetPlayer.SourceRect.Width)
                        {
                            currentState = CharacterState.IDLE;
                        }
                        break;
                }
            }

            
            base.Update(gameTime, level);
        }

        public void GetEnemyUpdate(GameTime gameTime, Level level)
        {
            base.Update(gameTime, level);
        }

    }
    #endregion

    class EnemyNormal : Enemy
    {
        public EnemyNormal()
        {
            this.id = 'N';
            this.texture = GameplayScreen.main.content.Load<Texture2D>("Character/Enemy/Normal");
            this.sourceRect = new Rectangle(0,0,70, 130);
            this.setDead(false);
        }

       public EnemyNormal(Vector2 pos)
           :base(GameplayScreen.main.content.Load<Texture2D>("Character/Enemy/Normal"),pos, new Vector2(70, 130), 60.0f)
        {
            this.id = 'N';
            this.setAttackDamage(3.0f);
            this.setMaximumHealth(150); // default maximum health is 500
            this.setDead(false);
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            base.Draw(spriteBatch);
        }

        public override void Update(GameTime gameTime, Level level)
        {
            base.Update(gameTime, level);
        }
    }
}
