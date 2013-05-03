using System;
using System.Collections.Generic;

namespace TicTacTotalDomination.Util.Models
{
    public partial class Player
    {
        public Player()
        {
            this.AIGames = new List<AIGame>();
            this.Games = new List<Game>();
            this.Games1 = new List<Game>();
            this.Games2 = new List<Game>();
            this.Games3 = new List<Game>();
            this.GameMoves = new List<GameMove>();
            this.Matches = new List<Match>();
            this.Matches1 = new List<Match>();
            this.Matches2 = new List<Match>();
        }

        public int PlayerId { get; set; }
        public string PlayerName { get; set; }
        public virtual ICollection<AIGame> AIGames { get; set; }
        public virtual ICollection<Game> Games { get; set; }
        public virtual ICollection<Game> Games1 { get; set; }
        public virtual ICollection<Game> Games2 { get; set; }
        public virtual ICollection<Game> Games3 { get; set; }
        public virtual ICollection<GameMove> GameMoves { get; set; }
        public virtual ICollection<Match> Matches { get; set; }
        public virtual ICollection<Match> Matches1 { get; set; }
        public virtual ICollection<Match> Matches2 { get; set; }
    }
}
