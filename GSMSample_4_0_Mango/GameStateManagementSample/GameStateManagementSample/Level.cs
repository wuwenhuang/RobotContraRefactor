using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using GameStateManagement;
using GameStateManagementSample;
using Lidgren.Network;

namespace GameStateManagement.SideScrollGame
{

    class Level : LevelLoader
    {
        public static Level main;
        public Color color;
        public Character targetPlayer;

        public Level(Character targetPlayer)
            : base()
        {
            
            main = this;
            gameWidth = 0;
            color = Color.CornflowerBlue;
            if (targetPlayer != null)
                this.targetPlayer = targetPlayer;
        }

        public Level()
            : base()
        {
            main = this;
            gameWidth = 0;
            color = Color.CornflowerBlue;
        }

        public void setTargetPlayer(Character targetPlayer)
        {
            this.targetPlayer = targetPlayer;
        }

        public void Draw(ScreenManager screenManager)
        {
            // This game has a blue background. Why? Because!
            screenManager.GraphicsDevice.Clear(ClearOptions.Target,
                                               this.color, 0, 0);

            SpriteBatch spriteBatch = screenManager.SpriteBatch;

 	        base.Draw(spriteBatch);

            if (enemiesLevel != null)
            {
                foreach (Enemy enemy in enemiesLevel)
                {
                    if (enemy.Alive && enemy.Texture != null && enemy.Dead == false)
                        enemy.Draw(spriteBatch);
                }
            }

        }

        public void ChangeLevel(int level)
        {

            base.level = level;
            
            base.LoadLevel();

            foreach (Enemy enemy in enemiesLevel)
            {
                enemy.SetTargetPlayer(targetPlayer);
                enemy.setDead(false);
            }

            //if (SideScrollGame.main.IsNetwork && SideScrollGame.main.isHost)
            //{
            //    NetOutgoingMessage outMsg = SideScrollGame.main.client.CreateMessage();

            //    outMsg.Write((byte)PacketTypes.WRITELEVEL);

            //    outMsg.Write((short)enemiesLevel.Count);
            //    outMsg.Write((short)SideScrollGame.main.currentLevel);

            //    foreach (Enemy enemy in enemiesLevel)
            //    {
            //        outMsg.Write((short)enemy.health);
            //        outMsg.Write((byte)enemy.currentState);
            //        outMsg.Write((byte)enemy.lastState);

            //        outMsg.Write((float)enemy.position.X);
            //        outMsg.Write((float)enemy.position.Y);

            //    }
            //    SideScrollGame.main.client.SendMessage(outMsg, NetDeliveryMethod.ReliableOrdered);

            //}
            //else if (SideScrollGame.main.isHost == false)
            //{
            //    NetOutgoingMessage msgOut = SideScrollGame.main.client.CreateMessage();
            //    msgOut.Write((byte)PacketTypes.GETSERVERLEVEL);
            //    SideScrollGame.main.client.SendMessage(msgOut, NetDeliveryMethod.ReliableOrdered);
            //}
        }

        public void ChangeLevel(int level, Color color)
        {
            this.color = color;
            base.level = level;
            base.LoadLevel();

            foreach (Enemy enemy in enemiesLevel)
            {
                enemy.SetTargetPlayer(targetPlayer);
                enemy.setDead(false);
            }

            //if (SideScrollGame.main.IsNetwork && SideScrollGame.main.isHost)
            //{
            //    NetOutgoingMessage outMsg = SideScrollGame.main.client.CreateMessage();

            //    outMsg.Write((byte)PacketTypes.WRITELEVEL);

            //    outMsg.Write((short)enemiesLevel.Count);
            //    outMsg.Write((short)SideScrollGame.main.currentLevel);

            //    foreach (Enemy enemy in enemiesLevel)
            //    {
            //        outMsg.Write((short)enemy.health);
            //        outMsg.Write((byte)enemy.currentState);
            //        outMsg.Write((byte)enemy.lastState);

            //        outMsg.Write((float)enemy.position.X);
            //        outMsg.Write((float)enemy.position.Y);

            //    }
            //    SideScrollGame.main.client.SendMessage(outMsg, NetDeliveryMethod.Unreliable);

            //}
            
        }
        
