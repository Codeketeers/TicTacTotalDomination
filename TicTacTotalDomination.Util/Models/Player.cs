using System;
using System.Collections.Generic;

namespace TicTacTotalDomination.Util.Models
{
    public partial class Player
    {
        public Player()
        {
            this.Games = new List<Game>();
            this.Games1 = new List<Game>();
            this.Games2 = new List<Game>();
            this.GameMoves = new List<GameMove>();
        }

        public int PlayerId { get; set; }
        public string PlayerName { get; set; }
        public virtual ICollection<Game> Games { get; set; }
        public virtual ICollection<Game> Games1 { get; set; }
        public virtual ICollection<Game> Games2 { get; set; }
        public virtual ICollection<GameMove> GameMoves { get; set; }
    }
}
