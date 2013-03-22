using System;
using System.Collections.Generic;

namespace TicTacTotalDomination.Util.Models
{
    public partial class Game
    {
        public Game()
        {
            this.GameMoves = new List<GameMove>();
        }

        public int GameId { get; set; }
        public int PlayerOneId { get; set; }
        public int PlayerTwoId { get; set; }
        public Nullable<int> WinningPlayerId { get; set; }
        public System.DateTime CreateDate { get; set; }
        public Nullable<System.DateTime> WonDate { get; set; }
        public virtual Player Player { get; set; }
        public virtual Player Player1 { get; set; }
        public virtual Player Player2 { get; set; }
        public virtual ICollection<GameMove> GameMoves { get; set; }
    }
}
