using System;
using System.Collections.Generic;
using System.Text;

using System.Xml;
using System.IO;

namespace XnaGameServer
{
    class LevelLoader
    {

        private Dictionary<char, Enemy> mEnemies = new Dictionary<char, Enemy>();
        public List<Enemy> enemiesLevel = new List<Enemy>();

        private XmlReader reader;

        protected string levelFile;
        protected int level;

        public Vector2 pos;

        public LevelLoader()
            : base()
        {
            levelFile = "Levels/Level.xml";
            pos = new Vector2();
            LoadAssets();
        }

        public void LoadLevel()
        {
            LoadCurrentLevel();
        }

        public int CurrentLevel
        {
            get { return this.level; }
            set { this.level = value; }
        }

        private void LoadAssets()
        {
            reader = XmlReader.Create(levelFile);
            while (reader.Read())
            {
                if (reader.NodeType == XmlNodeType.Element &&
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
                    mEnemies.Add(enemy.Id, enemy);
                    break;
                }

                if (reader.NodeType == XmlNodeType.Element)
                {
                    currentElement = reader.Name;

                    switch (currentElement)
                    {
                        case "id":
                            {
                                enemy.Id = reader.ReadElementContentAsString().ToCharArray()[0];
                                break;
                            }
                        case "name":
                            {
                                // if adding a new type of enemy add to enum and match it..
                                //if (reader.ReadElementContentAsString().Equals(KindEnemy.Normal.ToString()))
                                //{
                                //    enemy = new EnemyNormal();
                                //}
                                //enemy = new Enemy();
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
                     LoadLevel(reader);
                }
            }
        }

        public void LoadLevel(XmlReader reader)
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
                        LoadThisLevel(reader);
                    }
                }           
            }
        }

        public void LoadThisLevel(XmlReader reader)
        {
            Vector2 aPosition = new Vector2();
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
                                pos.X = reader.ReadElementContentAsInt();
                                break;
                            }

                        case "startY":
                            {
                                pos.Y = reader.ReadElementContentAsInt();
                                break;
                            }
                    }
                }
                else if (reader.NodeType == XmlNodeType.EndElement)
                {
                    if (aCurrentElement == "row")
                    {
                        
                        row += 1;

                        aPosition.X = 0;
                    }
                }
                else if (reader.NodeType == XmlNodeType.Text)
                {
                    if (aCurrentElement == "row")
                    {
                        string aRow = reader.Value;

                        for (int aCounter = 0; aCounter < aRow.Length; ++aCounter)
                        {
                            /*
                            if (mTextures.ContainsKey(aRow[aCounter]) == true)
                            {
                                if (mTextures[aRow[aCounter]].id == 'R')
                                {
                                    aPositionY = 480 - mTextures[aRow[aCounter]].texture.Height;
                                }
                                
                                tilesBackground.Add(new Background(mTextures[aRow[aCounter]].texture, new Vector2(aPositionX, aPositionY), mTextures[aRow[aCounter]].walkable));
                                aPositionX += mTextures[aRow[aCounter]].texture.Width;

                            }
                             * */
                            if (mEnemies.ContainsKey(aRow[aCounter]) == true)
                            {
                                // add to the level..base on its id..
                                if (mEnemies[aRow[aCounter]].Id == 'N')
                                {
                                    //if (aPositionX > gameWidth)
                                    //    aPositionX = gameWidth - mEnemies[aRow[aCounter]].SourceRect.Width - 20;

                                    Enemy enemy = new Enemy();
                                    enemy.Position.X = aPosition.X;
                                    enemy.Position.Y = rand.Next(360 - 120, 480 - 130);                                                
                                    enemiesLevel.Add(enemy);
                                }
                                aPosition.X += 50; // mEnemies[aRow[aCounter]].SourceRect.Width;
                            }
                            else
                            {
                                if (aRow[aCounter] == 'E')
                                {
                                    //gameWidth = aPosition.X;
                                }
                                if (aRow[aCounter] == ' ')
                                {
                                    aPosition.X += rand.Next(300, 600);
                                }
                            }
                        }
                    }
                }
            }
        }
    }
}
