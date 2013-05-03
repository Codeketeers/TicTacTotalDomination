using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TicTacTotalDomination.Util.Games
{
    public enum MoveResult { Failed, InvalidGame, NotPlayerTurn, InvalidOrigin, InvalidDestination, Valid }

    public class Move
    {
        public Move()
        {
            this.X = -1;
            this.Y = -1;
        }

        public int GameId { get; set; }
        public int PlayerId { get; set; }
        public int? OriginX { get; set; }
        public int? OriginY { get; set; }
        public int X { get; set; }
        public int Y { get; set; }
    }
}
