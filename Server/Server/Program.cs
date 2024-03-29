﻿using System;
using System.Threading;
using GameStateManagement;
using Lidgren.Network;
using System.Collections.Generic;

namespace XnaGameServer
{
    
    public enum PacketTypes
    {
        CREATEPLAYER,
        GETNUMBEROFPLAYERS,
        DELETEPLAYER,
        CHANGEHOST,

        WRITELEVEL,
        GETLEVEL,
        GETSERVERLEVEL,
        SETNEWLEVEL,
        SENDNEWLEVELPLAYERS,

        MYPOSITION,
        UPDATEPLAYERS,
        UPDATEVELOCITY,
        SENDUPDATEVELOCITY,
        SENDPLAYERDEAD,

        UPDATEENEMYPOSITION,
        SENDENEMYPOSITIONS,
        GETSERVERENEMYPOSITIONS,
        DELETEENEMY,

        SENDENEMYTARGETPLAYER,
        GETENEMYTARGETPLAYER
    };

    public enum CharacterState
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

    public class MultiplayerPlayer
    {
        public long id;
        public float x,y;
        public float lastPosX, lastPosY;
        public float velocityX, velocityY;
        public CharacterState state;
        public CharacterState lastState;
        public int health;
        public bool isDead;
        public bool isHost;

        public MultiplayerPlayer(long id)
        {
            this.id = id;
            this.health = 500;
            this.isHost = false;
            this.lastState = CharacterState.MOVERIGHT;
            this.state = CharacterState.IDLE;
            this.isDead = false;
        }
    };

    /*
    class Enemy
    {
        public char id;
        public float x,y;
        public long targetPlayer;
        public CharacterState state = CharacterState.IDLE;
        public CharacterState lastState = CharacterState.MOVELEFT;
        public int health = 300;
        public bool isDead = false;
    };
    */

    class Program
    {
        static List<MultiplayerPlayer> multiplayerPlayers = new List<MultiplayerPlayer>();
        static List<Enemy> enemies = new List<Enemy>();

        static Random rand = new Random();

        static double nextSendUpdates;
        static NetServer server;
        static bool updateEnemy = true;
        static Semaphore sem;
        static bool deleteEnemy = false;

        static int level;
        static LevelLoader levelLoader;
        static float newPlayerX = 10;
        static float newPlayerY = 350;

        static DateTime lastTime = DateTime.Now;
        static DateTime currentTime = DateTime.Now;
        static TimeSpan gameTime = TimeSpan.MinValue;

