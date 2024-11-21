using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TicTacTotal
{
    class Move
    {
        public int x;
        public int y;

        public int getX()
        {
            return x;
        }
        public void setX(int xPos)
        {
            this.x = xPos;
        }
        public int getY()
        {
            return y;
        }
        public void setY(int yPos)
        {
            this.y = yPos;
        }

        public void set(int x, int y)
        {
            this.x = x;
            this.y = y;
        }
    }
}
