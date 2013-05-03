using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TicTacTotalDomination.Util.AI
{
    public class Move
    {
        public int X;
        public int Y;
        public int? OriginX = null;
        public int? OriginY = null;

        public void SetOrigin(int x, int y)
        {
            this.OriginX = x;
            this.OriginY = y;
        }

        public int GetX()
        {
            return X;
        }
        public void SetX(int xPos)
        {
            this.X = xPos;
        }
        public int GetY()
        {
            return Y;
        }
        public void SetY(int yPos)
        {
            this.Y = yPos;
        }

        public void Set(int x, int y)
        {
            this.X = x;
            this.Y = y;
        }
    }
}
