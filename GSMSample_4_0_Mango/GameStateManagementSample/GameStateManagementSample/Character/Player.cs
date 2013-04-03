using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Lidgren.Network;
using GameStateManagementSample;

namespace GameStateManagement.SideScrollGame
{
    enum PlayerColor
    {
        BLUE,
        RED
    }

    class Player : Character
    {
        private Weapon weapon;
        private Health healthBar;
        public long id;

        public Player(Texture2D texture, Vector2 position)
            : base(texture, position, new Vector2(70, 130))
        {
            weapon = new Weapon(this, GameplayScreen.main.content.Load<Texture2D>("Weapon/rifle"), new Vector2(5,35), new Vector2(100,30));
            healthBar = new Health(this);

            this.setDead(false);
            this.AddChild(weapon);
            this.AddChild(healthBar);
            this.setBulletType(BulletType.NORMAL, 15.0f);
        }

        public Player(long id, Texture2D texture, Vector2 position)
            : base(texture, position, new Vector2(70, 130))
        {
            this.id = id;
            weapon = new Weapon(this, GameplayScreen.main.content.Load<Texture2D>("Weapon/rifle"), new Vector2(5, 35), new Vector2(100, 30));
            healthBar = new Health(this);

            this.setDead(false);
            this.AddChild(weapon);
            this.AddChild(healthBar);
            this.setBulletType(BulletType.NORMAL, 15.0f);
        }

        public void Reset()
        {
            if (SideScrollGame.main.IsNetwork == false)
                position = new Vector2(10, 350);
            else
            {
                position = networkPosition;
            }
        }

        public override void Update(GameTime gameTime, Level level)
        {
            UpdatePosition(gameTime, level);

            if (SideScrollGame.main.IsNetwork)
            {
                NetOutgoingMessage outmessage = SideScrollGame.main.client.CreateMessage();
                {
                    outmessage.Write((byte)PacketTypes.MYPOSITION);
                    outmessage.Write((byte)this.currentState);
                    outmessage.Write((byte)this.lastState);
                    outmessage.Write((int)this.health);
                    outmessage.Write((float)this.position.X);
                    outmessage.Write((float)this.position.Y);
                    SideScrollGame.main.client.SendMessage(outmessage, NetDeliveryMethod.Unreliable);
                }
            }

            base.Update(gameTime, level);  
        }

        public void CharacterUpdate(GameTime gameTime, Level level)
        {
            UpdatePosition(gameTime, level);
            base.Update(gameTime, level);
        }

        public void UpdatePosition(GameTime gameTime, Level level)
        {
            if (currentState != CharacterState.DEAD)
            {
                switch (currentState)
                {
                    case CharacterState.JUMP:
                        this.position.Y -= this.velocity.Y * jumpSpeed * (float)gameTime.ElapsedGameTime.TotalSeconds;
                        this.velocity += SideScrollGame.GRAVITY * jumpSpeed * (float)gameTime.ElapsedGameTime.TotalSeconds;

                        if (this.lastState == CharacterState.MOVERIGHT)
                        {
                            if (this.position.X + this.sourceRect.Width < level.width)
                                this.position.X += this.velocity.X * jumpSpeed * (float)gameTime.ElapsedGameTime.TotalSeconds;
                        }
                        else
                        {
                            if (this.position.X > 0)
                                this.position.X -= this.velocity.X * jumpSpeed * (float)gameTime.ElapsedGameTime.TotalSeconds;
                        }

                        if (this.lastPosition.Y < this.position.Y)
                        {
                            this.position.Y = this.lastPosition.Y;
                            currentState = CharacterState.IDLE;
                        }

                        break;

                    case CharacterState.BOOST:

                        if (this.lastState == CharacterState.MOVERIGHT)
                        {
                            if (this.position.X + this.sourceRect.Width < level.width)
                            {
                                this.position.X += (speed * 2.0f) * (float)gameTime.ElapsedGameTime.TotalSeconds;
                            }
                        }
                        else
                        {
                            if (this.position.X > 0)
                            {
                                this.position.X -= (speed * 2.0f) * (float)gameTime.ElapsedGameTime.TotalSeconds;
                            }
                        }
                        break;

                    case CharacterState.MOVELEFT:
                        if (this.position.X > 0)
                        {
                            this.position.X -= speed * (float)gameTime.ElapsedGameTime.TotalSeconds;
                        }
                        break;

                    case CharacterState.MOVERIGHT:
                        if (this.position.X + this.sourceRect.Width < level.width)
                        {
                            this.position.X += speed * (float)gameTime.ElapsedGameTime.TotalSeconds;
                        }
                        break;

                    case CharacterState.MOVEUP:
                        foreach (Background background in level.tilesBackground)
                        {
                            if (background.walkable)
                            {
                                if (this.position.Y > background.position.Y - background.texture.Height)
                                {
                                    this.position.Y -= speed * (float)gameTime.ElapsedGameTime.TotalSeconds;
                                }
                                break;
                            }
                        }
                        break;

                    case CharacterState.MOVEDOWN:
                        if (this.position.Y + this.sourceRect.Height < GameplayScreen.main.ScreenManager.GraphicsDevice.Viewport.Height)
                        {
                            this.position.Y += speed * (float)gameTime.ElapsedGameTime.TotalSeconds;
                        }
                        break;

                }
                //checking if bullets is hit an enemy
                if (weapon != null && weapon.bullets != null)
                {
                    for (int i = 0; i < weapon.bullets.Count; i++)
                    {
                        foreach (Enemy enemy in level.enemiesLevel)
                        {
                            if (weapon.bullets[i].BoundingBox.Intersects(enemy.BoundingBox))
                            {
                                enemy.getHit(this.attackDamage);
                                weapon.bullets.RemoveAt(i);
                                break;
                            }
                        }
                    }
                }
            }

        }

