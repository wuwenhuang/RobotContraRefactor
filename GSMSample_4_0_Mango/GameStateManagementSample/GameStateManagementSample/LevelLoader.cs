using System;
using System.Collections.Generic;
using System.Text;

using System.Xml;
using GameStateManagementSample;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.GamerServices;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System.IO;

namespace GameStateManagement.SideScrollGame
{
    abstract class LevelLoader : Background
    {
        //The types of textures (symbols) being defined by and being used in the level
        private Dictionary<char, Background> mTextures = new Dictionary<char, Background>();
        public List<Background> tilesBackground = new List<Background>();

        private Dictionary<char, Enemy> mEnemies = new Dictionary<char, Enemy>();
        public List<Enemy> enemiesLevel = new List<Enemy>();

        private XmlReader reader;

        protected string levelFile;

        private ContentManager content;

        protected int level;

        //The starting position for the first tile. 
        int mStartX = 0;
        int mStartY = 0;

        public Vector2 pos;

        //The default height and width for the tiles that make up the level
        protected int gameWidth = 0;

        public LevelLoader()
            : base()
        {
            levelFile = "Content/Levels/Level.xml";
            pos = new Vector2(mStartX, mStartY);
            this.content = GameplayScreen.main.content;
            LoadAssets();
        }

        public void LoadLevel()
        {
            LoadCurrentLevel();
        }

        public int width
        {
            get { return this.gameWidth; }
        }

        public int currentLevel
        {
            get { return this.level; }
        }

        private void LoadAssets()
        {
            reader = XmlReader.Create(levelFile);
            while (reader.Read())
            {
                if (reader.NodeType == XmlNodeType.Element &&
                    reader.Name.Equals("texture", StringComparison.OrdinalIgnoreCase))
                {
                    LoadTile(reader);                
                }
                else if (reader.NodeType == XmlNodeType.Element &&
                    reader.Name.Equals("enemy", StringComparison.OrdinalIgnoreCase))
                {
                    LoadEnemy(reader);
                }
            }
        }

        public void LoadEnemy(XmlReader reader)
        {
            string currentElement = string.Empty;
            Enemy enemy = new Enemy();

            while (reader.Read())
            {
                //Exit the While loop when the end node is encountered and add the Tile
                if (reader.NodeType == XmlNodeType.EndElement &&
                    reader.Name.Equals("kind", StringComparison.OrdinalIgnoreCase))
                {
                    mEnemies.Add(enemy.id, enemy);
                    break;
                }

                if (reader.NodeType == XmlNodeType.Element)
                {
                    currentElement = reader.Name;

                    switch (currentElement)
                    {
                        case "id":
                            {
                                enemy.id = reader.ReadElementContentAsString().ToCharArray()[0];
                                break;
                            }
                        case "name":
                            {
                                // if adding a new type of enemy add to enum and match it..
                                if (reader.ReadElementContentAsString().Equals(KindEnemy.Normal.ToString()))
                                {
                                    enemy = new EnemyNormal();
                                }
                                break;
                            }
                    }
                }
            }
        }

        public void LoadCurrentLevel()
        {
            reader = XmlReader.Create(levelFile);
            while (reader.Read())
            {
                if (reader.NodeType == XmlNodeType.Element &&
                    reader.Name.Equals("level", StringComparison.OrdinalIgnoreCase))
                {
                     LoadLevel(reader, content);
                }
            }
        }
        
        //Load information about the tile defined in the Level XML file
        private void LoadTile(XmlReader reader)
        {
            string currentElement = string.Empty;
            Background texture = new Background();

            while (reader.Read())
            {
                //Exit the While loop when the end node is encountered and add the Tile
                if (reader.NodeType == XmlNodeType.EndElement &&
                    reader.Name.Equals("texture", StringComparison.OrdinalIgnoreCase))
                {
                    mTextures.Add(texture.id, texture);
                    break;
                }

                if (reader.NodeType == XmlNodeType.Element)
                {
                    currentElement = reader.Name;

                    switch (currentElement)
                    {
                        case "id":
                            {
                                texture.id = reader.ReadElementContentAsString().ToCharArray()[0];
                                break;
                            }
                        case "picture":
                            {
                                LoadTexture(reader, content, texture);
                                break;
                            }
                        case "properties":
                            {
                                LoadProperties(reader, texture);
                                break;
                            }
                    }
                }
            }
        }

        private void LoadTexture(XmlReader reader, ContentManager theContent, Background theTile)
        {
            string currentElement = string.Empty;

            while (reader.Read())
            {
                //Exit the While loop when the end node is encountered and add the Tile
                if (reader.NodeType == XmlNodeType.EndElement &&
                    reader.Name.Equals("picture", StringComparison.OrdinalIgnoreCase))
                {
                    break;
                }

                if (reader.NodeType == XmlNodeType.Element)
                {
                    currentElement = reader.Name;
                    
                    switch (currentElement)
                    {
                        case "name":
                            {
                                string aAssetName = reader.ReadElementContentAsString();
                                theTile.texture = theContent.Load<Texture2D>("Background/"+aAssetName);
                                theTile.position = new Vector2(theTile.texture.Width, theTile.texture.Height);
                                break;
                            }
                    }
                }
            }

        }