        static void Main(string[] args)
        {
            levelLoader = new LevelLoader();
            LoadLevel();

            sem = new Semaphore(1, 1);
            NetPeerConfiguration config = new NetPeerConfiguration("robotcontra");
            config.EnableMessageType(NetIncomingMessageType.DiscoveryRequest);
            config.Port = 16868;

            Thread _writeClientUpdate = new Thread(new ThreadStart(writeClientsUpdate));
            
            // create and start server
            server = new NetServer(config);
            server.Start();
            
            // schedule initial sending of position updates
            double nextSendUpdates = NetTime.Now;

            _writeClientUpdate.Start();

            // GAME LOOP =====================================================

            // run until escape is pressed
            while (!Console.KeyAvailable || Console.ReadKey().Key != ConsoleKey.Escape)
            {
                // calc game time

                lastTime = currentTime;
                currentTime = DateTime.Now;
                gameTime = lastTime - currentTime;

                foreach (Enemy enemy in enemies)
                {
                    enemy.Update(gameTime);
                }

                NetIncomingMessage msg;
                while ((msg = server.ReadMessage()) != null)
                {
                    switch (msg.MessageType)
                    {
                        case NetIncomingMessageType.DiscoveryRequest:
                            //
                            // Server received a discovery request from a client; send a discovery response (with no extra data attached)
                            //
                            
                            server.SendDiscoveryResponse(null, msg.SenderEndPoint);
                            break;
                        case NetIncomingMessageType.VerboseDebugMessage:
                        case NetIncomingMessageType.DebugMessage:
                        case NetIncomingMessageType.WarningMessage:
                        case NetIncomingMessageType.ErrorMessage:
                            //
                            // Just print diagnostic messages to console
                            //
                            Console.WriteLine(msg.ReadString());
                            break;
                        case NetIncomingMessageType.StatusChanged:
                            NetConnectionStatus status = (NetConnectionStatus)msg.ReadByte();
                            if (status == NetConnectionStatus.Connected)
                            {
                                //
                                // A new player just connected!
                                //
                                Console.WriteLine(NetUtility.ToHexString(msg.SenderConnection.RemoteUniqueIdentifier) + " connected!");
                                
                                multiplayerPlayers.Add(new MultiplayerPlayer(msg.SenderConnection.RemoteUniqueIdentifier));

                                // randomize his position and store in connection tag
                                multiplayerPlayers[multiplayerPlayers.Count - 1].x = newPlayerX; // multiplayerPlayers[multiplayerPlayers.Count - 2].x + 70;
                                multiplayerPlayers[multiplayerPlayers.Count - 1].y = newPlayerY; // multiplayerPlayers[multiplayerPlayers.Count - 2].y;

                                for (int i = 0; i < server.Connections.Count; i++)
                                {
                                    if (server.Connections[i].RemoteUniqueIdentifier == msg.SenderConnection.RemoteUniqueIdentifier)
                                    {
                                        NetConnection player = server.Connections[i] as NetConnection;
                                        NetOutgoingMessage outMessage = server.CreateMessage();
                                        outMessage.Write((byte)PacketTypes.CREATEPLAYER);
                                        outMessage.Write((long)multiplayerPlayers[i].id);
                                        outMessage.Write((float)multiplayerPlayers[i].x);
                                        outMessage.Write((float)multiplayerPlayers[i].y);
                                        server.SendMessage(outMessage, player, NetDeliveryMethod.ReliableOrdered);
                                        break;
                                    }
                                }

                                SetEnemyTarget();
                            }
                            else if (status == NetConnectionStatus.Disconnected || status == NetConnectionStatus.Disconnecting)
                            {
                                Console.WriteLine(NetUtility.ToHexString(msg.SenderConnection.RemoteUniqueIdentifier) + " DISCONNECTED FROM SERVER!");

                                for (int i = 0; i < multiplayerPlayers.Count; i++)
                                {
                                    if (multiplayerPlayers[i].id == msg.SenderConnection.RemoteUniqueIdentifier)
                                    {
                                        if (multiplayerPlayers[i].isHost)
                                        {
                                            if (multiplayerPlayers.Count > 1)
                                            {
                                                multiplayerPlayers[i + 1].isHost = true;

                                                NetConnection player = server.Connections[i];

                                                NetOutgoingMessage outMsg = server.CreateMessage();
                                                outMsg.Write((byte)PacketTypes.CHANGEHOST);
                                                outMsg.Write((bool)multiplayerPlayers[i + 1].isHost);

                                                server.SendMessage(outMsg, player, NetDeliveryMethod.ReliableOrdered);

                                            }
                                        }

                                        multiplayerPlayers.RemoveAt(i);
                                        if (deletePlayerFromServer(msg.SenderConnection.RemoteUniqueIdentifier))
                                        {
                                            Console.WriteLine(NetUtility.ToHexString(msg.SenderConnection.RemoteUniqueIdentifier) + " DELETED!");
                                        }
                                        else
                                        {
                                            Console.WriteLine(NetUtility.ToHexString(msg.SenderConnection.RemoteUniqueIdentifier) + " IS NOT EXIST!");
                                        }

                                        foreach (NetConnection player in server.Connections)
                                        {
                                            NetOutgoingMessage outMessage = server.CreateMessage();
                                            outMessage.Write((byte)PacketTypes.DELETEPLAYER);
                                            outMessage.Write((long)msg.SenderConnection.RemoteUniqueIdentifier);

                                            server.SendMessage(outMessage, player, NetDeliveryMethod.ReliableOrdered);
                                        }

                                        SetEnemyTarget();
                                        SendToAllPlayerEnemyTarget();
                                        
                                        break;
                                    }
                                }
                            }

                            break;
                        case NetIncomingMessageType.Data:
                            
                            switch (msg.ReadByte())
                            {
                                case (byte)PacketTypes.MYPOSITION:
                                    CharacterState state = (CharacterState)msg.ReadByte();
                                    CharacterState laststate = (CharacterState)msg.ReadByte();
                                    int health = msg.ReadInt32();
                                    float xPosition = msg.ReadFloat();
                                    float yPosition = msg.ReadFloat();

                                    foreach (MultiplayerPlayer players in multiplayerPlayers)
                                    {
                                        if (players.id == msg.SenderConnection.RemoteUniqueIdentifier)
                                        {
                                            players.state = state;
                                            players.lastState = laststate;
                                            players.health = health;
                                            players.x = xPosition;
                                            players.y = yPosition;
                                            newPlayerX = xPosition + 70;
                                            newPlayerY = yPosition;
                                            break;
                                        }
                                    }
                                    break;

                                case (byte)PacketTypes.UPDATEVELOCITY:
                                    CharacterState currState = (CharacterState)msg.ReadByte();
                                    CharacterState lastState = (CharacterState)msg.ReadByte();

                                    float lastPosX = msg.ReadFloat();
                                    float lastPosY = msg.ReadFloat();

                                    float updateVelX = msg.ReadFloat();
                                    float updateVelY = msg.ReadFloat();

                                    foreach (MultiplayerPlayer players in multiplayerPlayers)
                                    {
                                        if (players.id == msg.SenderConnection.RemoteUniqueIdentifier)
                                        {
                                            players.state = currState;
                                            players.lastState = lastState;
                                            players.lastPosX = lastPosX;
                                            players.lastPosY = lastPosY;
                                            players.velocityX = updateVelX;
                                            players.velocityY = updateVelY;
                                        }
                                    }

                                    for (int i = 0; i < server.Connections.Count; i++)
                                    {
                                        NetConnection player = server.Connections[i] as NetConnection;
                                        // ... send information about every other player (actually including self)
                                        for (int j = 0; j < server.Connections.Count; j++)
                                        {
                                            // send position update about 'otherPlayer' to 'player'
                                            NetOutgoingMessage om = server.CreateMessage();

                                            // write who this position is for

                                            om.Write((byte)PacketTypes.SENDUPDATEVELOCITY);
                                            om.Write((long)multiplayerPlayers[j].id);
                                            om.Write((byte)multiplayerPlayers[j].state);
                                            om.Write((byte)multiplayerPlayers[j].lastState);
                                            om.Write((float)multiplayerPlayers[j].lastPosX);
                                            om.Write((float)multiplayerPlayers[j].lastPosY);
                                            om.Write((float)multiplayerPlayers[j].velocityX);
                                            om.Write((float)multiplayerPlayers[j].velocityY);

                                            // send message
                                            server.SendMessage(om, player, NetDeliveryMethod.Unreliable);
                                        }

                                    }

                                    break;

                                    case (byte)PacketTypes.SENDPLAYERDEAD:
                                    foreach (MultiplayerPlayer players in multiplayerPlayers)
                                    {
                                        if (players.id == msg.SenderConnection.RemoteUniqueIdentifier)
                                        {
                                            players.isDead = true;
                                        }
                                    }
                                    SetEnemyTarget();
                                    SendToAllPlayerEnemyTarget();
                                    break;

                                case (byte)PacketTypes.GETNUMBEROFPLAYERS:
                                    NetOutgoingMessage msgOut = server.CreateMessage();

                                    msgOut.Write((byte)PacketTypes.GETNUMBEROFPLAYERS);
                                    msgOut.Write((short)multiplayerPlayers.Count);
                                    server.SendMessage(msgOut, msg.SenderConnection, NetDeliveryMethod.ReliableOrdered);

                                    break;

                                case (byte)PacketTypes.WRITELEVEL:
                                    enemies.Clear();

                                    for (int i = 0; i < multiplayerPlayers.Count; i++)
                                    {
                                        if (multiplayerPlayers[i].id == msg.SenderConnection.RemoteUniqueIdentifier)
                                        {
                                            multiplayerPlayers[i].isHost = true;
                                        }
                                    }

                                    int enemiesInLevel = msg.ReadInt16();
                                    level = msg.ReadInt16();

                                    for (int i = 0; i < enemiesInLevel; i++)
                                    {
                                        Enemy tempEnemy = new Enemy();
                                        tempEnemy.IsDead = false;

                                        tempEnemy.Health = msg.ReadInt16();
                                        tempEnemy.CurrentState = (CharacterState)msg.ReadByte();
                                        tempEnemy.LastState = (CharacterState)msg.ReadByte();
                                        tempEnemy.Position.X = msg.ReadFloat();
                                        tempEnemy.Position.Y = msg.ReadFloat();

                                        enemies.Add(tempEnemy);

                                    }
                                    SetEnemyTarget();
                                    
                                    SendToAllPlayerEnemyTarget();

                                    break;

                                case (byte)PacketTypes.SETNEWLEVEL:
                                    level = msg.ReadInt16();

                                    for (int i = 0; i < server.Connections.Count; i++)
                                    {
                                        NetConnection player = server.Connections[i] as NetConnection;
                                        // ... send information about every other player (actually including self)
                                        for (int j = 0; j < multiplayerPlayers.Count; j++)
                                        {
                                            NetOutgoingMessage outMsg = server.CreateMessage();

                                            outMsg.Write((byte)PacketTypes.SENDNEWLEVELPLAYERS);
                                            outMsg.Write((short)level);

                                            server.SendMessage(outMsg, player, NetDeliveryMethod.ReliableOrdered);

                                        }
                                    }
                                    break;

                                case (byte)PacketTypes.UPDATEENEMYPOSITION:
                                    for (int i = 0; i < enemies.Count; i++)
                                    {
                                        enemies[i].Health = msg.ReadInt32();
                                        enemies[i].IsDead = msg.ReadBoolean();
                                        enemies[i].CurrentState = (CharacterState)msg.ReadByte();
                                        enemies[i].LastState = (CharacterState)msg.ReadByte();
                                        enemies[i].Position.X = msg.ReadFloat();
                                        enemies[i].Position.Y = msg.ReadFloat();
                                    }
                                    break;

                                case (byte)PacketTypes.DELETEENEMY:
                                    int enemyDead = msg.ReadInt16();
                                    sem.WaitOne();
                                    enemies[enemyDead].IsDead = true;
                                    sem.Release();
                                    break;

                                case (byte)PacketTypes.GETSERVERENEMYPOSITIONS:
                                    msgOut = server.CreateMessage();

                                    msgOut.Write((byte)PacketTypes.SENDENEMYPOSITIONS);

                                    for (int i = 0; i < enemies.Count; i++)
                                    {
                                       
                                        msgOut.Write((byte)enemies[i].CurrentState);
                                        msgOut.Write((byte)enemies[i].LastState);
                                        msgOut.Write((short)enemies[i].Health);
                                        msgOut.Write((bool)enemies[i].IsDead);
                                        msgOut.Write((float)enemies[i].Position.X);
                                        msgOut.Write((float)enemies[i].Position.Y);
                                    }

                                    server.SendMessage(msgOut, msg.SenderConnection, NetDeliveryMethod.ReliableOrdered);

                                    break;

                                case (byte)PacketTypes.GETSERVERLEVEL:
                                    msgOut = server.CreateMessage();

                                    msgOut.Write((byte)PacketTypes.GETLEVEL);
                                    msgOut.Write((short)level);
                                    msgOut.Write((short)enemies.Count);

                                    for (int i = 0; i < enemies.Count; i++)
                                    {
                                        msgOut.Write((byte)enemies[i].CurrentState);
                                        msgOut.Write((byte)enemies[i].LastState);
                                        msgOut.Write((short)enemies[i].Health);
                                        msgOut.Write((bool)enemies[i].IsDead);
                                        msgOut.Write((float)enemies[i].Position.X);
                                        msgOut.Write((float)enemies[i].Position.Y);

                                    }
                                    server.SendMessage(msgOut, msg.SenderConnection, NetDeliveryMethod.ReliableOrdered);
                                    break;

                                

                                case (byte)PacketTypes.GETENEMYTARGETPLAYER:
                                    SetEnemyTarget();
                                    SendToAllPlayerEnemyTarget();

                                    break;
                            }
                            break;
                    }

                }

                double now = NetTime.Now;
                if (now > nextSendUpdates)
                {
                    // Yes, it's time to send position updates

                    // for each player...
                    for (int i = 0; i < server.Connections.Count; i++)
                    {
                        NetConnection player = server.Connections[i] as NetConnection;
                        // ... send information about every other player (actually including self)
                        for (int j = 0; j < multiplayerPlayers.Count; j++)
                        {
                            // send position update about 'otherPlayer' to 'player'
                            NetOutgoingMessage om = server.CreateMessage();

                            // write who this position is for
                            om.Write((byte)PacketTypes.UPDATEPLAYERS);
                            om.Write((long)multiplayerPlayers[j].id);
                            om.Write((byte)multiplayerPlayers[j].state);
                            om.Write((byte)multiplayerPlayers[j].lastState);
                            om.Write((int)multiplayerPlayers[j].health);
                            om.Write((float)multiplayerPlayers[j].x);
                            om.Write((float)multiplayerPlayers[j].y);

                            // send message
                            server.SendMessage(om, player, NetDeliveryMethod.Unreliable);
                        }

                    }

                    // schedule next update
                    nextSendUpdates += (1.0 / 60.0);
                }
                // sleep to allow other processes to run smoothly
                Thread.Sleep(1);
            }

            server.Shutdown("app exiting");
        }

        