        public void HandleInput(GamePadState gamePad)
        {
            switch (currentState)
            {
                case CharacterState.IDLE:
                    this.lastPosition = position;
                    
                    if (gamePad.IsButtonDown(Buttons.DPadLeft))
                    {
                        this.currentState = CharacterState.MOVELEFT;
                        this.lastState = currentState;
                    }

                    if (gamePad.IsButtonDown(Buttons.DPadRight))
                    {
                        this.currentState = CharacterState.MOVERIGHT;
                        this.lastState = currentState;
                    }

                    if (gamePad.IsButtonDown(Buttons.DPadUp))
                    {
                        this.currentState = CharacterState.MOVEUP;
                    }

                    if (gamePad.IsButtonDown(Buttons.DPadDown))
                    {
                        this.currentState = CharacterState.MOVEDOWN;
                    }
                    
                    if (gamePad.IsButtonDown(Buttons.A))
                    {

                        this.velocity = new Vector2(0, jumpHeight);
                        this.currentState = CharacterState.JUMP;
                    }

                    if (gamePad.IsButtonDown(Buttons.X))
                    {
                        this.currentState = CharacterState.SHOOT;
                    }
                    break;

                case CharacterState.MOVERIGHT:
                    if (gamePad.IsButtonUp(Buttons.DPadRight))
                    {
                        this.lastState = currentState;
                        this.currentState = CharacterState.IDLE;
                    }
                    else
                    {
                        this.lastPosition = position;
                        if (gamePad.IsButtonDown(Buttons.X))
                        {
                            this.currentState = CharacterState.SHOOT;
                        }

                        if (gamePad.IsButtonDown(Buttons.A))
                        {
                            this.velocity = new Vector2(jumpDistance, jumpHeight);
                            this.currentState = CharacterState.JUMP;
                        }

                        if (gamePad.IsButtonDown(Buttons.RightTrigger))
                        {
                            this.currentState = CharacterState.BOOST;
                        }
                    }
                    break;

                case CharacterState.MOVELEFT:
                    if (gamePad.IsButtonUp(Buttons.DPadLeft))
                    {
                        this.lastState = currentState;
                        this.currentState = CharacterState.IDLE;
                    }
                    else
                    {
                        this.lastPosition = position;
                        if (gamePad.IsButtonDown(Buttons.X))
                        {
                            this.currentState = CharacterState.SHOOT;
                        }

                        if (gamePad.IsButtonDown(Buttons.A))
                        {
                            this.velocity = new Vector2(jumpDistance, jumpHeight);
                            this.currentState = CharacterState.JUMP;
                        }

                        if (gamePad.IsButtonDown(Buttons.RightTrigger))
                        {
                            this.currentState = CharacterState.BOOST;
                        }
                    }
                    break;

                case CharacterState.MOVEUP:
                    if (gamePad.IsButtonUp(Buttons.DPadUp))
                    {
                        this.currentState = CharacterState.IDLE;
                    }
                    break;

                case CharacterState.MOVEDOWN:
                    if (gamePad.IsButtonUp(Buttons.DPadDown))
                    {
                        this.currentState = CharacterState.IDLE;
                    }
                    
                    break;

                case CharacterState.SHOOT:
                    if (gamePad.IsButtonUp(Buttons.X))
                    {
                        this.currentState = CharacterState.IDLE;
                    }
                    break;

                case CharacterState.BOOST:
                    if (gamePad.IsButtonUp(Buttons.RightTrigger))
                    {
                        this.currentState = lastState;
                    }
                    else
                    {
                        this.lastPosition = position;
                        if (gamePad.IsButtonDown(Buttons.A))
                        {
                            this.velocity = new Vector2((jumpDistance * 2), jumpHeight);
                            this.currentState = CharacterState.JUMP;
                        }
                    }
                    break;
            }
        }

