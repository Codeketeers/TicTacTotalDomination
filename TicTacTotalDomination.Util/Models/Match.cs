using System;
using System.Collections.Generic;

namespace TicTacTotalDomination.Util.Models
{
    public partial class Match
    {
        public Match()
        {
            this.AIGames = new List<AIGame>();
            this.ConfigSections = new List<ConfigSection>();
            this.Games = new List<Game>();
        }

        public int MatchId { get; set; }
        public int NumberOfGames { get; set; }
        public System.DateTime CreateDate { get; set; }
        public Nullable<System.DateTime> WonDate { get; set; }
        public Nullable<System.DateTime> EndDate { get; set; }
        public System.DateTime StateDate { get; set; }
        public int NumberOfRounds { get; set; }
        public int PlayerOneId { get; set; }
        public Nullable<bool> PlayerOneAccepted { get; set; }
        public int PlayerTwoId { get; set; }
        public Nullable<bool> PlayerTwoAccepted { get; set; }
        public Nullable<int> WinningPlayerId { get; set; }
        public Nullable<int> CurrentGameId { get; set; }
        public virtual ICollection<AIGame> AIGames { get; set; }
        public virtual ICollection<ConfigSection> ConfigSections { get; set; }
        public virtual ICollection<Game> Games { get; set; }
        public virtual Game Game { get; set; }
        public virtual Player Player { get; set; }
        public virtual Player Player1 { get; set; }
        public virtual Player Player2 { get; set; }
    }
}
