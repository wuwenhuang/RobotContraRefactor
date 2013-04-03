using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;

namespace GameStateManagement.SideScrollGame
{
    abstract class AnimateSprite
    {
        protected Texture2D texture;
        protected int spriteWidth, spriteHeight;
        protected int frame;
        public Character character;

        private float timer = 0;
        public float interval = 0.1f;

        protected Rectangle sourceRect;

        public AnimateSprite()
        {

        }

        public AnimateSprite(Texture2D texture, int frame, int width, int height)
        {
            this.texture = texture;
            this.frame = frame;
            spriteWidth = width;
            spriteHeight = height;
            sourceRect = new Rectangle(0, 0, spriteWidth, spriteHeight);
        }

       
        public virtual void Update(GameTime gameTime)
        {
            timer += (float)gameTime.ElapsedGameTime.TotalSeconds;
            switch (character.currentState)
            {
                case CharacterState.IDLE:
                    if (character.lastState == CharacterState.MOVELEFT)
                        sourceRect = new Rectangle(1 * spriteWidth, 0, spriteWidth, spriteHeight);
                    else
                        sourceRect = new Rectangle(0, 0, spriteWidth, spriteHeight);
                    break;

                case CharacterState.MOVELEFT:
                    if (interval < timer)
                    {
                        if (frame > 3)
                        {
                            frame = 0;
                        }
                        else
                        {
                            frame += 1;
                        }

                        sourceRect = new Rectangle(frame * spriteWidth, 2 * spriteHeight, spriteWidth, spriteHeight);

                        timer = 0f;
                    }
                    break;

                case CharacterState.MOVERIGHT:

                    if (interval < timer)
                    {
                        if (frame > 3)
                        {
                            frame = 0;
                        }
                        else
                        {
                            frame += 1;
                        }

                        sourceRect = new Rectangle(frame * spriteWidth, 1 * spriteHeight, spriteWidth, spriteHeight);

                        timer = 0f;
                    }
                    
                    break;

                case CharacterState.MOVEUP:
                case CharacterState.MOVEDOWN:
                    if (interval < timer)
                    {
                        if (frame > 3)
                        {
                            frame = 0;
                        }
                        else
                        {
                            frame += 1;
                        }

                        timer = 0f;
                    }
                    if (character.lastState == CharacterState.MOVELEFT)
                        sourceRect = new Rectangle(frame * spriteWidth, 2 * spriteHeight, spriteWidth, spriteHeight);
                    else
                        sourceRect = new Rectangle(frame * spriteWidth, 1 * spriteHeight, spriteWidth, spriteHeight);
                    break;

                case CharacterState.SHOOT:
                    if (character.lastState == CharacterState.MOVELEFT)
                        sourceRect = new Rectangle(3 * spriteWidth, 0, spriteWidth, spriteHeight);
                    else
                        sourceRect = new Rectangle(2 * spriteWidth, 0, spriteWidth, spriteHeight);
                    break;

                case CharacterState.ATTACK:
                    if (interval < timer)
                    {
                        if (frame > 3)
                        {
                            frame = 0;
                        }
                        else
                        {
                            frame += 1;
                        }

                        timer = 0f;
                    }
                    if (character.lastState == CharacterState.MOVELEFT)
                        sourceRect = new Rectangle(frame * spriteWidth, 4 * spriteHeight, spriteWidth, spriteHeight);
                    else
                        sourceRect = new Rectangle(frame * spriteWidth, 3 * spriteHeight, spriteWidth, spriteHeight);
                    break;

                case CharacterState.BOOST:
                    if (character.lastState == CharacterState.MOVELEFT)
                        sourceRect = new Rectangle(5 * spriteWidth, 0, spriteWidth, spriteHeight);
                    else
                        sourceRect = new Rectangle(4 * spriteWidth, 0, spriteWidth, spriteHeight);
                    break;

                case CharacterState.JUMP:
                    if (interval < timer)
                    {
                        if (frame > 3)
                        {
                            frame = 0;
                        }
                        else
                        {
                            frame += 1;
                        }

                        if (character.lastState == CharacterState.MOVERIGHT)
                            sourceRect = new Rectangle(frame * spriteWidth, 3 * spriteHeight+spriteHeight/2, spriteWidth, spriteHeight-spriteHeight/2);
                        else
                            sourceRect = new Rectangle(frame * spriteWidth, 4 * spriteHeight + spriteHeight / 2, spriteWidth, spriteHeight - spriteHeight / 2);

                        timer = 0f;
                    }
                    break;
                    
                case CharacterState.DEAD:
                    if (interval < timer)
                    {
                        if (frame < 3)
                        {
                            frame += 1;
                        }

                        timer = 0f;
                    }
                    if (character.lastState == CharacterState.MOVERIGHT)
                        sourceRect = new Rectangle(frame * spriteWidth, 5 * spriteHeight, spriteWidth, spriteHeight);
                    else
                        sourceRect = new Rectangle(frame * spriteWidth, 6 * spriteHeight, spriteWidth, spriteHeight);
                    break;
            }
        }

        public Texture2D Texture
        {
            get { return this.texture; }
            set { this.texture = value; }
        }

        public Rectangle SourceRect
        {
            get { return this.sourceRect; }
        }
    }
}