        private void LoadProperties(XmlReader theReader, Background theTile)
        {
            string aCurrentElement = string.Empty;

            while (theReader.Read())
            {
                if (theReader.NodeType == XmlNodeType.EndElement &&
                    theReader.Name.Equals("properties", StringComparison.OrdinalIgnoreCase))
                {
                    break;
                }

                if (theReader.NodeType == XmlNodeType.Element)
                {
                    aCurrentElement = theReader.Name;
                    switch (aCurrentElement)
                    {
                        case "walkable":
                            {
                                theTile.walkable = theReader.ReadElementContentAsBoolean();
                                break;
                            }
                    }
                }
            }
        }

        public void LoadLevel(XmlReader reader, ContentManager theContent)
        {
            while (reader.Read())
            {
                if (reader.NodeType == XmlNodeType.EndElement &&
                    reader.Name.Equals("level", StringComparison.OrdinalIgnoreCase))
                {
                    break;
                }

                if (reader.NodeType == XmlNodeType.Element &&
                    reader.Name.Equals("name", StringComparison.OrdinalIgnoreCase))
                {
                    int tempLevel = reader.ReadElementContentAsInt();

                    if (level.Equals(tempLevel))
                    {
                        LoadThisLevel(reader, theContent);
                    }
                }           
            }
        }

        public void LoadThisLevel(XmlReader reader, ContentManager theContent)
        {
            int aPositionY = 0;
            int aPositionX = 0;
            Random rand = new Random();
            int row = 0;

            string aCurrentElement = string.Empty;

            while (reader.Read())
            {
                if (reader.NodeType == XmlNodeType.EndElement &&
                    reader.Name.Equals("layout", StringComparison.OrdinalIgnoreCase))
                {
                    break;
                }

                if (reader.NodeType == XmlNodeType.Element)
                {
                    aCurrentElement = reader.Name;
                    switch (aCurrentElement)
                    {
                        case "startX":
                            {
                                mStartX = reader.ReadElementContentAsInt();
                                break;
                            }

                        case "startY":
                            {
                                mStartY = reader.ReadElementContentAsInt();
                                break;
                            }
                    }
                    pos = new Vector2(mStartX, mStartY);
                }
                else if (reader.NodeType == XmlNodeType.EndElement)
                {
                    if (aCurrentElement == "row")
                    {
                        
                        row += 1;

                        aPositionX = 0;
                    }
                }
                else if (reader.NodeType == XmlNodeType.Text)
                {
                    if (aCurrentElement == "row")
                    {
                        string aRow = reader.Value;

                        for (int aCounter = 0; aCounter < aRow.Length; ++aCounter)
                        {
                            if (mTextures.ContainsKey(aRow[aCounter]) == true)
                            {
                                if (mTextures[aRow[aCounter]].id == 'R')
                                {
                                    aPositionY = 480 - mTextures[aRow[aCounter]].texture.Height;
                                }
                                
                                tilesBackground.Add(new Background(mTextures[aRow[aCounter]].texture, new Vector2(aPositionX, aPositionY), mTextures[aRow[aCounter]].walkable));
                                aPositionX += mTextures[aRow[aCounter]].texture.Width;

                            }
                            else if (mEnemies.ContainsKey(aRow[aCounter]) == true)
                            {
                                // add to the level..base on its id..
                                if (mEnemies[aRow[aCounter]].id == 'N')
                                {
                                    if (aPositionX > gameWidth)
                                        aPositionX = gameWidth - mEnemies[aRow[aCounter]].SourceRect.Width - 20;

                                    enemiesLevel.Add(
                                        new EnemyNormal(
                                            new Vector2(
                                                aPositionX, 
                                                rand.Next(360 - 120, GameplayScreen.main.ScreenManager.GraphicsDevice.Viewport.Height - 130))));
                                }
                                aPositionX += mEnemies[aRow[aCounter]].SourceRect.Width;
                            }
                            else
                            {
                                if (aRow[aCounter] == 'E')
                                {
                                    gameWidth = aPositionX;
                                }
                                if (aRow[aCounter] == ' ')
                                {
                                    
                                    aPositionX += rand.Next(300, 600);
                                }
                            }
                        }
                    }
                }
            }
        }

        //Draw the currently loaded level
        public virtual void Draw(SpriteBatch theBatch)
        {
            foreach (Background background in tilesBackground)
            {
                theBatch.Draw(background.texture, Camera2D.main.WorldToScreenPoint(background.position), Color.White);
            }
        }
    }
}