        static void SetEnemyTarget()
        {
            if (enemies.Count > 0 && multiplayerPlayers.Count > 0)
            {
                for (int i = 0; i < enemies.Count; i++)
                {
                    int index = rand.Next(0, multiplayerPlayers.Count);

                    while (multiplayerPlayers[index].isDead == true)
                    {
                        index = rand.Next(0, multiplayerPlayers.Count);
                    }

                    if (multiplayerPlayers[index].isDead == false)
                        enemies[i].TargetPlayer = multiplayerPlayers[index];
                }
            }
        }

        static void SendToAllPlayerEnemyTarget()
        {
            if (enemies.Count > 0 && server != null)
            {
                for (int i = 0; i < server.Connections.Count; i++)
                {
                    NetConnection player = server.Connections[i] as NetConnection;
                    // ... send information about every other player (actually including self)
                    for (int j = 0; j < multiplayerPlayers.Count; j++)
                    {
                        // send position update about 'otherPlayer' to 'player'
                        NetOutgoingMessage om = server.CreateMessage();

                        om.Write((byte)PacketTypes.SENDENEMYTARGETPLAYER);
                        for (int k = 0; k < enemies.Count; k++)
                        {
                            om.Write((long)enemies[k].TargetPlayer.id);
                        }
                        server.SendMessage(om, player, NetDeliveryMethod.ReliableOrdered);
                    }
                }
            }
        }