        public void ChangeColor(Color color)
        {
            this.color = color;
        }

        public void Reset()
        {
            foreach (Background background in tilesBackground)
            {
                background.texture = null;
                background.position = Vector2.Zero;
            }

            foreach (Enemy enemy in enemiesLevel)
            {
                enemy.Texture = null;
                enemy.position = Vector2.Zero;
            }
            enemiesLevel.Clear();
            tilesBackground.Clear();
            gameWidth = 0;
            color = Color.White;
        }

        public void Update(GameTime gameTime, Player player)
        {
            if (player.position.X + player.SourceRect.Width > GameplayScreen.main.ScreenManager.GraphicsDevice.Viewport.Width / 2 && player.position.X + player.SourceRect.Width < this.width - GameplayScreen.main.ScreenManager.GraphicsDevice.Viewport.Width / 2)
            {
                Camera2D.main.setPosition(new Vector2(player.position.X + player.SourceRect.Width - Camera2D.main.rect.Width / 2, 0));
            }

            if (enemiesLevel != null)
            {
                for (int i = 0; i < enemiesLevel.Count; i++)
                {
                    if (enemiesLevel[i].Alive == false && enemiesLevel[i].Dead == false && enemiesLevel[i].position.X < Camera2D.main.getPosition().X + Camera2D.main.rect.Width)
                        enemiesLevel[i].setAlive(true);

                    if (enemiesLevel[i].Alive == true && enemiesLevel[i].Dead == false)
                    {
                        enemiesLevel[i].Update(gameTime, this);

                    }

                    //if (SideScrollGame.main.IsNetwork)
                    //{
                    //    if (SideScrollGame.main.isHost)
                    //    {
                    //        if (enemiesLevel.Count > 0)
                    //        {
                    //            NetOutgoingMessage outMsg = SideScrollGame.main.client.CreateMessage();
                    //            outMsg.Write((byte)PacketTypes.UPDATEENEMYPOSITION);

                    //            foreach (Enemy enemy in enemiesLevel)
                    //            {
                    //                outMsg.Write((int)enemy.health);
                    //                outMsg.Write((bool)enemy.Dead);
                    //                outMsg.Write((byte)enemy.currentState);
                    //                outMsg.Write((byte)enemy.lastState);
                    //                outMsg.Write((float)enemy.position.X);
                    //                outMsg.Write((float)enemy.position.Y);
                    //            }

                    //            SideScrollGame.main.client.SendMessage(outMsg, NetDeliveryMethod.ReliableOrdered);
                    //        }
                    //    }
                    //    else
                    //    {
                    //        NetOutgoingMessage outMsg = SideScrollGame.main.client.CreateMessage();

                    //        outMsg.Write((byte)PacketTypes.GETSERVERENEMYPOSITIONS);

                    //        SideScrollGame.main.client.SendMessage(outMsg, NetDeliveryMethod.ReliableOrdered);
                    //    }
                    //}

                    //if (enemiesLevel[i].Texture == null && enemiesLevel[i].Dead == true)
                    //{
                    //    if (SideScrollGame.main.isHost && SideScrollGame.main.IsNetwork)
                    //    {
                    //        NetOutgoingMessage msgOut = SideScrollGame.main.client.CreateMessage();
                    //        msgOut.Write((byte)PacketTypes.DELETEENEMY); // set enemy dead true
                    //        msgOut.Write((short)i);
                    //        SideScrollGame.main.client.SendMessage(msgOut, NetDeliveryMethod.ReliableOrdered);
                    //    }
                    //}


                }
            }
            
        }
        
    }
}
