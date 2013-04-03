#region File Description
//-----------------------------------------------------------------------------
// PauseMenuScreen.cs
//
// Microsoft XNA Community Game Platform
// Copyright (C) Microsoft Corporation. All rights reserved.
//-----------------------------------------------------------------------------
#endregion

#region Using Statements
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content;
#endregion

namespace GameStateManagementSample
{
    /// <summary>
    /// The pause menu comes up over the top of the game,
    /// giving the player options to resume or quit.
    /// </summary>
    class WinLoseScreen : MenuScreen
    {
        #region Initialization

        Texture2D texture;
        bool playersDead, enemiesDead;

        GameplayScreen removeScreenGameplay;

        /// <summary>
        /// Constructor.
        /// </summary>
        public WinLoseScreen(GameplayScreen screen, bool allPlayersDead, bool allEnemiesDead)
            : base("")
        {
            removeScreenGameplay = screen;
            playersDead = allPlayersDead;
            enemiesDead = allEnemiesDead;
            
            MenuEntry restartGameMenuEntry = new MenuEntry("Restart Game");
            MenuEntry quitGameMenuEntry = new MenuEntry("Quit Game");

            // Hook up menu event handlers.
            restartGameMenuEntry.Selected += RestartGameMenuEntrySelected;
            quitGameMenuEntry.Selected += QuitGameMenuEntrySelected;

            // Add entries to the menu.
            MenuEntries.Add(restartGameMenuEntry);
            MenuEntries.Add(quitGameMenuEntry);
        }

        public override void Activate(bool instancePreserved)
        {
            base.Activate(instancePreserved);

            // Create our menu entries.
            ContentManager content = ScreenManager.Game.Content;

            if (playersDead && !enemiesDead)
            {
                this.menuTitle = "YOU LOSE!";
                this.texture = content.Load<Texture2D>("WinLoseScreenBackground/lose");

            }
            else
            {
                this.menuTitle = "YOU WIN!";
                this.texture = content.Load<Texture2D>("WinLoseScreenBackground/win");
            }
        }

        public override void Draw(GameTime gameTime)
        {
            SpriteBatch spriteBatch = ScreenManager.SpriteBatch;
            spriteBatch.Begin();

            spriteBatch.Draw(texture, new Vector2(texture.Width / 2, texture.Height / 2), Color.White);

            spriteBatch.End();

            base.Draw(gameTime);
        }


        #endregion

        #region Handle Input


        /// <summary>
        /// Event handler for when the Quit Game menu entry is selected.
        /// </summary>
        /// 

        void RestartGameMenuEntrySelected(object sender, PlayerIndexEventArgs e)
        {
            const string message = "Do you want to play LOCAL or NETWORK";

            RestartLocalNetwork restartGame = new RestartLocalNetwork(removeScreenGameplay, message);

            restartGame.Local += LocalNetworkGame;
            restartGame.Network += LocalNetworkGame;
            
            ScreenManager.AddScreen(restartGame, ControllingPlayer);
        }

        void QuitGameMenuEntrySelected(object sender, PlayerIndexEventArgs e)
        {
            const string message = "Are you sure you want to quit this game?";

            MessageBoxScreen confirmQuitMessageBox = new MessageBoxScreen(message);

            confirmQuitMessageBox.Accepted += LocalNetworkGame;

            ScreenManager.AddScreen(confirmQuitMessageBox, ControllingPlayer);
        }


        /// <summary>
        /// Event handler for when the user selects ok on the "are you sure
        /// you want to quit" message box. This uses the loading screen to
        /// transition from the game back to the main menu screen.
        /// </summary>
        void LocalNetworkGame(object sender, PlayerIndexEventArgs e)
        {
            LoadingScreen.Load(ScreenManager, false, null, new BackgroundScreen(),
                                                           new MainMenuScreen());
        }


        #endregion
    }
}
