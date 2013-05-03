using System;
using System.Collections.Generic;

namespace TicTacTotalDomination.Util.Models
{
    public partial class Game
    {
        public Game()
        {
            this.AIGames = new List<AIGame>();
            this.CentralServerSessions = new List<CentralServerSession>();
            this.GameMoves = new List<GameMove>();
            this.Matches = new List<Match>();
        }

        public int GameId { get; set; }
        public int MatchId { get; set; }
        public int PlayerOneId { get; set; }
        public Nullable<bool> PlayerOneAccepted { get; set; }
        public int PlayerTwoId { get; set; }
        public Nullable<bool> PlayerTwoAccepted { get; set; }
        public Nullable<int> WinningPlayerId { get; set; }
        public Nullable<int> CurrentPlayerId { get; set; }
        public System.DateTime StateDate { get; set; }
        public System.DateTime CreateDate { get; set; }
        public Nullable<System.DateTime> WonDate { get; set; }
        public Nullable<System.DateTime> EndDate { get; set; }
        public bool DeathMatchMode { get; set; }
        public virtual ICollection<AIGame> AIGames { get; set; }
        public virtual ICollection<CentralServerSession> CentralServerSessions { get; set; }
        public virtual Player Player { get; set; }
        public virtual Match Match { get; set; }
        public virtual Player Player1 { get; set; }
        public virtual Player Player2 { get; set; }
        public virtual Player Player3 { get; set; }
        public virtual ICollection<GameMove> GameMoves { get; set; }
        public virtual ICollection<Match> Matches { get; set; }
    }
}
