#region File Description
//-----------------------------------------------------------------------------
// MultiplayerMenuScreen.cs
//
// Microsoft XNA Community Game Platform
// Copyright (C) Microsoft Corporation. All rights reserved.
//-----------------------------------------------------------------------------
#endregion

#region Using Statements
using Microsoft.Xna.Framework;
#endregion

namespace GameStateManagementSample
{
    /// <summary>
    /// The options screen is brought up over the top of the main menu
    /// screen, and gives the user a chance to configure the game
    /// in various hopefully useful ways.
    /// </summary>
    class MultiplayerMenuScreen : MenuScreen
    {
        #region Fields

        MenuEntry createSessionMenuEntry;
        MenuEntry joinSessionMenuEntry;
        MenuEntry backMenuEntry;

        #endregion

        #region Initialization


        /// <summary>
        /// Constructor.
        /// </summary>
        public MultiplayerMenuScreen()
            : base("Multiplayer")
        {

            // Create our menu entries.
            joinSessionMenuEntry = new MenuEntry("Join Session");
            backMenuEntry = new MenuEntry("Back");

            // Hook up menu event handlers.
            joinSessionMenuEntry.Selected += JoinSessionMenuSelected;
            backMenuEntry.Selected += BackMenuSelected;

            // Add entries to the menu.
            MenuEntries.Add(joinSessionMenuEntry);
            MenuEntries.Add(backMenuEntry);
        }


        /// <summary>
        /// Fills in the latest values for the options screen menu text.
        /// </summary>

        #endregion

        #region Handle Input

        public override void HandleInput(GameTime gameTime, GameStateManagement.InputState input)
        {
            PlayerIndex playerIndex;

            // Move to the previous menu entry?
            if (menuUp.Evaluate(input, ControllingPlayer, out playerIndex))
            {
                selectedEntry--;

                if (selectedEntry < 0)
                    selectedEntry = menuEntries.Count - 1;
            }

            // Move to the next menu entry?
            if (menuDown.Evaluate(input, ControllingPlayer, out playerIndex))
            {
                selectedEntry++;

                if (selectedEntry >= menuEntries.Count)
                    selectedEntry = 0;
            }

            if (menuSelect.Evaluate(input, ControllingPlayer, out playerIndex))
            {
                OnSelectEntry(selectedEntry, playerIndex);
            }
            else if (menuCancel.Evaluate(input, ControllingPlayer, out playerIndex))
            {

                ScreenManager.AddScreen(new MainMenuScreen(), playerIndex);
            }
        }

        #endregion

        #region Menu Selected Create, Multiplayer, Back Session
        
        void JoinSessionMenuSelected(object sender, PlayerIndexEventArgs e)
        {
            // join session
            ScreenManager.AddScreen(new GameplayScreen(true), e.PlayerIndex);
        }

        void BackMenuSelected(object sender, PlayerIndexEventArgs e)
        {
            ScreenManager.AddScreen(new MainMenuScreen(), e.PlayerIndex);
        }

        #endregion
    }
}
