#region File Description
//-----------------------------------------------------------------------------
// MessageBoxScreen.cs
//
// Microsoft XNA Community Game Platform
// Copyright (C) Microsoft Corporation. All rights reserved.
//-----------------------------------------------------------------------------
#endregion

#region Using Statements
using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using GameStateManagement;
using Lidgren.Network;
#endregion

namespace GameStateManagementSample
{
    /// <summary>
    /// A popup message box screen, used to display "are you sure?"
    /// confirmation messages.
    /// </summary>
    class RestartLocalNetwork : GameScreen
    {
        #region Fields

        string message;
        Texture2D gradientTexture;

        InputAction menuLocal;
        InputAction menuNetwork;
        InputAction menuCancel;

        GameScreen removeGameplayScreen;

        #endregion

        #region Events

        public event EventHandler<PlayerIndexEventArgs> Local;
        public event EventHandler<PlayerIndexEventArgs> Network;
        public event EventHandler<PlayerIndexEventArgs> Cancel;

        #endregion

        #region Initialization


        /// <summary>
        /// Constructor automatically includes the standard "A=ok, B=cancel"
        /// usage text prompt.
        /// </summary>
        public RestartLocalNetwork(GameScreen screen, string message)
            : this(message, true)
        {
            removeGameplayScreen = screen;
        }


        /// <summary>
        /// Constructor lets the caller specify whether to include the standard
        /// "A=ok, B=cancel" usage text prompt.
        /// </summary>
        public RestartLocalNetwork(string message, bool includeUsageText)
        {
            const string usageText = "\nX button, l, L = LOCAL" +
                                     "\nA button, n, N = NETWORK" +
                                     "\nB button, ESC  = Cancel";

            if (includeUsageText)
                this.message = message + usageText;
            else
                this.message = message;

            IsPopup = true;

            TransitionOnTime = TimeSpan.FromSeconds(0.2);
            TransitionOffTime = TimeSpan.FromSeconds(0.2);

            menuLocal = new InputAction(
                new Buttons[] { Buttons.X},
                new Keys[] { Keys.L},
                true);
            menuNetwork = new InputAction(
                new Buttons[] { Buttons.A},
                new Keys[] { Keys.N},
                true);
            menuCancel = new InputAction(
                new Buttons[] { Buttons.B },
                new Keys[] { Keys.Escape },
                true);
        }


        /// <summary>
        /// Loads graphics content for this screen. This uses the shared ContentManager
        /// provided by the Game class, so the content will remain loaded forever.
        /// Whenever a subsequent MessageBoxScreen tries to load this same content,
        /// it will just get back another reference to the already loaded data.
        /// </summary>
        public override void Activate(bool instancePreserved)
        {
            if (!instancePreserved)
            {
                ContentManager content = ScreenManager.Game.Content;
                gradientTexture = content.Load<Texture2D>("gradient");
            }
        }


        #endregion

        #region Handle Input


        /// <summary>
        /// Responds to user input, accepting or cancelling the message box.
        /// </summary>
        public override void HandleInput(GameTime gameTime, InputState input)
        {
            PlayerIndex playerIndex;

            // We pass in our ControllingPlayer, which may either be null (to
            // accept input from any player) or a specific index. If we pass a null
            // controlling player, the InputState helper returns to us which player
            // actually provided the input. We pass that through to our Accepted and
            // Cancelled events, so they can tell which player triggered them.
            if (menuLocal.Evaluate(input, ControllingPlayer, out playerIndex))
            {
                // Raise the Local event, then exit the message box.
                if (Local != null)
                {
                    GameStateManagement.SideScrollGame.SideScrollGame.main.player = null;
                    if (GameStateManagement.SideScrollGame.SideScrollGame.main.IsNetwork)
                    {
                        GameStateManagement.SideScrollGame.SideScrollGame.main.client.Disconnect("exiting");
                        GameStateManagement.SideScrollGame.SideScrollGame.main.otherPlayers.Clear();
                    }

                    
                    Local(this, new PlayerIndexEventArgs(playerIndex));
                }
                ScreenManager.RemoveScreen(removeGameplayScreen);

                ScreenManager.AddScreen(new GameplayScreen(false), ControllingPlayer);
                ExitScreen();
            }
            else if (menuNetwork.Evaluate(input, ControllingPlayer, out playerIndex))
            {
                // Raise the Local event, then exit the message box.
                if (Network != null)
                {
                    GameStateManagement.SideScrollGame.SideScrollGame.main.player = null;
                    if (GameStateManagement.SideScrollGame.SideScrollGame.main.IsNetwork)
                    {
                        GameStateManagement.SideScrollGame.SideScrollGame.main.client.Disconnect("exiting");
                        GameStateManagement.SideScrollGame.SideScrollGame.main.otherPlayers.Clear();
                    }

                    Network(this, new PlayerIndexEventArgs(playerIndex));
                }
                ScreenManager.RemoveScreen(removeGameplayScreen);

                ScreenManager.AddScreen(new GameplayScreen(true), ControllingPlayer);
                ExitScreen();
            }
            else if (menuCancel.Evaluate(input, ControllingPlayer, out playerIndex))
            {
                if (Cancel != null)
                    Cancel(this, new PlayerIndexEventArgs(playerIndex));

                ExitScreen();
            }
        }


        #endregion

        #region Draw


        /// <summary>
        /// Draws the message box.
        /// </summary>
        public override void Draw(GameTime gameTime)
        {
            SpriteBatch spriteBatch = ScreenManager.SpriteBatch;
            SpriteFont font = ScreenManager.Font;

            // Darken down any other screens that were drawn beneath the popup.
            ScreenManager.FadeBackBufferToBlack(TransitionAlpha * 2 / 3);

            // Center the message text in the viewport.
            Viewport viewport = ScreenManager.GraphicsDevice.Viewport;
            Vector2 viewportSize = new Vector2(viewport.Width, viewport.Height);
            Vector2 textSize = font.MeasureString(message);
            Vector2 textPosition = (viewportSize - textSize) / 2;

            // The background includes a border somewhat larger than the text itself.
            const int hPad = 32;
            const int vPad = 16;

            Rectangle backgroundRectangle = new Rectangle((int)textPosition.X - hPad,
                                                          (int)textPosition.Y - vPad,
                                                          (int)textSize.X + hPad * 2,
                                                          (int)textSize.Y + vPad * 2);

            // Fade the popup alpha during transitions.
            Color color = Color.White * TransitionAlpha;

            spriteBatch.Begin();

            // Draw the background rectangle.
            spriteBatch.Draw(gradientTexture, backgroundRectangle, color);

            // Draw the message box text.
            spriteBatch.DrawString(font, message, textPosition, color);

            spriteBatch.End();
        }


        #endregion
    }
}
