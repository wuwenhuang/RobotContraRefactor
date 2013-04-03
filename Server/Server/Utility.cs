using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace XnaGameServer
{
    class Vector2
    {
        public float X;
        public float Y;

        public Vector2() : this(0, 0) { }
                
        public Vector2(float x, float y)
        {
            this.X = x;
            this.Y = y;
        }
    }
}