        static bool deletePlayerFromServer(long id)
        {
            for (int i = 0; i < multiplayerPlayers.Count; i++)
            {
                if (multiplayerPlayers[i].id.Equals(id))
                {
                    multiplayerPlayers.RemoveAt(i);
                    return true;
                }
            }
            return false;
        }

        static void writeClientsUpdate()
        {
            ////
            //// send position updates 60 times per second
            ////
            while (true)
            {
                if (enemies.Count > 0)
                {
                    for (int i = 0; i < server.Connections.Count; i++)
                    {
                        NetConnection player = server.Connections[i] as NetConnection;
                        // ... send information about every other player (actually including self)
                        for (int j = 0; j < multiplayerPlayers.Count; j++)
                        {
                            NetOutgoingMessage msgOut = server.CreateMessage();

                            msgOut.Write((byte)PacketTypes.SENDENEMYPOSITIONS);

                            for (int k = 0; k < enemies.Count; k++)
                            {
                                msgOut.Write((byte)enemies[i].CurrentState);
                                msgOut.Write((byte)enemies[i].LastState);
                                msgOut.Write((short)enemies[i].Health);
                                msgOut.Write((bool)enemies[i].IsDead);
                                msgOut.Write((float)enemies[i].Position.X);
                                msgOut.Write((float)enemies[i].Position.Y);
                            }

                            server.SendMessage(msgOut, player, NetDeliveryMethod.ReliableOrdered);
                        }
                    }
                }
                Thread.Sleep(0);
            }
        }

        static void LoadLevel()
        {
            enemies.Clear();
            levelLoader.CurrentLevel = 1;
            levelLoader.LoadLevel();

            //int enemiesInLevel = msg.ReadInt16();
            //level = msg.ReadInt16();

            for (int i = 0; i < levelLoader.enemiesLevel.Count; i++)
            {
                /*
                Enemy tempEnemy = new Enemy();
                tempEnemy.isDead = false;

                tempEnemy.health = msg.ReadInt16();
                tempEnemy.state = (CharacterState)msg.ReadByte();
                tempEnemy.lastState = (CharacterState)msg.ReadByte();
                tempEnemy.x = msg.ReadFloat();
                tempEnemy.y = msg.ReadFloat();
                */
                enemies.Add(levelLoader.enemiesLevel[i]);

            }
            SetEnemyTarget();

            SendToAllPlayerEnemyTarget();
        }
    }
}
