using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace XnaGameServer
{
    enum KindEnemy
    {
        Normal
    };

    public class Enemy
    {
        public MultiplayerPlayer TargetPlayer { get; set; }
        public char Id { get; set; }
        public bool IsAlive { get; set; } // should enemy show and move
        public float distancePlayerEnemyAttack = 40.0f;
        public float Speed { get; set; }
        public CharacterState CurrentState { get; set; }
        public CharacterState LastState { get; set; }
        public bool IsDead { get; set; }
        public Vector2 Position { get; set; }
        public int Health { get; set; }

        public Enemy() : this(new Vector2(0, 350), 3)
        {
        }

        public Enemy(Vector2 position, float speed)
        {
            this.Speed = speed;
            this.CurrentState = CharacterState.MOVELEFT;
            this.Position = position;
            this.Health = 300;

            this.IsAlive = false;
            this.IsDead = false;
        }

        public void SetTargetPlayer(MultiplayerPlayer targetPerson)
        {
            this.TargetPlayer = targetPerson;
        }

        public bool Alive
        {
            set { this.IsAlive = value; }
            get { return this.IsAlive; }
        }

        public void FindPlayerYPosition()
        {
            float distanceY = TargetPlayer.y - this.Position.Y;
            if (distanceY < 0)
                this.Position.Y -= Speed; // * (float)gameTime.ElapsedGameTime.TotalSeconds;
            else if (distanceY > 2)
                this.Position.Y += Speed; // * (float)gameTime.ElapsedGameTime.TotalSeconds;
            else
            {
                if (CurrentState == CharacterState.IDLE)
                    CurrentState = CharacterState.ATTACK;
            }

            /*
            if (targetPlayer.y + targetPlayer.SourceRect.Height> GameplayScreen.main.ScreenManager.GraphicsDevice.Viewport.Height)
            {
                targetPlayer.y = GameplayScreen.main.ScreenManager.GraphicsDevice.Viewport.Height - targetPlayer.SourceRect.Height;
            }
            */
          
        }

        /*
        public void AttackPlayer()
        {
            if (targetPlayer.state != CharacterState.JUMP)
                targetPlayer.(this.attackDamage);
            else
                currentState = CharacterState.IDLE;
        }
        */

        public void GetEnemyMovingToPlayer(float distanceX)
        {
            if (distanceX < 0)
            {
                CurrentState = CharacterState.MOVELEFT;
            }
            if (distanceX > 0)
            {
                CurrentState = CharacterState.MOVERIGHT;
            }
        }

        public virtual void Update()
        {
            if (this.CurrentState != CharacterState.DEAD && TargetPlayer != null)
            {
                FindPlayerYPosition();

                float distanceX = TargetPlayer.x - this.Position.X;

                switch (CurrentState)
                {
                    case CharacterState.ATTACK:
                        //AttackPlayer(gameTime);
                        GetEnemyMovingToPlayer(distanceX);
                        break;

                    case CharacterState.IDLE:
                        GetEnemyMovingToPlayer(distanceX);
                        break;

                    case CharacterState.MOVELEFT:
                        this.LastState = CurrentState;

                        this.Position.X -= Speed; // * (float)gameTime.ElapsedGameTime.TotalSeconds;

                        if (distanceX - distancePlayerEnemyAttack >= 0 && distanceX < 70)
                        {
                            CurrentState = CharacterState.IDLE;
                        }
                        break;

                    case CharacterState.MOVERIGHT:
                        this.LastState = CurrentState;
                        this.Position.X += Speed; // * (float)gameTime.ElapsedGameTime.TotalSeconds;

                        if (distanceX >= 0 && distanceX < 70)
                        {
                            CurrentState = CharacterState.IDLE;
                        }
                        break;
                }
            }
        }
    }
}