        public void HandleInput(KeyboardState keyPad, MouseState mousePad)
        {
            if (this.Dead == false)
            {
                switch (currentState)
                {
                    case CharacterState.IDLE:
                        this.lastPosition = position;
                        if (keyPad.IsKeyDown(Keys.A))
                        {
                            this.currentState = CharacterState.MOVELEFT;
                            this.lastState = currentState;
                        }

                        if (keyPad.IsKeyDown(Keys.D))
                        {
                            this.currentState = CharacterState.MOVERIGHT;
                            this.lastState = currentState;
                        }

                        if (keyPad.IsKeyDown(Keys.W))
                        {
                            this.currentState = CharacterState.MOVEUP;
                        }

                        if (keyPad.IsKeyDown(Keys.S))
                        {
                            this.currentState = CharacterState.MOVEDOWN;
                        }

                        if (keyPad.IsKeyDown(Keys.Space))
                        {
                            this.velocity = new Vector2(0, jumpHeight);
                            this.currentState = CharacterState.JUMP;

                            if (SideScrollGame.main.IsNetwork)
                            {
                                if (this.currentState == CharacterState.JUMP)
                                {
                                    NetOutgoingMessage outMsg = SideScrollGame.main.client.CreateMessage();
                                    outMsg.Write((byte)PacketTypes.UPDATEVELOCITY);
                                    outMsg.Write((byte)this.currentState);
                                    outMsg.Write((byte)this.lastState);
                                    outMsg.Write((float)this.lastPosition.X);
                                    outMsg.Write((float)this.lastPosition.Y);
                                    outMsg.Write((float)this.velocity.X);
                                    outMsg.Write((float)this.velocity.Y);

                                    SideScrollGame.main.client.SendMessage(outMsg, NetDeliveryMethod.ReliableOrdered);

                                }
                            }
                        }
                        if (mousePad.LeftButton == ButtonState.Pressed)
                        {
                            this.currentState = CharacterState.SHOOT;
                        }
                        if (keyPad.IsKeyDown(Keys.OemMinus))
                        {
                            this.health -= 10;
                        }
                        if (keyPad.IsKeyDown(Keys.OemPlus))
                        {
                            if (health < healthMaximum)
                                this.health += 10;
                        }

                        if (keyPad.IsKeyDown(Keys.PageUp))
                        {
                            this.setBulletType(BulletType.NORMAL, 15.0f);
                        }

                        if (keyPad.IsKeyDown(Keys.PageDown))
                        {
                            this.setBulletType(BulletType.LASER, 30.0f);
                        }

                        if (keyPad.IsKeyDown(Keys.Delete))
                        {
                            foreach (Enemy enemy in Level.main.enemiesLevel)
                            {
                                if (enemy.Alive)
                                    enemy.getHit(10);
                            }
                        }
                        break;

                    case CharacterState.MOVERIGHT:
                        if (keyPad.IsKeyUp(Keys.D))
                        {
                            this.lastState = currentState;
                            this.currentState = CharacterState.IDLE;
                        }
                        else
                        {
                            this.lastPosition = position;
                            if (mousePad.LeftButton == ButtonState.Pressed)
                            {
                                this.currentState = CharacterState.SHOOT;
                            }

                            if (keyPad.IsKeyDown(Keys.Space))
                            {
                                this.velocity = new Vector2(jumpDistance, jumpHeight);
                                this.currentState = CharacterState.JUMP;

                                if (SideScrollGame.main.IsNetwork)
                                {
                                    if (this.currentState == CharacterState.JUMP)
                                    {
                                        NetOutgoingMessage outMsg = SideScrollGame.main.client.CreateMessage();
                                        outMsg.Write((byte)PacketTypes.UPDATEVELOCITY);
                                        outMsg.Write((byte)this.currentState);
                                        outMsg.Write((byte)this.lastState);
                                        outMsg.Write((float)this.lastPosition.X);
                                        outMsg.Write((float)this.lastPosition.Y);
                                        outMsg.Write((float)this.velocity.X);
                                        outMsg.Write((float)this.velocity.Y);

                                        SideScrollGame.main.client.SendMessage(outMsg, NetDeliveryMethod.ReliableOrdered);

                                    }
                                }
                            }

                            if (keyPad.IsKeyDown(Keys.LeftShift))
                            {
                                this.currentState = CharacterState.BOOST;
                            }
                        }
                        break;

                    case CharacterState.MOVELEFT:
                        if (keyPad.IsKeyUp(Keys.A))
                        {
                            this.lastState = currentState;
                            this.currentState = CharacterState.IDLE;
                        }
                        else
                        {
                            this.lastPosition = position;
                            if (mousePad.LeftButton == ButtonState.Pressed)
                            {
                                this.currentState = CharacterState.SHOOT;
                            }
                            if (keyPad.IsKeyDown(Keys.Space))
                            {
                                this.velocity = new Vector2(jumpDistance, jumpHeight);
                                this.currentState = CharacterState.JUMP;

                                if (SideScrollGame.main.IsNetwork)
                                {
                                    if (this.currentState == CharacterState.JUMP)
                                    {
                                        NetOutgoingMessage outMsg = SideScrollGame.main.client.CreateMessage();
                                        outMsg.Write((byte)PacketTypes.UPDATEVELOCITY);
                                        outMsg.Write((byte)this.currentState);
                                        outMsg.Write((byte)this.lastState);
                                        outMsg.Write((float)this.lastPosition.X);
                                        outMsg.Write((float)this.lastPosition.Y);
                                        outMsg.Write((float)this.velocity.X);
                                        outMsg.Write((float)this.velocity.Y);

                                        SideScrollGame.main.client.SendMessage(outMsg, NetDeliveryMethod.ReliableOrdered);

                                    }
                                }
                            }

                            if (keyPad.IsKeyDown(Keys.LeftShift))
                            {
                                this.currentState = CharacterState.BOOST;
                            }
                        }
                        break;

                    case CharacterState.MOVEUP:
                        if (keyPad.IsKeyUp(Keys.W))
                        {
                            this.currentState = CharacterState.IDLE;
                        }
                        break;

                    case CharacterState.MOVEDOWN:
                        if (keyPad.IsKeyUp(Keys.S))
                        {
                            this.currentState = CharacterState.IDLE;
                        }
                        break;

                    case CharacterState.SHOOT:
                        if (mousePad.LeftButton == ButtonState.Released)
                        {
                            this.currentState = CharacterState.IDLE;
                        }
                        break;

                    case CharacterState.BOOST:
                        if (keyPad.IsKeyUp(Keys.LeftShift))
                        {
                            this.currentState = lastState;
                        }
                        else
                        {
                            this.lastPosition = position;
                            if (keyPad.IsKeyDown(Keys.Space))
                            {
                                this.velocity = new Vector2((jumpDistance * 2), jumpHeight);
                                this.currentState = CharacterState.JUMP;

                                if (SideScrollGame.main.IsNetwork)
                                {
                                    if (this.currentState == CharacterState.JUMP)
                                    {
                                        NetOutgoingMessage outMsg = SideScrollGame.main.client.CreateMessage();
                                        outMsg.Write((byte)PacketTypes.UPDATEVELOCITY);
                                        outMsg.Write((byte)this.currentState);
                                        outMsg.Write((byte)this.lastState);
                                        outMsg.Write((float)this.lastPosition.X);
                                        outMsg.Write((float)this.lastPosition.Y);
                                        outMsg.Write((float)this.velocity.X);
                                        outMsg.Write((float)this.velocity.Y);

                                        SideScrollGame.main.client.SendMessage(outMsg, NetDeliveryMethod.ReliableOrdered);

                                    }
                                }
                            }
                        }
                        break;
                }
            }
            
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            if (this.Dead == false)
                base.Draw(spriteBatch);
        }

        public Texture2D getTexture()
        {
            return this.texture;
        }
    }
}
