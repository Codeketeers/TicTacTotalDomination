using System;
using System.Collections.Generic;

namespace TicTacTotalDomination.Util.Models
{
    public partial class GameMove
    {
        public int MoveId { get; set; }
        public int GameId { get; set; }
        public int PlayerId { get; set; }
        public System.DateTime MoveDate { get; set; }
        public bool IsSettingPiece { get; set; }
        public int X { get; set; }
        public int y { get; set; }
        public virtual Game Game { get; set; }
        public virtual Player Player { get; set; }
    }
}
