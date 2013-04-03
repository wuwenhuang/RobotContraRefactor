using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;

namespace GameStateManagement.SideScrollGame
{
    class Camera2D
    {
        public Rectangle rect;

        public static Camera2D main;

        public Camera2D()
        {
            main = this;
            rect = new Rectangle();
        }

        public void setPosition(Vector2 pos)
        {
            rect.X = (int)pos.X;
            rect.Y = (int)pos.Y;
        }

        public void setSize(int width, int height)
        {
            rect.Width = width;
            rect.Height = height;
        }

        public Vector2 WorldToScreenPoint(Vector2 worldPoint)
        {
            Vector2 cameraPos = new Vector2(this.rect.X, this.rect.Y);
            Vector2 resultPos = worldPoint - cameraPos;

            return resultPos;
        }

        public Vector2 getPosition()
        {
            Vector2 cameraPos = new Vector2(this.rect.X, this.rect.Y);

            return cameraPos;
        }
    }
}
